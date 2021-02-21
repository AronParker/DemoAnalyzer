using DemoInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoAnalyzer.Data
{
    public class DemoData
    {
        private Dictionary<int, PlayerData> _playerData = new Dictionary<int, PlayerData>();
        private PlayerKillData _playerKills = new PlayerKillData();
        private List<RoundInfo> _rounds = new List<RoundInfo>();

        public IReadOnlyList<RoundInfo> Rounds => _rounds;

        public int LastTick { get; private set; }
        public string MapName { get; private set; }
        public double MinimapOffsetX { get; private set; }
        public double MinimapOffsetY { get; private set; }
        public double MinimapScale { get; private set; }

        public void Reset()
        {
            _playerData.Clear();
            _playerKills.Kills.Clear();
            _playerKills.KillsTicks.Clear();
            _rounds.Clear();

            LastTick = 0;
            MapName = null;
            MinimapOffsetX = 0.0;
            MinimapOffsetY = 0.0;
            MinimapScale = 0.0;
        }

        public void Parse(DemoParser parser)
        {
            Reset();

            LoadMap(parser.Header.MapName);

            _playerData.Clear();
            _playerKills.Kills.Clear();
            _playerKills.KillsTicks.Clear();
            _rounds.Clear();

            var currentRoundInfo = new RoundInfo();

            parser.RoundStart += (sender, e) =>
            {
                currentRoundInfo.StartTick = parser.IngameTick;
                currentRoundInfo.TimeLimit = e.TimeLimit;
                currentRoundInfo.FragLimit = e.FragLimit;
                currentRoundInfo.Objective = e.Objective;
            };

            parser.RoundEnd += (sender, e) =>
            {
                currentRoundInfo.EndTick = parser.IngameTick;
                currentRoundInfo.Winner = e.Winner;
                currentRoundInfo.Reason = e.Reason;
                currentRoundInfo.Message = e.Message;
                _rounds.Add(currentRoundInfo);
            };

            parser.PlayerKilled += (sender, e) =>
            {
                var pk = new PlayerKill
                {
                    Weapon = e.Weapon,
                    VictimId = e.Victim.EntityID,
                    VictimName = e.Victim.Name,
                    VictimTeam = e.Victim.Team,
                    PenetratedObjects = e.PenetratedObjects,
                    Headshot = e.Headshot,
                    AssistedFlash = e.AssistedFlash
                };

                if (e.Killer != null)
                {
                    pk.KillerId = e.Killer.EntityID;
                    pk.KillerName = e.Killer.Name;
                    pk.KillerTeam = e.Killer.Team;
                }

                if (e.Assister != null)
                {
                    pk.AssisterId = e.Assister.EntityID;
                    pk.AssisterName = e.Assister.Name;
                    pk.AssisterTeam = e.Assister.Team;
                }

                _playerKills.KillsTicks.Add(parser.IngameTick);
                _playerKills.Kills.Add(pk);
            };

            parser.PlayerDisconnect += (sender, e) =>
            {
                if (_playerData.TryGetValue(e.Player.EntityID, out var data))
                {
                    data.StatesTicks.Add(parser.IngameTick);
                    data.States.Add(new PlayerState { IsConnected = false, Name = e.Player.Name });
                }
            };

            try
            {
                while (parser.ParseNextTick())
                {
                    foreach (var player in parser.Participants)
                    {
                        PlayerData data;

                        if (!_playerData.TryGetValue(player.EntityID, out data))
                        {
                            data = new PlayerData(player.EntityID);
                            _playerData.Add(player.EntityID, data);
                        }

                        var currentState = new PlayerState
                        {
                            IsConnected = true,
                            IsAlive = player.IsAlive,
                            Team = player.Team,
                            Name = player.Name
                        };

                        if (data.States.Count == 0 || data.States.Last() != currentState)
                        {
                            data.StatesTicks.Add(parser.IngameTick);
                            data.States.Add(currentState);
                        }

                        if (player.IsAlive)
                        {
                            var currentPos = new PlayerPosition
                            {
                                PositionX = player.Position.X,
                                PositionY = player.Position.Y,
                                PositionZ = player.Position.Z,
                                ViewDirectionX = player.ViewDirectionX,
                                ViewDirectionY = player.ViewDirectionY,
                            };

                            if (data.Positions.Count == 0 || data.Positions.Last() != currentPos)
                            {
                                data.PositionsTicks.Add(parser.IngameTick);
                                data.Positions.Add(currentPos);
                            }
                        }

                        var currentStats = new PlayerStatistics
                        {
                            Kills = player.AdditionaInformations.Kills,
                            Deaths = player.AdditionaInformations.Deaths,
                            Assists = player.AdditionaInformations.Assists,
                            Score = player.AdditionaInformations.Score,
                            MVPs = player.AdditionaInformations.MVPs,
                            Ping = player.AdditionaInformations.Ping,
                            Clantag = player.AdditionaInformations.Clantag,
                            TotalCashSpent = player.AdditionaInformations.TotalCashSpent,
                        };

                        if (data.Statistics.Count == 0 || data.Statistics.Last() != currentStats)
                        {
                            data.StatisticsTicks.Add(parser.IngameTick);
                            data.Statistics.Add(currentStats);
                        }
                    }
                }
            }
            catch
            {

            }

            LastTick = parser.IngameTick;
        }

        public System.Windows.Vector WorldSpaceToMinimapSpace(System.Windows.Vector worldSpace)
        {
            var distanceFromTopLeft = new System.Windows.Vector(worldSpace.X - MinimapOffsetX, MinimapOffsetY - worldSpace.Y);

            return distanceFromTopLeft / MinimapScale;
        }

        public System.Windows.Vector MinimapSpaceToWorldSpace(System.Windows.Vector minimapSpace)
        {
            minimapSpace *= MinimapScale;

            return new System.Windows.Vector(minimapSpace.X + MinimapOffsetX, -minimapSpace.Y + MinimapOffsetY);
        }

        public IEnumerable<PlayerInfo> ReadPlayerInfos(int tick)
        {
            foreach (var kvp in _playerData)
            {
                var stateIdx = GetLatestTickIndex(kvp.Value.StatesTicks, tick);

                if (stateIdx == -1)
                    continue;

                var playerInfo = new PlayerInfo();
                playerInfo.EntityID = kvp.Value.EntityID;
                playerInfo.State = kvp.Value.States[stateIdx];

                if (!playerInfo.State.IsConnected)
                    continue;

                if (kvp.Value.Positions.Count > 0)
                {
                    var posIdx = GetLatestTickIndex(kvp.Value.PositionsTicks, tick);

                    if (posIdx != -1)
                        playerInfo.Position = kvp.Value.Positions[posIdx];
                }

                if (kvp.Value.Statistics.Count > 0)
                {
                    var statsIdx = GetLatestTickIndex(kvp.Value.StatisticsTicks, tick);

                    if (statsIdx != -1)
                        playerInfo.Statistics = kvp.Value.Statistics[statsIdx];
                }

                yield return playerInfo;
            }

            yield break;
        }

        public IEnumerable<PlayerKill> ReadRecentKills(int tick, int number = 5)
        {
            if (number < 1)
                throw new ArgumentOutOfRangeException(nameof(number));

            var lastIndexInclusive = GetLatestTickIndex(_playerKills.KillsTicks, tick);

            if (lastIndexInclusive == -1)
                yield break;

            var firstIndexInclusive = Math.Max(lastIndexInclusive - number + 1, 0);

            for (int i = firstIndexInclusive; i <= lastIndexInclusive; i++)
                yield return _playerKills.Kills[i];
        }

        private static int GetLatestTickIndex(List<int> tickList, int tick)
        {
            var idx = tickList.BinarySearch(tick);

            if (idx >= 0)
                return idx;

            return ~idx - 1;
        }

        private void LoadMap(string mapName)
        {
            switch (mapName)
            {
                case "de_cache":
                    MinimapOffsetX = -2000;
                    MinimapOffsetY = 3250;
                    MinimapScale = 5.5;
                    break;
                case "de_dust2":
                    MinimapOffsetX = -2476;
                    MinimapOffsetY = 3239;
                    MinimapScale = 4.4;
                    break;
                case "de_inferno":
                    MinimapOffsetX = -2087;
                    MinimapOffsetY = 3870;
                    MinimapScale = 4.9;
                    break;
                case "de_mirage":
                    MinimapOffsetX = -3230;
                    MinimapOffsetY = 1713;
                    MinimapScale = 5.00;
                    break;
                case "de_nuke":
                    MinimapOffsetX = -3453;
                    MinimapOffsetY = 2887;
                    MinimapScale = 7;
                    break;
                case "de_train":
                    MinimapOffsetX = -2477;
                    MinimapOffsetY = 2392;
                    MinimapScale = 4.7;
                    break;
                case "de_vertigo":
                    MinimapOffsetX = -3168;
                    MinimapOffsetY = 1762;
                    MinimapScale = 4.0;
                    break;
                default:
                    throw new DemoDataException("Unsupported map.");
            }

            MapName = mapName;
        }
    }
}
