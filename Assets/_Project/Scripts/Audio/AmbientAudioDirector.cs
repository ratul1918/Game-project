using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    /// <summary>
    /// Manages the ambient audio soundscape for Harrow Rock:
    /// ocean waves, storm rain, distant lighthouse foghorn, and cottage radio static.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AmbientAudioDirector : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip oceanWavesClip;
        [SerializeField] private AudioClip rainStormClip;
        [SerializeField] private AudioClip foghornClip;
        [SerializeField] private AudioClip radioStaticClip;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float oceanVolume = 0.55f;
        [Range(0f, 1f)] [SerializeField] private float rainVolume = 0.40f;
        [Range(0f, 1f)] [SerializeField] private float foghornVolume = 0.65f;
        [Range(0f, 1f)] [SerializeField] private float radioVolume = 0.25f;

        [Header("Foghorn Timing")]
        [SerializeField] private float foghornInitialDelay = 5f;
        [SerializeField] private float foghornMinInterval = 22f;
        [SerializeField] private float foghornMaxInterval = 38f;

        private AudioSource _oceanSource;
        private AudioSource _rainSource;
        private AudioSource _foghornSource;
        private AudioSource _radioSource;
        private Coroutine _foghornRoutine;

        private void Awake()
        {
            SetupAudioSources();
        }

        private void Start()
        {
            StartAmbience();
        }

        private void OnDisable()
        {
            if (_foghornRoutine != null)
            {
                StopCoroutine(_foghornRoutine);
                _foghornRoutine = null;
            }
        }

        private void SetupAudioSources()
        {
            // 1. Ocean waves loop (ambient stereo)
            _oceanSource = gameObject.AddComponent<AudioSource>();
            _oceanSource.clip = oceanWavesClip;
            _oceanSource.loop = true;
            _oceanSource.playOnAwake = false;
            _oceanSource.volume = oceanVolume;
            _oceanSource.spatialBlend = 0f; // 2D ambient wrap
            _oceanSource.priority = 64;

            // 2. Rain storm loop (ambient stereo)
            _rainSource = gameObject.AddComponent<AudioSource>();
            _rainSource.clip = rainStormClip;
            _rainSource.loop = true;
            _rainSource.playOnAwake = false;
            _rainSource.volume = rainVolume;
            _rainSource.spatialBlend = 0f;
            _rainSource.priority = 64;

            // 3. Foghorn one-shot source (spatialized at lighthouse top)
            _foghornSource = gameObject.AddComponent<AudioSource>();
            _foghornSource.clip = foghornClip;
            _foghornSource.loop = false;
            _foghornSource.playOnAwake = false;
            _foghornSource.volume = foghornVolume;
            _foghornSource.spatialBlend = 0.35f; // Slight 3D presence
            _foghornSource.minDistance = 15f;
            _foghornSource.maxDistance = 120f;
            _foghornSource.priority = 32;

            // 4. Radio static source (in cottage)
            _radioSource = gameObject.AddComponent<AudioSource>();
            _radioSource.clip = radioStaticClip;
            _radioSource.loop = true;
            _radioSource.playOnAwake = false;
            _radioSource.volume = radioVolume;
            _radioSource.spatialBlend = 0.8f; // 3D positioned near radio
            _radioSource.minDistance = 1.5f;
            _radioSource.maxDistance = 18f;
            _radioSource.priority = 96;
        }

        public void StartAmbience()
        {
            if (_oceanSource && _oceanSource.clip && !_oceanSource.isPlaying)
            {
                _oceanSource.Play();
            }

            if (_rainSource && _rainSource.clip && !_rainSource.isPlaying)
            {
                _rainSource.Play();
            }

            if (_radioSource && _radioSource.clip && !_radioSource.isPlaying)
            {
                _radioSource.Play();
            }

            if (_foghornRoutine == null && _foghornSource && _foghornSource.clip)
            {
                _foghornRoutine = StartCoroutine(FoghornLoop());
            }
        }

        public void StopAmbience()
        {
            if (_oceanSource) _oceanSource.Stop();
            if (_rainSource) _rainSource.Stop();
            if (_radioSource) _radioSource.Stop();
            if (_foghornRoutine != null)
            {
                StopCoroutine(_foghornRoutine);
                _foghornRoutine = null;
            }
        }

        private IEnumerator FoghornLoop()
        {
            yield return new WaitForSeconds(foghornInitialDelay);

            while (true)
            {
                if (_foghornSource && _foghornSource.clip)
                {
                    _foghornSource.PlayOneShot(_foghornSource.clip, foghornVolume);
                }

                float waitTime = Random.Range(foghornMinInterval, foghornMaxInterval);
                yield return new WaitForSeconds(waitTime);
            }
        }

        public void ConfigureClips(AudioClip ocean, AudioClip rain, AudioClip foghorn, AudioClip radio)
        {
            oceanWavesClip = ocean;
            rainStormClip = rain;
            foghornClip = foghorn;
            radioStaticClip = radio;

            if (_oceanSource) _oceanSource.clip = ocean;
            if (_rainSource) _rainSource.clip = rain;
            if (_foghornSource) _foghornSource.clip = foghorn;
            if (_radioSource) _radioSource.clip = radio;
        }
    }
}
