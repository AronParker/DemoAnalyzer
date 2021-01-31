﻿using DemoInfo;
using System.Collections.Generic;
using System.Linq;

namespace DemoAnalyzer.Data
{
    public class DemoData
    {
        private Dictionary<int, PlayerData> _playerData = new Dictionary<int, PlayerData>();
        private PlayerKillData _playerKills = new PlayerKillData();
        private List<RoundInfo> _rounds = new List<RoundInfo>();

        public int LastTick { get; }

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
        }
    }
}
