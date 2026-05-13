using UnityEngine;
using System.Collections.Generic;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSourcePrefab;
        [SerializeField] private UnityEngine.Audio.AudioMixer audioMixer;

        private readonly Dictionary<BgmTrack, AudioClip> bgmClips = new();
        private readonly Dictionary<SoundEffect, AudioClip> seClips = new();
        private readonly List<AudioSource> activeSeSources = new();

        private BgmTrack? currentBgmTrack;
        private BgmTrack? pendingBgmTrack;
        private BgmTrack? failedBgmTrack;
        private int bgmFadeInFramesRemaining;
        private int bgmFadeOutFramesRemaining;
        private bool isStoppingBgm;

        private void Awake()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
            }
        }

        public void InitializeAudio()
        {
            RegisterBgm(BgmTrack.MainMenu, "BGM/SFC_main_menu");
            RegisterBgm(BgmTrack.Prologue, "BGM/SFC_prologue02");
            RegisterBgm(BgmTrack.Field, "BGM/SFC_field");
            RegisterBgm(BgmTrack.Castle, "BGM/SFC_castle");
            RegisterBgm(BgmTrack.Battle, "BGM/SFC_battle");
            RegisterBgm(BgmTrack.Shop, "BGM/SFC_shop_buy");

            RegisterSe(SoundEffect.Dialog, "SE/Serif_SE");
            RegisterSe(SoundEffect.Collision, "SE/当たり判定SFC");
            RegisterSe(SoundEffect.Attack, "SE/SFC_atk1");
            RegisterSe(SoundEffect.Defend, "SE/SFC_def");
            RegisterSe(SoundEffect.Magic, "SE/SFC_magic");
            RegisterSe(SoundEffect.Cure, "SE/SFC_cure");
            RegisterSe(SoundEffect.Poison, "SE/SFC_poison");
            RegisterSe(SoundEffect.Raiden, "SE/SFC_raiden");
            RegisterSe(SoundEffect.Fire, "SE/SFC_fire");
            RegisterSe(SoundEffect.Equip, "SE/SFC_equip");
            RegisterSe(SoundEffect.Cursor, "SE/SFC_cursor");
            RegisterSe(SoundEffect.Cancel, "SE/SFC_cancel");
            RegisterSe(SoundEffect.Escape, "SE/SFC_escape");
        }

        private void RegisterBgm(BgmTrack track, string path)
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                bgmClips[track] = clip;
            }
        }

        private void RegisterSe(SoundEffect effect, string path)
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                seClips[effect] = clip;
            }
        }

        public void UpdateBgm()
        {
            var desiredTrack = GetDesiredBgmTrack();
            if (!bgmClips.TryGetValue(desiredTrack, out var clip))
            {
                StopBgm();
                currentBgmTrack = null;
                failedBgmTrack = desiredTrack;
                return;
            }

            if (currentBgmTrack == desiredTrack && bgmSource.isPlaying)
            {
                pendingBgmTrack = null;
                bgmFadeOutFramesRemaining = 0;
                UpdateBgmFade();
                return;
            }

            if (failedBgmTrack == desiredTrack)
            {
                return;
            }

            pendingBgmTrack = desiredTrack;

            if (!bgmSource.isPlaying)
            {
                PlayBgm(desiredTrack, clip);
                return;
            }

            if (bgmFadeOutFramesRemaining <= 0)
            {
                bgmFadeOutFramesRemaining = GameConstants.BgmFadeOutDurationFrames;
            }

            UpdateBgmFade();
        }

        private void PlayBgm(BgmTrack track, AudioClip clip)
        {
            StopBgm();
            bgmSource.clip = clip;
            bgmSource.volume = 0f;
            bgmSource.Play();
            currentBgmTrack = track;
            pendingBgmTrack = null;
            failedBgmTrack = null;
            bgmFadeInFramesRemaining = GameConstants.BgmFadeInDurationFrames;
            bgmFadeOutFramesRemaining = 0;
        }

        private void UpdateBgmFade()
        {
            if (bgmFadeOutFramesRemaining > 0)
            {
                var volume = GameConstants.BgmPlaybackVolume * bgmFadeOutFramesRemaining / GameConstants.BgmFadeOutDurationFrames;
                bgmSource.volume = Mathf.Clamp(volume, 0f, GameConstants.BgmPlaybackVolume);
                bgmFadeOutFramesRemaining--;

                if (bgmFadeOutFramesRemaining == 0)
                {
                    var nextTrack = pendingBgmTrack;
                    StopBgm();
                    if (nextTrack.HasValue && bgmClips.TryGetValue(nextTrack.Value, out var nextClip))
                    {
                        PlayBgm(nextTrack.Value, nextClip);
                    }
                }
                return;
            }

            if (bgmFadeInFramesRemaining > 0)
            {
                var elapsedFrames = GameConstants.BgmFadeInDurationFrames - bgmFadeInFramesRemaining + 1;
                var volume = GameConstants.BgmPlaybackVolume * elapsedFrames / GameConstants.BgmFadeInDurationFrames;
                bgmSource.volume = Mathf.Clamp(volume, 0f, GameConstants.BgmPlaybackVolume);
                bgmFadeInFramesRemaining--;
                return;
            }

            bgmSource.volume = GameConstants.BgmPlaybackVolume;
        }

        public void StopBgm()
        {
            if (isStoppingBgm) return;
            isStoppingBgm = true;
            bgmSource.Stop();
            bgmSource.clip = null;
            currentBgmTrack = null;
            pendingBgmTrack = null;
            failedBgmTrack = null;
            bgmFadeInFramesRemaining = 0;
            bgmFadeOutFramesRemaining = 0;
            isStoppingBgm = false;
        }

        public void PlaySe(SoundEffect effect)
        {
            if (!seClips.TryGetValue(effect, out var clip))
            {
                return;
            }

            var source = Instantiate(seSourcePrefab, transform);
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.clip = clip;
            source.volume = GameConstants.SePlaybackVolume;
            source.Play();
            activeSeSources.Add(source);

            Destroy(source.gameObject, clip.length + 0.1f);
        }

        public void StopAllSoundEffects()
        {
            foreach (var source in activeSeSources)
            {
                if (source != null)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                }
            }
            activeSeSources.Clear();
        }

        public void SetBgmVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
            }
        }

        public void SetSeVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("SEVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
            }
        }

        private BgmTrack GetDesiredBgmTrack()
        {
            var session = GameSession.Instance;
            if (session == null) return BgmTrack.MainMenu;

            var targetState = session.PendingGameState ?? session.CurrentGameState;
            return targetState switch
            {
                GameState.LanguageSelection => BgmTrack.Prologue,
                GameState.Battle => BgmTrack.Battle,
                GameState.ShopBuy => BgmTrack.Shop,
                GameState.Bank => BgmTrack.Shop,
                GameState.EncounterTransition => GetFieldBgmTrack(session.CurrentFieldMap),
                GameState.Field => GetFieldBgmTrack(session.CurrentFieldMap),
                _ => BgmTrack.MainMenu
            };
        }

        private static BgmTrack GetFieldBgmTrack(FieldMapId mapId)
        {
            return mapId switch
            {
                FieldMapId.Dungeon => BgmTrack.Castle,
                FieldMapId.Castle => BgmTrack.Castle,
                FieldMapId.Field => BgmTrack.Field,
                _ => BgmTrack.Field
            };
        }
    }
}