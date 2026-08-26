using System;

namespace SPFramework
{
    /// <summary>
    /// 游戏运行期间唯一的实体标识
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public static EntityId Invalid => default;

        public ulong Value { get; }

        public bool IsValid => Value != 0;

        internal EntityId(ulong value)
        {
            Value = value;
        }

        public bool Equals(EntityId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !left.Equals(right);
        }
    }
}
