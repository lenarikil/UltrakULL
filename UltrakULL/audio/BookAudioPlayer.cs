using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.audio
{
    public static class BookAudioPlayer
    {
        private static GameObject persistentHost;
        private static AudioSource audioSource;
        private static float defaultVolume = 1f;

        private static string _currentLevelFolder;
        private static string _currentBookId;
        private static bool _isPaused;
        private static bool _clipEnded;
        private static bool _gamePaused;

        private static Dictionary<AudioSource, float> _savedVolumes = new Dictionary<AudioSource, float>();
        private const float DuckingFactor = 0.5f;

        public static event Action OnPlay;
        public static event Action OnStop;
        public static event Action OnPause;
        public static event Action OnResume;
        public static event Action OnError;

        public static bool IsPlaying => audioSource != null && audioSource.isPlaying && !_gamePaused && !_clipEnded;
        public static bool RawIsPlaying => audioSource != null && audioSource.isPlaying;
        public static bool IsPaused => _isPaused;
        public static bool IsGamePaused => _gamePaused;
        public static bool ClipEnded => _clipEnded;
        public static bool HasClip => audioSource != null && audioSource.clip != null;
        public static bool HasError { get; private set; }
        public static string ErrorMessage { get; private set; }
        public static float CurrentTime => audioSource != null ? audioSource.time : 0f;
        public static float TotalTime => audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
        public static string CurrentBookId => _currentBookId;
        public static string CurrentLevelFolder => _currentLevelFolder;

        public static float Volume
        {
            get => defaultVolume;
            set
            {
                defaultVolume = Mathf.Clamp01(value);
                if (audioSource != null)
                    audioSource.volume = defaultVolume;
            }
        }

        private static string BooksFolder => Path.Combine(AudioSwapper.SpeechFolder, "books");

        private static void EnsureHost()
        {
            if (persistentHost != null) return;

            persistentHost = new GameObject("BookAudioPlayer_Host");
            UnityEngine.Object.DontDestroyOnLoad(persistentHost);
            audioSource = persistentHost.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.volume = defaultVolume;
        }

        public static void Play(string levelFolder, string bookId)
        {
            if (string.IsNullOrEmpty(levelFolder) || string.IsNullOrEmpty(bookId))
                return;

            EnsureHost();

            _currentLevelFolder = levelFolder;
            _currentBookId = bookId;
            _isPaused = false;
            _clipEnded = false;
            _gamePaused = false;

            string audioPath = Path.Combine(BooksFolder, levelFolder, bookId);

            AudioSwapper.PreloadClipAsync(audioPath, null, clip =>
            {
                if (clip == null)
                {
                    StopDucking();
                    audioSource.Stop();
                    audioSource.clip = null;
                    HasError = true;
                    ErrorMessage = "ERROR: IMPOSSIBLE TO BUILD A SPEECH PATTERN";
                    Logging.Error("[BookAudio] ATTEMPT TO RECONSTRUCT THE VOICE...");
                    Logging.Error("[BookAudio] ERROR: SOURCE NOT FOUND");
                    Logging.Error("[BookAudio] MEMORY OF THE SOUND LOST");
                    Logging.Error("[BookAudio] I CAN NO LONGER REMEMBER HOW THEY SOUNDED");
                    if (OnError != null)
                        OnError();
                    return;
                }

                HasError = false;
                ErrorMessage = null;
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.time = 0f;
                audioSource.volume = defaultVolume;
                audioSource.Play();
                StartDucking();
                Logging.Message("[BookAudio] Playing: " + levelFolder + "/" + bookId);
                if (OnPlay != null)
                    OnPlay();
            });
        }

        public static void Stop()
        {
            if (audioSource == null) return;

            StopDucking();
            audioSource.Stop();
            audioSource.clip = null;
            _isPaused = false;
            _clipEnded = false;
            _gamePaused = false;
            HasError = false;
            ErrorMessage = null;
            _currentLevelFolder = null;
            _currentBookId = null;
            if (OnStop != null)
                OnStop();
        }

        public static void TogglePause()
        {
            if (audioSource == null || audioSource.clip == null) return;

            if (_clipEnded)
            {
                audioSource.time = 0f;
                audioSource.volume = defaultVolume;
                audioSource.Play();
                _isPaused = false;
                _clipEnded = false;
                if (OnPlay != null)
                    OnPlay();
            }
            else if (_isPaused)
            {
                audioSource.UnPause();
                _isPaused = false;
                if (OnResume != null)
                    OnResume();
            }
            else
            {
                audioSource.Pause();
                _isPaused = true;
                if (OnPause != null)
                    OnPause();
            }
        }

        public static void Seek(float offsetSeconds)
        {
            if (audioSource == null || audioSource.clip == null) return;

            float newTime = Mathf.Clamp(audioSource.time + offsetSeconds, 0f, audioSource.clip.length);
            audioSource.time = newTime;

            if (_clipEnded && newTime > 0f)
            {
                _clipEnded = false;
                _isPaused = false;
            }
        }

        private static void StartDucking()
        {
            if (audioSource == null) return;

            AudioSource[] allSources = GameObject.FindObjectsOfType<AudioSource>();
            _savedVolumes.Clear();

            foreach (AudioSource src in allSources)
            {
                if (src == audioSource || src == null) continue;
                _savedVolumes[src] = src.volume;
                src.volume *= DuckingFactor;
            }
        }

        private static void StopDucking()
        {
            foreach (var kvp in _savedVolumes)
            {
                if (kvp.Key != null)
                    kvp.Key.volume = kvp.Value;
            }
            _savedVolumes.Clear();
        }

        public static void PauseForGamePause()
        {
            if (audioSource != null && audioSource.isPlaying && !_isPaused && !_clipEnded)
            {
                audioSource.Pause();
                _gamePaused = true;
            }
        }

        public static void ResumeForGamePause()
        {
            if (audioSource != null && _gamePaused)
            {
                _gamePaused = false;
                if (!_isPaused && !_clipEnded)
                {
                    audioSource.UnPause();
                }
            }
        }

        public static void NotifyClipEnded()
        {
            _isPaused = true;
            _clipEnded = true;
        }
    }
}
