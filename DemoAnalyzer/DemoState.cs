using DemoInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAnalyzer
{
    public class DemoState
    {
        private Dictionary<Player, PlayerData> _dict = new Dictionary<Player, PlayerData>();
        private List<RoundData> _rounds = new List<RoundData>();
        private int _lastRoundStart;
        public int MinTick => _dict.Min(x => x.Value.IngameTicks.First());
        public int MaxTick => _dict.Max(x => x.Value.IngameTicks.Last());
        public IReadOnlyList<RoundData> Rounds => _rounds;

        public void Parse(DemoParser parser)
        {
            parser.PlayerDisconnect += (sender, e) =>
            {
                if (_dict.TryGetValue(e.Player, out var data))
                {
                    data.IngameTicks.Add(parser.IngameTick);
                    data.States.Add(new PlayerState { IsConnected = false });
                }
            };

            parser.RoundStart += (sender, e) =>
            {
                _lastRoundStart = parser.IngameTick;
            };

            parser.RoundEnd += (sender, e) =>
            {
                _rounds.Add(new RoundData { Start = _lastRoundStart, End = parser.IngameTick });
            };

            try
            {
                while (parser.ParseNextTick())
                {
                    foreach (var player in parser.PlayingParticipants)
                    {
                        PlayerData data;

                        if (!_dict.TryGetValue(player, out data))
                        {
                            data = new PlayerData();
                            _dict.Add(player, data);
                        }

                        var playerInfo = new PlayerState
                        {
                            IsConnected = true,
                            IsAlive = player.IsAlive,
                            IsTerrorist = player.Team == Team.Terrorist,
                            PositionX = player.IsAlive ? player.Position.X : player.LastAlivePosition.X,
                            PositionY = player.IsAlive ? player.Position.Y : player.LastAlivePosition.Y,
                            ViewDirectionX = player.ViewDirectionX
                        };

                        if (data.States.Count == 0 || !data.States.Last().Equals(playerInfo))
                        {
                            data.IngameTicks.Add(parser.IngameTick);
                            data.States.Add(playerInfo);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        public IEnumerable<KeyValuePair<Player, PlayerState>> ReadPlayerStates(int tick)
        {
            foreach (var kvp in _dict)
            {
                var idx = kvp.Value.IngameTicks.BinarySearch(tick);

                if (idx < 0)
                {
                    idx = ~idx;

                    if (idx == 0)
                        continue;

                    idx--;
                }

                var state = kvp.Value.States[idx];

                if (!state.IsConnected)
                    continue;

                yield return new KeyValuePair<Player, PlayerState>(kvp.Key, state);
            }

            yield break;
        }

        public struct RoundData
        {
            public int Start;
            public int End;
        }

        public struct PlayerState : IEquatable<PlayerState>
        {
            public bool IsConnected;
            public bool IsAlive;
            public bool IsTerrorist;
            public float PositionX;
            public float PositionY;
            public float ViewDirectionX;

            public bool Equals(PlayerState other)
            {
                if (IsConnected != other.IsConnected)
                    return false;

                if (IsAlive != other.IsAlive)
                    return false;

                if (IsTerrorist != other.IsTerrorist)
                    return false;

                if (PositionX != other.PositionX)
                    return false;

                if (PositionY != other.PositionY)
                    return false;

                if (ViewDirectionX != other.ViewDirectionX)
                    return false;

                return true;
            }
        }

        public class PlayerData
        {
            public List<PlayerState> States { get; } = new List<PlayerState>();
            public List<int> IngameTicks { get; } = new List<int>();
        }
    }
}
