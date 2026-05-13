using UnityEngine;

namespace DragonGlare
{
    public class SceneControllerBase : MonoBehaviour
    {
        protected GameSession Session => GameSession.Instance;
        protected InputManager Input => GameManager.Instance.Input;
        protected AudioManager Audio => GameManager.Instance.Audio;

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }

        protected void PlayCursorSe()
        {
            Audio.PlaySe(SoundEffect.Cursor);
        }

        protected void PlayCancelSe()
        {
            Audio.PlaySe(SoundEffect.Cancel);
        }

        protected void PlayConfirmSe()
        {
            Audio.PlaySe(SoundEffect.Dialog);
        }
    }
}
