using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TCS {
    public class AudioSettings : MonoBehaviour {
        [SerializeField] AudioMixerGroup m_masterMixerGroup;
        [SerializeField] AudioMixerGroup m_musicMixerGroup;
        [SerializeField] AudioMixerGroup m_sfxMixerGroup;
        
        const string MASTER_VOLUME_PARAMETER = "MasterVolume";
        const string MUSIC_VOLUME_PARAMETER = "MusicVolume";
        const string SFX_VOLUME_PARAMETER = "SFXVolume";
    }

    public class AudioExample : MonoBehaviour {
        [Header("Audio Settings")]
        [SerializeField] List<AudioClip> m_audioClips = new();

        [SerializeField] bool m_loop;

        [SerializeField, Range(0f, 5f)] float m_fadeDuration = 1f;

        [SerializeField] AudioSource m_audioSourceA;
        [SerializeField] AudioSource m_audioSourceB;
        AudioSource m_activeSource;
        int m_currentClipIndex;
        bool m_isFading;

        Coroutine m_currentFadeCoroutine;

        void Awake() {
            ConfigureAudioSource(m_audioSourceA);
            ConfigureAudioSource(m_audioSourceB);

            m_activeSource = m_audioSourceA;

            if (m_audioClips.Count > 0) {
                m_activeSource.clip = m_audioClips[m_currentClipIndex];
                m_activeSource.volume = 1f; // Ensure the first source is audible
            }
            else {
                Debug.LogWarning("No audio clips assigned.");
            }
        }

        void ConfigureAudioSource(AudioSource source) {
            if (!source) {
                Debug.LogWarning("Audio source is null.");
                return;
            }

            source.loop = m_loop;
            source.playOnAwake = false;
            source.volume = 0f;
        }

        public void PlayAudio() {
            if (!m_activeSource.clip) {
                Debug.LogWarning("No audio clip to play.");
                return;
            }

            if (m_activeSource.isPlaying) {
                Debug.LogWarning("Audio is already playing.");
                return;
            }

            m_activeSource.volume = 0f;
            m_activeSource.Play();
            m_currentFadeCoroutine = StartCoroutine
            (
                FadeVolume
                (
                    m_activeSource,
                    0f,
                    1f,
                    m_fadeDuration
                )
            );
        }

        public void PauseAudio() {
            if (m_activeSource.isPlaying) {
                m_activeSource.Pause();
            }
        }

        public void StopAudio() {
            if (!m_activeSource.isPlaying) return;
            if (m_currentFadeCoroutine != null) {
                StopCoroutine(m_currentFadeCoroutine);
            }

            m_currentFadeCoroutine = StartCoroutine(FadeVolume(m_activeSource, m_activeSource.volume, 0f, m_fadeDuration, () => m_activeSource.Stop()));
        }

        public void NextAudio() {
            if (m_audioClips.Count == 0) {
                Debug.LogWarning("No audio clips available.");
                return;
            }

            m_currentClipIndex = (m_currentClipIndex + 1) % m_audioClips.Count;
            CrossfadeToClip(m_audioClips[m_currentClipIndex]);
        }

        public void PreviousAudio() {
            if (m_audioClips.Count == 0) {
                Debug.LogWarning("No audio clips available.");
                return;
            }

            m_currentClipIndex = (m_currentClipIndex - 1 + m_audioClips.Count) % m_audioClips.Count;
            CrossfadeToClip(m_audioClips[m_currentClipIndex]);
        }

        public void SetAudioClip(AudioClip clip) {
            if (!clip) {
                Debug.LogWarning("Provided audio clip is null.");
                return;
            }

            if (!m_audioClips.Contains(clip)) {
                m_audioClips.Add(clip);
            }

            m_currentClipIndex = m_audioClips.IndexOf(clip);
            CrossfadeToClip(clip);
        }

        public void SetAudioClip(int index) {
            if (index < 0 || index >= m_audioClips.Count) {
                Debug.LogWarning("Index out of range.");
                return;
            }

            m_currentClipIndex = index;
            CrossfadeToClip(m_audioClips[m_currentClipIndex]);
        }

        void CrossfadeToClip(AudioClip newClip) {
            if (m_isFading) {
                Debug.LogWarning("A fade operation is already in progress.");
                return;
            }

            if (!newClip) {
                Debug.LogWarning("New audio clip is null.");
                return;
            }

            var inactiveSource = m_activeSource == m_audioSourceA ? m_audioSourceB : m_audioSourceA;
            inactiveSource.clip = newClip;
            inactiveSource.loop = m_loop;
            inactiveSource.volume = 0f;
            inactiveSource.Play();

            m_isFading = true;
            m_currentFadeCoroutine = StartCoroutine(Crossfade(m_activeSource, inactiveSource, m_fadeDuration));
            m_activeSource = inactiveSource;
        }

        IEnumerator FadeVolume
        (
            AudioSource source,
            float startVolume,
            float endVolume,
            float duration,
            Action onComplete = null
        ) {
            var elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, endVolume, t);
                yield return null;
            }

            source.volume = endVolume;
            onComplete?.Invoke();
        }

        IEnumerator Crossfade(AudioSource fromSource, AudioSource toSource, float duration) {
            var elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                fromSource.volume = Mathf.Lerp(1f, 0f, t);
                toSource.volume = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            fromSource.volume = 0f;
            fromSource.Stop();
            toSource.volume = 1f;

            m_isFading = false;
        }

        public void AddAudioClip(AudioClip clip) {
            if (!clip) {
                Debug.LogWarning("Cannot add a null audio clip.");
                return;
            }

            if (!m_audioClips.Contains(clip)) {
                m_audioClips.Add(clip);
            }
            else {
                Debug.LogWarning("Audio clip already exists in the list.");
            }
        }

        public void RemoveAudioClip(AudioClip clip) {
            if (!clip) {
                Debug.LogWarning("Cannot remove a null audio clip.");
                return;
            }

            if (m_audioClips.Contains(clip)) {
                int removedIndex = m_audioClips.IndexOf(clip);
                m_audioClips.Remove(clip);

                if (removedIndex <= m_currentClipIndex && m_currentClipIndex > 0) {
                    m_currentClipIndex--;
                }

                if (m_activeSource.clip == clip) {
                    m_activeSource.clip = null;
                }
            }
            else {
                Debug.LogWarning("Audio clip not found in the list.");
            }
        }

        public void ClearAudioClips() {
            StopAudio();
            m_audioClips.Clear();
            m_activeSource.clip = null;
            m_currentClipIndex = 0;
        }

        public bool IsPlaying() => m_audioSourceA.isPlaying || m_audioSourceB.isPlaying;
    }
}