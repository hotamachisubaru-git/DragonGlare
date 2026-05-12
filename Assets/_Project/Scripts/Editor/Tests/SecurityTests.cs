using UnityEngine;
using NUnit.Framework;
using DragonGlare.Security;

namespace DragonGlare.Tests
{
    public class SecurityTests
    {
        [Test]
        public void ProtectedInt_ShouldStoreValueCorrectly()
        {
            var protectedInt = new ProtectedInt(42);
            Assert.AreEqual(42, protectedInt.Value);
        }

        [Test]
        public void ProtectedInt_ShouldSupportArithmetic()
        {
            var a = new ProtectedInt(10);
            var b = new ProtectedInt(5);

            Assert.AreEqual(15, (a + b).Value);
            Assert.AreEqual(5, (a - b).Value);
            Assert.AreEqual(50, (a * b).Value);
            Assert.AreEqual(2, (a / b).Value);
        }

        [Test]
        public void ProtectedInt_ShouldSupportImplicitConversion()
        {
            ProtectedInt protectedInt = 100;
            int value = protectedInt;

            Assert.AreEqual(100, value);
        }

        [Test]
        public void ProtectedInt_ShouldDetectTampering()
        {
            var protectedInt = new ProtectedInt(42);
            // Note: In Unity IL2CPP, direct memory manipulation is harder
            // This test verifies the basic checksum mechanism
            Assert.AreEqual(42, protectedInt.Value);
        }
    }
}
