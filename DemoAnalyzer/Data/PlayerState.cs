using DemoInfo;
using System;
using System.Collections.Generic;

namespace DemoAnalyzer.Data
{
    public struct PlayerState : IEquatable<PlayerState>
    {
        public bool IsConnected;
        public bool IsAlive;
        public Team Team;
        public string Name;

        public override bool Equals(object obj)
        {
            return obj is PlayerState state && Equals(state);
        }

        public bool Equals(PlayerState other)
        {
            return IsConnected == other.IsConnected &&
                   IsAlive == other.IsAlive &&
                   Team == other.Team &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 1111309928;
            hashCode = hashCode * -1521134295 + IsConnected.GetHashCode();
            hashCode = hashCode * -1521134295 + IsAlive.GetHashCode();
            hashCode = hashCode * -1521134295 + Team.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static bool operator ==(PlayerState left, PlayerState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerState left, PlayerState right)
        {
            return !(left == right);
        }
    }
}
