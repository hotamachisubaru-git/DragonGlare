using UnityEngine;
using System.Collections.Generic;

namespace DragonGlare
{
    public class SpriteManager : MonoBehaviour
    {
        [SerializeField] private Sprite defaultHeroSprite;
        [SerializeField] private Sprite defaultNpcSprite;
        [SerializeField] private Sprite defaultEnemySprite;
        [SerializeField] private Sprite defaultPortraitSprite;
        [SerializeField] private Sprite defaultUiSprite;
        [SerializeField] private Sprite defaultTileSheet;
        [SerializeField] private Sprite defaultFieldMapImage;

        private readonly Dictionary<string, Sprite> npcSprites = new();
        private readonly Dictionary<string, Sprite> enemySprites = new();
        private readonly Dictionary<string, Sprite> npcPortraits = new();
        private readonly Dictionary<string, Sprite> uiImages = new();
        private readonly Dictionary<PlayerFacingDirection, Sprite> heroSprites = new();

        private Sprite mapTileSheet;
        private Sprite fieldMapImage;

        public void LoadFieldSprites()
        {
            LoadHeroSprites();
            mapTileSheet = LoadSprite("Sprites/TileSheets/mapTile_Assets_SFC") ?? defaultTileSheet;
            fieldMapImage = LoadSprite("Sprites/Maps/field_map") ?? defaultFieldMapImage;
        }

        private void LoadHeroSprites()
        {
            heroSprites[PlayerFacingDirection.Left] = LoadSprite("Sprites/Characters/hero_left") ?? defaultHeroSprite;
            heroSprites[PlayerFacingDirection.Right] = LoadSprite("Sprites/Characters/hero_right") ?? defaultHeroSprite;
            heroSprites[PlayerFacingDirection.Up] = LoadSprite("Sprites/Characters/hero_up") ?? defaultHeroSprite;
            heroSprites[PlayerFacingDirection.Down] = LoadSprite("Sprites/Characters/hero_down") ?? defaultHeroSprite;
        }

        public Sprite GetHeroSprite(PlayerFacingDirection direction)
        {
            if (heroSprites.TryGetValue(direction, out var sprite))
                return sprite;
            return defaultHeroSprite;
        }

        public Sprite GetNpcSprite(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            if (!npcSprites.TryGetValue(assetName, out var sprite))
            {
                sprite = LoadSprite($"Sprites/NPC/{assetName}");
                if (sprite != null)
                    npcSprites[assetName] = sprite;
            }
            return sprite ?? defaultNpcSprite;
        }

        public Sprite GetEnemySprite(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            if (!enemySprites.TryGetValue(assetName, out var sprite))
            {
                sprite = LoadSprite($"Sprites/Enemies/{assetName}");
                if (sprite != null)
                    enemySprites[assetName] = sprite;
            }
            return sprite ?? defaultEnemySprite;
        }

        public Sprite GetNpcPortrait(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            if (!npcPortraits.TryGetValue(assetName, out var sprite))
            {
                sprite = LoadSprite($"Portraits/NPC/{assetName}");
                if (sprite != null)
                    npcPortraits[assetName] = sprite;
            }
            return sprite ?? defaultPortraitSprite;
        }

        public Sprite GetUiImage(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            if (!uiImages.TryGetValue(assetName, out var sprite))
            {
                sprite = LoadSprite($"UI/{assetName}");
                if (sprite != null)
                    uiImages[assetName] = sprite;
            }
            return sprite ?? defaultUiSprite;
        }

        public Sprite GetMapTileSheet() => mapTileSheet;

        public Sprite GetFieldMapImage() => fieldMapImage;

        private static Sprite LoadSprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }
    }
}
