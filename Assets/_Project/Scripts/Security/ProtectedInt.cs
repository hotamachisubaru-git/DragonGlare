using UnityEngine;
using System;

namespace DragonGlare.Security
{
    [Serializable]
    public struct ProtectedInt : IEquatable<ProtectedInt>, IComparable<ProtectedInt>
    {
        [SerializeField] private int obfuscatedValue;
        [SerializeField] private int checksum;

        private const int ObfuscationKey = 0x5A3C7F9E;

        public int Value
        {
            get
            {
                var value = obfuscatedValue ^ ObfuscationKey;
                if (checksum != CalculateChecksum(value))
                {
                    Debug.LogError("[ProtectedInt] Memory tampering detected!");
                    return 0;
                }
                return value;
            }
            set
            {
                obfuscatedValue = value ^ ObfuscationKey;
                checksum = CalculateChecksum(value);
            }
        }

        public ProtectedInt(int value)
        {
            obfuscatedValue = value ^ ObfuscationKey;
            checksum = CalculateChecksum(value);
        }

        private static int CalculateChecksum(int value)
        {
            return unchecked((int)((value * 0x12345678L) + 0x9ABCDEF0L));
        }

        public static implicit operator int(ProtectedInt protectedInt) => protectedInt.Value;
        public static implicit operator ProtectedInt(int value) => new ProtectedInt(value);

        public static ProtectedInt operator +(ProtectedInt a, ProtectedInt b) => new ProtectedInt(a.Value + b.Value);
        public static ProtectedInt operator -(ProtectedInt a, ProtectedInt b) => new ProtectedInt(a.Value - b.Value);
        public static ProtectedInt operator *(ProtectedInt a, ProtectedInt b) => new ProtectedInt(a.Value * b.Value);
        public static ProtectedInt operator /(ProtectedInt a, ProtectedInt b) => new ProtectedInt(a.Value / b.Value);

        public bool Equals(ProtectedInt other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ProtectedInt other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(ProtectedInt other) => Value.CompareTo(other.Value);
        public override string ToString() => Value.ToString();
    }
}
