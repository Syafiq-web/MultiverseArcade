using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Audio Manager global untuk mengatur Mixer / AudioSource (Master, Music, SFX),
    /// pemutaran BGM berulang, efek UI Click & Hover, serta pause SFX saat jeda game.
    /// Sesuai GDD Bagian 2.2.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Default UI Clips")]
        [SerializeField] private AudioClip _clickClip;
        [SerializeField] private AudioClip _hoverClip;
        [SerializeField] private AudioClip _defaultMenuMusic;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float _masterVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 0.9f;

        private Coroutine _fadeMusicRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSources();
        }

        private void OnEnable()
        {
            EventBus.OnGamePaused += HandleGamePaused;
            EventBus.OnGameResumed += HandleGameResumed;
        }

        private void OnDisable()
        {
            EventBus.OnGamePaused -= HandleGamePaused;
            EventBus.OnGameResumed -= HandleGameResumed;
        }

        private void InitializeSources()
        {
            if (_musicSource == null)
            {
                GameObject musicObj = new GameObject("Music_Source");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX_Source");
                sfxObj.transform.SetParent(transform);
                _sfxSource = sfxObj.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        private void HandleGamePaused()
        {
            if (_sfxSource != null && _sfxSource.isPlaying)
            {
                _sfxSource.Pause();
            }
        }

        private void HandleGameResumed()
        {
            if (_sfxSource != null)
            {
                _sfxSource.UnPause();
            }
        }

        public void PlayBGM(AudioClip clip) => PlayMusic(clip, true, 0.5f);
        public void StopBGM() => StopMusic();

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.5f)
        {
            if (clip == null || _musicSource == null) return;
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            if (_fadeMusicRoutine != null) StopCoroutine(_fadeMusicRoutine);
            _fadeMusicRoutine = StartCoroutine(FadeToNewMusicRoutine(clip, loop, fadeDuration));
        }

        public void PlayDefaultMenuMusic()
        {
            if (_defaultMenuMusic != null)
            {
                PlayMusic(_defaultMenuMusic, true, 0.5f);
            }
        }

        private IEnumerator FadeToNewMusicRoutine(AudioClip newClip, bool loop, float duration)
        {
            float startVol = _musicSource.volume;
            float targetVol = _musicVolume * _masterVolume;

            if (_musicSource.isPlaying && duration > 0f)
            {
                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                    yield return null;
                }
            }

            _musicSource.clip = newClip;
            _musicSource.loop = loop;
            _musicSource.Play();

            if (duration > 0f)
            {
                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(0f, targetVol, t / duration);
                    yield return null;
                }
            }

            _musicSource.volume = targetVol;
            _fadeMusicRoutine = null;
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale * _sfxVolume * _masterVolume));
        }

        public void PlayButtonClick()
        {
            if (_clickClip != null)
            {
                PlaySFX(_clickClip, 1f);
            }
        }

        public void PlayButtonHover()
        {
            if (_hoverClip != null)
            {
                PlaySFX(_hoverClip, 0.7f);
            }
        }

        public void SetMasterVolume(float vol)
        {
            _masterVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        public void SetMusicVolume(float vol)
        {
            _musicVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        public void SetSFXVolume(float vol)
        {
            _sfxVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (_musicSource != null) _musicSource.volume = _musicVolume * _masterVolume;
            if (_sfxSource != null) _sfxSource.volume = _sfxVolume * _masterVolume;
        }

        public void SetAudioClips(AudioClip click, AudioClip hover, AudioClip defaultMenuMusic)
        {
            _clickClip = click;
            _hoverClip = hover;
            _defaultMenuMusic = defaultMenuMusic;
        }
    }
}
