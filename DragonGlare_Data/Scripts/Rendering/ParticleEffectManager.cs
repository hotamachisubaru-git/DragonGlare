using UnityEngine;
using System.Collections;

namespace DragonGlare
{
    public class ParticleEffectManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem slashParticles;
        [SerializeField] private ParticleSystem spellParticles;
        [SerializeField] private ParticleSystem healParticles;
        [SerializeField] private ParticleSystem itemParticles;
        [SerializeField] private ParticleSystem defeatParticles;

        public void PlaySlash(Vector3 position)
        {
            PlayEffect(slashParticles, position);
        }

        public void PlaySpell(Vector3 position)
        {
            PlayEffect(spellParticles, position);
        }

        public void PlayHeal(Vector3 position)
        {
            PlayEffect(healParticles, position);
        }

        public void PlayItem(Vector3 position)
        {
            PlayEffect(itemParticles, position);
        }

        public void PlayDefeat(Vector3 position)
        {
            PlayEffect(defeatParticles, position);
        }

        private void PlayEffect(ParticleSystem particles, Vector3 position)
        {
            if (particles != null)
            {
                particles.transform.position = position;
                particles.Play();
            }
        }
    }
}
