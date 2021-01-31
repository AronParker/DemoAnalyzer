using DemoInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DemoAnalyzer
{
    public struct PlayerInfo
    {
        public Player Player;
        public DemoState.PlayerState State;

        public string Name => Player.Name;
        public Team Team => State.IsTerrorist ? Team.Terrorist : Team.CounterTerrorist;
        public bool IsDead => !State.IsAlive;
    }

    public class DemoState
    {
        private Dictionary<Player, PlayerData> _dict = new Dictionary<Player, PlayerData>();
        private PlayerKillData _playerKillData = new PlayerKillData();
        private List<RoundData> _rounds = new List<RoundData>();
        private int _lastRoundStart;

        // TODO implement correctly other potential ticks that might be even lower than player data
        public int MinTick => _dict.Min(x => x.Value.IngameTicks.First());
        public int MaxTick => _dict.Max(x => x.Value.IngameTicks.Last());
        public IReadOnlyList<RoundData> Rounds => _rounds;

        public void Parse(DemoParser parser)
        {
            _dict.Clear();
            _playerKillData.IngameTicks.Clear();
            _playerKillData.Kills.Clear();
            _rounds.Clear();

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

            parser.PlayerKilled += (sender, e) =>
            {
                _playerKillData.IngameTicks.Add(parser.IngameTick);
                _playerKillData.Kills.Add(e);
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
                // TODO
            }
        }

        public IEnumerable<PlayerInfo> ReadPlayerStates(int tick)
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

                yield return new PlayerInfo { Player = kvp.Key, State = state };
            }

            yield break;
        }

        public IEnumerable<PlayerKilledEventArgs> ReadRecentKills(int tick, int kills = 5)
        {
            var idx = _playerKillData.IngameTicks.BinarySearch(tick);

            if (idx < 0)
            {
                idx = ~idx;

                if (idx == 0)
                    yield break;

                idx--;
            }

            var lastIndexInclusive = idx;
            var firstIndexInclusive = Math.Max(lastIndexInclusive - kills + 1, 0);

            for (int i = firstIndexInclusive; i <= lastIndexInclusive; i++)
                yield return _playerKillData.Kills[i];
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

        public class PlayerKillData
        {
            public List<int> IngameTicks { get; } = new List<int>();
            public List<PlayerKilledEventArgs> Kills { get; } = new List<PlayerKilledEventArgs>();
        }

        public class PlayerData
        {
            public List<int> IngameTicks { get; } = new List<int>();
            public List<PlayerState> States { get; } = new List<PlayerState>();
        }
    }
}
