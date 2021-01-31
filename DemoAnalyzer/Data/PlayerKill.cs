using DemoInfo;

namespace DemoAnalyzer.Data
{
    public struct PlayerKill
    {
        public Equipment Weapon;
        public int VictimId;
        public string VictimName;
        public Team VictimTeam;
        public int KillerId;
        public string KillerName;
        public Team KillerTeam;
        public int AssisterId;
        public string AssisterName;
        public Team AssisterTeam;
        public int PenetratedObjects;
        public bool Headshot;
        public bool AssistedFlash;
    }
}
