using System;
using System.Collections.Generic;

namespace DemoAnalyzer.Data
{
    public struct PlayerStatistics : IEquatable<PlayerStatistics>
    {
        public int Kills;
        public int Deaths;
        public int Assists;
        public int Score;
        public int MVPs;
        public int Ping;
        public string Clantag;
        public int TotalCashSpent;

        public override bool Equals(object obj)
        {
            return obj is PlayerStatistics statistics && Equals(statistics);
        }

        public bool Equals(PlayerStatistics other)
        {
            return Kills == other.Kills &&
                   Deaths == other.Deaths &&
                   Assists == other.Assists &&
                   Score == other.Score &&
                   MVPs == other.MVPs &&
                   Ping == other.Ping &&
                   Clantag == other.Clantag &&
                   TotalCashSpent == other.TotalCashSpent;
        }

        public override int GetHashCode()
        {
            int hashCode = 658347853;
            hashCode = hashCode * -1521134295 + Kills.GetHashCode();
            hashCode = hashCode * -1521134295 + Deaths.GetHashCode();
            hashCode = hashCode * -1521134295 + Assists.GetHashCode();
            hashCode = hashCode * -1521134295 + Score.GetHashCode();
            hashCode = hashCode * -1521134295 + MVPs.GetHashCode();
            hashCode = hashCode * -1521134295 + Ping.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Clantag);
            hashCode = hashCode * -1521134295 + TotalCashSpent.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PlayerStatistics left, PlayerStatistics right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerStatistics left, PlayerStatistics right)
        {
            return !(left == right);
        }
    }
}
