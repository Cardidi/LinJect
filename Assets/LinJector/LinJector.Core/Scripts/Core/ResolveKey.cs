using System;

namespace LinJector.Core
{
    internal struct ResolveKey
    {
        public object ContractId;

        public Type ContractType;

        public bool Equals(ResolveKey other)
        {
            return Equals(ContractId, other.ContractId) && ContractType == other.ContractType;
        }

        public override bool Equals(object obj)
        {
            return obj is ResolveKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ContractId, ContractType);
        }

        public static bool operator ==(ResolveKey left, ResolveKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResolveKey left, ResolveKey right)
        {
            return !left.Equals(right);
        }
    }
}