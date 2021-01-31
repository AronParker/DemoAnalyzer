using DemoInfo;

namespace DemoAnalyzer.Data
{
    public struct RoundInfo
    {
        public int StartTick;
        public int EndTick;

        public int TimeLimit;
        public int FragLimit;
        public string Objective;

        public Team Winner;
        public RoundEndReason Reason;
        public string Message;
    }
}
