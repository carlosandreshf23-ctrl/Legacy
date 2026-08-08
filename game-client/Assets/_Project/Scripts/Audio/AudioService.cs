using UnityEngine;

namespace LegadoPeru.Audio
{
    /// <summary>
    /// Audio MVP (prompt §47): pasos, ambiente, interacción, UI. Los clips son opcionales;
    /// sin clips asignados el servicio no hace nada (no hay assets de audio en este entorno
    /// de desarrollo — ver PHASE_01_IMPLEMENTATION.md, limitaciones).
    /// </summary>
    public class AudioService
    {
        private readonly AudioSource sfxSource;
        private readonly AudioSource ambientSource;

        public AudioClip footstepClip;
        public AudioClip interactionClip;
        public AudioClip uiClip;

        public AudioService(AudioSource sfxSource, AudioSource ambientSource)
        {
            this.sfxSource = sfxSource;
            this.ambientSource = ambientSource;
        }

        public void PlayFootstep()
        {
            if (footstepClip != null) sfxSource.PlayOneShot(footstepClip, 0.6f);
        }

        public void PlayInteraction()
        {
            if (interactionClip != null) sfxSource.PlayOneShot(interactionClip);
        }

        public void PlayUI()
        {
            if (uiClip != null) sfxSource.PlayOneShot(uiClip);
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null || ambientSource == null) return;
            ambientSource.clip = clip;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }
}
