using System;

namespace DemoAnalyzer.Data
{
    public struct PlayerPosition : IEquatable<PlayerPosition>
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float ViewDirectionX;
        public float ViewDirectionY;

        public override bool Equals(object obj)
        {
            return obj is PlayerPosition position && Equals(position);
        }

        public bool Equals(PlayerPosition other)
        {
            return PositionX == other.PositionX &&
                   PositionY == other.PositionY &&
                   PositionZ == other.PositionZ &&
                   ViewDirectionX == other.ViewDirectionX &&
                   ViewDirectionY == other.ViewDirectionY;
        }

        public override int GetHashCode()
        {
            int hashCode = 308736946;
            hashCode = hashCode * -1521134295 + PositionX.GetHashCode();
            hashCode = hashCode * -1521134295 + PositionY.GetHashCode();
            hashCode = hashCode * -1521134295 + PositionZ.GetHashCode();
            hashCode = hashCode * -1521134295 + ViewDirectionX.GetHashCode();
            hashCode = hashCode * -1521134295 + ViewDirectionY.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PlayerPosition left, PlayerPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerPosition left, PlayerPosition right)
        {
            return !(left == right);
        }
    }
}
