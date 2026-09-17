using UnityEngine;
using AnimalGrid.Save;

namespace AnimalGrid.Audio
{
    /// <summary>
    /// Plays synthesized SFX + optional music loop (Assets/Resources/Audio/music_home).
    /// Respects SettingsSave toggles. Vibration fires on invalid placement (mobile only).
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        public static float SfxVolume = 1f;

        private AudioSource source;
        private AudioSource musicSource;
        private AudioClip musicClip;

        private AudioClip clipTap, clipButton, clipX, clipPlace, clipInvalid, clipHint, clipWin, clipBossWin;

        private void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicClip = Resources.Load<AudioClip>("Audio/music_home");

            clipTap = AudioKit.Blip(660f, 0.06f);
            clipButton = AudioKit.Blip(880f, 0.05f);
            clipX = AudioKit.Blip(240f, 0.09f);
            clipPlace = AudioKit.Sweep(523f, 784f, 0.14f);
            clipInvalid = AudioKit.Sweep(300f, 140f, 0.22f);
            clipHint = AudioKit.Blip(990f, 0.10f);
            clipWin = AudioKit.Arpeggio(new float[] { 523f, 659f, 784f, 1046f }, 0.14f);
            clipBossWin = AudioKit.Arpeggio(new float[] { 523f, 659f, 784f, 1046f, 1318f, 1568f }, 0.13f);

            ApplySettings();
        }

        /// <summary>Re-reads SettingsSave: SFX volume + music play/stop.</summary>
        public static void ApplySettings()
        {
            SfxVolume = SettingsSave.SfxOn ? 1f : 0f;
            if (Instance != null) Instance.RefreshMusic();
        }

        private void RefreshMusic()
        {
            if (musicSource == null) return;
            bool want = SettingsSave.MusicOn && musicClip != null;
            if (want && !musicSource.isPlaying)
            {
                musicSource.clip = musicClip;
                musicSource.volume = 0.35f;
                musicSource.Play();
            }
            else if (!want && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || SfxVolume <= 0f) return;
            source.PlayOneShot(clip, SfxVolume);
        }

        public void PlayTap() { Play(clipTap); }
        public void PlayButton() { Play(clipButton); }
        public void PlayX() { Play(clipX); }
        public void PlayPlace() { Play(clipPlace); }

        public void PlayInvalid()
        {
            Play(clipInvalid);
            if (SettingsSave.VibrationOn && Application.isMobilePlatform)
            {
                Handheld.Vibrate();
            }
        }

        public void PlayHint() { Play(clipHint); }
        public void PlayWin() { Play(clipWin); }
        public void PlayBossWin() { Play(clipBossWin); }
    }
}