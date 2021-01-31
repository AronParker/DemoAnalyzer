using System.Collections.Generic;

namespace DemoAnalyzer.Data
{
    public class PlayerKillData
    {
        public List<int> KillsTicks { get; } = new List<int>();
        public List<PlayerKill> Kills { get; } = new List<PlayerKill>();
    }
}
