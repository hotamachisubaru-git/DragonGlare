using UnityEngine;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class SceneControllerTests
    {
        [Test]
        public void StartupOptionsController_ShouldInitializeCorrectly()
        {
            var controller = new GameObject().AddComponent<StartupOptionsController>();
            var session = new GameObject().AddComponent<GameSession>();
            session.Initialize();

            // Controller should not throw on enter
            controller.OnEnter();
            Assert.Pass();
        }

        [Test]
        public void ModeSelectController_ShouldInitializeCorrectly()
        {
            var controller = new GameObject().AddComponent<ModeSelectController>();
            var session = new GameObject().AddComponent<GameSession>();
            session.Initialize();

            controller.OnEnter();
            Assert.Pass();
        }

        [Test]
        public void FieldController_ShouldInitializeCorrectly()
        {
            var controller = new GameObject().AddComponent<FieldController>();
            var session = new GameObject().AddComponent<GameSession>();
            session.Initialize();

            controller.OnEnter();
            Assert.Pass();
        }
    }
}
