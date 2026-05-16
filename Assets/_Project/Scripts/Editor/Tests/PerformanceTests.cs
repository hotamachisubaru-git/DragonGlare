using UnityEngine;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class PerformanceTests
    {
        [Test]
        public void InputManager_PollInput_ShouldCompleteInFrameBudget()
        {
            var inputManager = new GameObject().AddComponent<InputManager>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                inputManager.PollInput();
            }

            stopwatch.Stop();
            var averageMs = stopwatch.ElapsedMilliseconds / 1000.0;

            Assert.Less(averageMs, 1.0, "Input polling should take less than 1ms per frame");
        }

        [Test]
        public void SaveManager_EncryptDecrypt_ShouldBeFast()
        {
            var saveManager = new GameObject().AddComponent<SaveManager>();
            var testData = new SaveData
            {
                Name = "PerformanceTest",
                Level = 99,
                Gold = 999999
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                saveManager.SaveSlot(1, testData);
            }

            stopwatch.Stop();
            var averageMs = stopwatch.ElapsedMilliseconds / 100.0;

            Assert.Less(averageMs, 10.0, "Save operation should take less than 10ms");
        }

        [Test]
        public void SpriteManager_LoadFieldSprites_ShouldCacheResults()
        {
            var spriteManager = new GameObject().AddComponent<SpriteManager>();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            spriteManager.LoadFieldSprites();
            var firstLoadMs = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            spriteManager.LoadFieldSprites();
            var secondLoadMs = stopwatch.ElapsedMilliseconds;

            Assert.LessOrEqual(secondLoadMs, firstLoadMs, "Second load should not be slower due to caching");
        }
    }
}
