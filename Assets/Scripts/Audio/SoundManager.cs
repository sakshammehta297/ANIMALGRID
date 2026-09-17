using UnityEngine;

namespace AnimalGrid.Audio
{
    /// <summary>
    /// Plays the synthesized SFX. Independent SFX volume (spec 31).
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        public static float SfxVolume = 1f;

        private AudioSource source;
        private AudioClip clipTap, clipButton, clipX, clipPlace, clipInvalid, clipHint, clipWin, clipBossWin;

        private void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();

            clipTap = AudioKit.Blip(660f, 0.06f);
            clipButton = AudioKit.Blip(880f, 0.05f);
            clipX = AudioKit.Blip(240f, 0.09f);
            clipPlace = AudioKit.Sweep(523f, 784f, 0.14f);
            clipInvalid = AudioKit.Sweep(300f, 140f, 0.22f);
            clipHint = AudioKit.Blip(990f, 0.10f);
            clipWin = AudioKit.Arpeggio(new float[] { 523f, 659f, 784f, 1046f }, 0.14f);
            clipBossWin = AudioKit.Arpeggio(new float[] { 523f, 659f, 784f, 1046f, 1318f, 1568f }, 0.13f);
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
        public void PlayInvalid() { Play(clipInvalid); }
        public void PlayHint() { Play(clipHint); }
        public void PlayWin() { Play(clipWin); }
        public void PlayBossWin() { Play(clipBossWin); }
    }
}