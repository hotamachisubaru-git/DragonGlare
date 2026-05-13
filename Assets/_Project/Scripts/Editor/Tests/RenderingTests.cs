using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class RenderingTests
    {
        [Test]
        public void TilemapRenderer_Initialize_ShouldCreateCorrectGrid()
        {
            var go = new GameObject("Tilemap");
            var renderer = go.AddComponent<TilemapRenderer>();
            var grid = go.AddComponent<GridLayoutGroup>();
            renderer.GetType().GetField("grid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(renderer, grid);
            renderer.GetType().GetField("tilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(renderer, new GameObject("TilePrefab"));

            renderer.Initialize(10, 10);

            Assert.AreEqual(100, grid.transform.childCount);
        }

        [Test]
        public void SpriteAnimator_SetFrames_ShouldUpdateCurrentFrame()
        {
            var go = new GameObject("SpriteAnimator");
            var animator = go.AddComponent<SpriteAnimator>();
            var image = go.AddComponent<Image>();
            animator.GetType().GetField("targetImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(animator, image);

            var frames = new[]
            {
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero),
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero)
            };

            animator.SetFrames(frames);

            Assert.AreEqual(frames[0], image.sprite);
        }

        [Test]
        public void SceneTransition_FadeIn_ShouldSetTargetAlpha()
        {
            var go = new GameObject("Transition");
            var transition = go.AddComponent<SceneTransition>();
            var canvasGroup = go.AddComponent<CanvasGroup>();
            transition.GetType().GetField("fadeGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(transition, canvasGroup);

            transition.FadeIn();

            Assert.AreEqual(0f, transition.GetType().GetField("targetAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(transition));
        }

        [Test]
        public void BattleEffectAnimator_ClearAll_ShouldDisableAllEffects()
        {
            var go = new GameObject("BattleEffects");
            var animator = go.AddComponent<BattleEffectAnimator>();

            // Create effect objects
            var slash = new GameObject("Slash");
            slash.AddComponent<Image>();
            animator.GetType().GetField("slashImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(animator, slash.GetComponent<Image>());

            var spell = new GameObject("Spell");
            spell.AddComponent<Image>();
            animator.GetType().GetField("spellBurstImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(animator, spell.GetComponent<Image>());

            animator.ClearAll();

            Assert.IsFalse(slash.GetComponent<Image>().gameObject.activeSelf);
            Assert.IsFalse(spell.GetComponent<Image>().gameObject.activeSelf);
        }
    }
}
