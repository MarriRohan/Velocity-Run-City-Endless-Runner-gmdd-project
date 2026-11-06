using UnityEngine;

namespace NeonVelocity
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip slideClip;
        [SerializeField] private AudioClip orbCollectClip;
        [SerializeField] private AudioClip crashClip;
        [SerializeField] private AudioClip footstepClip;
        [SerializeField] private AudioClip uiClickClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSources();
        }

        private void EnsureAudioSources()
        {
            if (bgmSource == null)
            {
                var bgmObj = new GameObject("_BgmSource");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.volume = 0.65f;
            }

            if (sfxSource == null)
            {
                var sfxObj = new GameObject("_SfxSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = 0.85f;
            }
        }

        public void PlayBGM()
        {
            if (bgmSource != null && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void PlayJump()
        {
            PlaySFX(jumpClip, 0.9f);
        }

        public void PlaySlide()
        {
            PlaySFX(slideClip, 0.85f);
        }

        public void PlayOrbCollect()
        {
            PlaySFX(orbCollectClip, 0.8f, Random.Range(0.95f, 1.08f));
        }

        public void PlayCrash()
        {
            PlaySFX(crashClip, 1.0f);
        }

        public void PlayFootstep()
        {
            PlaySFX(footstepClip, 0.45f, Random.Range(0.9f, 1.1f));
        }

        public void PlayUIClick()
        {
            PlaySFX(uiClickClip, 0.8f);
        }

        private void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
