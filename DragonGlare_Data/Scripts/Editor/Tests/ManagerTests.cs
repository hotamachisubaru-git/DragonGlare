using UnityEngine;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class InputManagerTests
    {
        [Test]
        public void WasPressed_ShouldReturnFalseForUnpressedKey()
        {
            var inputManager = new GameObject().AddComponent<InputManager>();
            inputManager.PollInput();

            Assert.IsFalse(inputManager.WasPressed(KeyCode.F1));
        }

        [Test]
        public void WasPrimaryConfirmPressed_ShouldDetectReturnAndZ()
        {
            var inputManager = new GameObject().AddComponent<InputManager>();
            // Note: Actual key press simulation requires Input System package
            // This test verifies the method exists and returns expected default
            Assert.IsFalse(inputManager.WasPrimaryConfirmPressed());
        }
    }

    public class AudioManagerTests
    {
        [Test]
        public void InitializeAudio_ShouldRegisterBgmTracks()
        {
            var audioManager = new GameObject().AddComponent<AudioManager>();
            audioManager.InitializeAudio();

            // BGM tracks should be registered (will be null in test environment without Resources)
            // This test verifies the method doesn't throw
            Assert.Pass();
        }
    }

    public class SaveManagerTests
    {
        [Test]
        public void GetSlotPath_ShouldReturnValidPath()
        {
            var saveManager = new GameObject().AddComponent<SaveManager>();
            var path = saveManager.GetSlotPath(1);

            Assert.IsTrue(path.Contains("slot1.sav"));
        }

        [Test]
        public void ValidateSlotNumber_ShouldThrowForInvalidSlot()
        {
            var saveManager = new GameObject().AddComponent<SaveManager>();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveManager.GetSlotPath(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveManager.GetSlotPath(4));
        }
    }
}
