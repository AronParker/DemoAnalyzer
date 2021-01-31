﻿using DemoInfo;
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

        public void Parse(DemoParser parser)
        {
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
                    KillerId = e.Killer.EntityID,
                    KillerName = e.Killer.Name,
                    KillerTeam = e.Killer.Team,
                    PenetratedObjects = e.PenetratedObjects,
                    Headshot = e.Headshot,
                    AssistedFlash = e.AssistedFlash
                };

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
    }
}