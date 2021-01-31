using System.Collections.Generic;

namespace DemoAnalyzer.Data
{
    public class PlayerData
    {
        public PlayerData(int entityID)
        {
            EntityID = entityID;
        }

        public int EntityID { get; }

        public List<int> StatesTicks { get; } = new List<int>();
        public List<PlayerState> States { get; } = new List<PlayerState>();

        public List<int> PositionsTicks { get; } = new List<int>();
        public List<PlayerPosition> Positions { get; } = new List<PlayerPosition>();

        public List<int> StatisticsTicks { get; } = new List<int>();
        public List<PlayerStatistics> Statistics { get; } = new List<PlayerStatistics>();        
    }
}
