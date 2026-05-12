using UnityEngine;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class UIComponentTests
    {
        [Test]
        public void FadeOverlay_SetAlpha_ShouldUpdateColor()
        {
            var fadeOverlay = new GameObject().AddComponent<FadeOverlay>();
            var image = fadeOverlay.gameObject.AddComponent<UnityEngine.UI.Image>();
            fadeOverlay.GetType().GetField("overlayImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fadeOverlay, image);

            fadeOverlay.SetAlpha(0.5f);

            Assert.AreEqual(0.5f, image.color.a, 0.01f);
        }

        [Test]
        public void ColorPalette_ShouldHaveValidColors()
        {
            Assert.AreNotEqual(Color.black, ColorPalette.BattleTopWindowBackground);
            Assert.AreNotEqual(Color.clear, ColorPalette.SelectionHighlight);
        }

        [Test]
        public void RectExtensions_ToRect_ShouldConvertCorrectly()
        {
            var rectInt = new RectInt(10, 20, 100, 200);
            var rect = rectInt.ToRect();

            Assert.AreEqual(10f, rect.x);
            Assert.AreEqual(20f, rect.y);
            Assert.AreEqual(100f, rect.width);
            Assert.AreEqual(200f, rect.height);
        }

        [Test]
        public void MathUtils_Mod_ShouldHandleNegativeNumbers()
        {
            Assert.AreEqual(2, MathUtils.Mod(-1, 3));
            Assert.AreEqual(0, MathUtils.Mod(3, 3));
            Assert.AreEqual(1, MathUtils.Mod(4, 3));
        }

        [Test]
        public void StringUtils_SplitLines_ShouldHandleDifferentLineEndings()
        {
            var result1 = StringUtils.SplitLines("line1\nline2");
            var result2 = StringUtils.SplitLines("line1\r\nline2");

            Assert.AreEqual(2, result1.Length);
            Assert.AreEqual(2, result2.Length);
            Assert.AreEqual("line1", result1[0]);
            Assert.AreEqual("line2", result1[1]);
        }
    }
}
