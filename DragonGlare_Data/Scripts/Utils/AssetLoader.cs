using UnityEngine;

namespace DragonGlare
{
    public class AssetLoader : MonoBehaviour
    {
        public static T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public static T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }

        public static Sprite LoadSprite(string path)
        {
            return Load<Sprite>(path);
        }

        public static AudioClip LoadAudioClip(string path)
        {
            return Load<AudioClip>(path);
        }

        public static Font LoadFont(string path)
        {
            return Load<Font>(path);
        }
    }
}
