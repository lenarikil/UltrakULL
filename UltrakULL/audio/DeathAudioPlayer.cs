using UnityEngine;

namespace UltrakULL.audio
{
    public static class DeathAudioPlayer
    {
        private static GameObject persistentHost;
        private static AudioSource audioSource;

        private static void EnsureHost()
        {
            if (persistentHost != null) return;

            persistentHost = new GameObject("DeathAudioPlayer_Host");
            Object.DontDestroyOnLoad(persistentHost);
            audioSource = persistentHost.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
        }

        public static AudioSource AudioSource
        {
            get
            {
                EnsureHost();
                return audioSource;
            }
        }

        public static void PlayDeathClip(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            EnsureHost();
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }

        public static void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            EnsureHost();
            audioSource.PlayOneShot(clip, volume);
        }

        public static void Stop()
        {
            if (audioSource != null)
                audioSource.Stop();
        }
    }
}
