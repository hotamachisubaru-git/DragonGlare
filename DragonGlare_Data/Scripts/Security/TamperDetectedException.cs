using System;

namespace DragonGlare.Security
{
    public class TamperDetectedException : Exception
    {
        public TamperDetectedException() : base("Memory tampering detected!") { }
        public TamperDetectedException(string message) : base(message) { }
        public TamperDetectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
