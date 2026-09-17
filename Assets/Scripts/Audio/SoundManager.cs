using System.Collections;
using UnityEngine;
using AnimalGrid.Save;

namespace AnimalGrid.Audio
{
    /// <summary>
    /// Audio hub with diagnostics: real files from Resources/Audio, synth fallbacks,
    /// music states (home/game/boss) with crossfade, stings (win/world).
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        public static float SfxVolume = 1f;
        private const float MusicVolume = 0.45f;

        private AudioSource sfxSource;
        private AudioSource stingSource;
        private readonly AudioSource[] musicSources = new AudioSource[2];
        private int activeMusic;
        private string currentState = "";
        private Coroutine fadeRoutine;

        private AudioClip clipTap, clipButton, clipX, clipPlace, clipInvalid, clipHint;
        private AudioClip clipTray, clipHeart, clipUnlock;
        private AudioClip musicHome, musicGame, musicBoss, stingWin, stingWorld;

        private void Awake()
        {
            Instance = this;
            sfxSource = gameObject.AddComponent<AudioSource>();
            stingSource = gameObject.AddComponent<AudioSource>();
            musicSources[0] = gameObject.AddComponent<AudioSource>();
            musicSources[1] = gameObject.AddComponent<AudioSource>();
            musicSources[0].loop = true;
            musicSources[1].loop = true;

            clipTap = Load("sfx_tap") ?? AudioKit.Blip(660f, 0.06f);
            clipButton = Load("sfx_button") ?? AudioKit.Blip(880f, 0.05f);
            clipX = Load("sfx_x") ?? AudioKit.Blip(240f, 0.09f);
            clipPlace = Load("sfx_place") ?? AudioKit.Sweep(523f, 784f, 0.14f);
            clipInvalid = Load("sfx_invalid") ?? AudioKit.Sweep(300f, 140f, 0.22f);
            clipHint = Load("sfx_hint") ?? AudioKit.Blip(990f, 0.10f);
            clipTray = Load("sfx_tray") ?? AudioKit.Blip(1046f, 0.08f);
            clipHeart = Load("sfx_heart") ?? AudioKit.Sweep(400f, 200f, 0.25f);
            clipUnlock = Load("sfx_unlock") ?? AudioKit.Arpeggio(new float[] { 659f, 784f, 988f, 1319f }, 0.12f);

            musicHome = Load("music_home");
            musicGame = Load("music_game");
            musicBoss = Load("music_boss");
            stingWin = Load("music_win");
            stingWorld = Load("music_world");

            Debug.Log("AUDIO clips loaded -> home:" + (musicHome != null)
                + " game:" + (musicGame != null)
                + " boss:" + (musicBoss != null)
                + " win:" + (stingWin != null)
                + " world:" + (stingWorld != null)
                + " sfxPlace:" + (clipPlace != null));

            ApplySettings();
        }

        private static AudioClip Load(string name)
        {
            return Resources.Load<AudioClip>("Audio/" + name);
        }

        public static void ApplySettings()
        {
            SfxVolume = SettingsSave.SfxOn ? 1f : 0f;
            Debug.Log("AUDIO ApplySettings -> sfxOn:" + SettingsSave.SfxOn + " musicOn:" + SettingsSave.MusicOn);
            if (Instance == null) return;
            if (!SettingsSave.MusicOn) Instance.StopMusic();
            else if (!string.IsNullOrEmpty(Instance.currentState)) Instance.PlayMusic(Instance.currentState);
        }

        // ---------- Music ----------

        public void PlayMusic(string state)
        {
            currentState = state;
            AudioClip target = ClipForState(state);
            Debug.Log("AUDIO PlayMusic(" + state + ") -> musicOn:" + SettingsSave.MusicOn + " clipFound:" + (target != null));
            if (!SettingsSave.MusicOn) return;
            if (target == null) { StopMusic(); return; }

            var current = musicSources[activeMusic];
            if (current.clip == target && current.isPlaying) return;

            int next = 1 - activeMusic;
            var nextSrc = musicSources[next];
            nextSrc.clip = target;
            nextSrc.volume = 0f;
            nextSrc.Play();
            activeMusic = next;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(Crossfade(current, nextSrc));
        }

        private AudioClip ClipForState(string state)
        {
            if (state == "game") return musicGame;
            if (state == "boss") return musicBoss;
            return musicHome;
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to)
        {
            float start = from.volume;
            float t = 0f;
            const float dur = 0.8f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                to.volume = Mathf.Lerp(0f, MusicVolume, k);
                from.volume = Mathf.Lerp(start, 0f, k);
                yield return null;
            }
            from.Stop();
            from.volume = 0f;
            to.volume = MusicVolume;
            fadeRoutine = null;
        }

        private void StopMusic()
        {
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            foreach (var s in musicSources)
            {
                if (s != null) { s.Stop(); s.volume = 0f; }
            }
        }

        public void PlaySting(string name)
        {
            if (!SettingsSave.MusicOn) return;
            AudioClip clip = name == "world" ? stingWorld : stingWin;
            if (clip != null) stingSource.PlayOneShot(clip, 0.9f);
        }

        // ---------- SFX ----------

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            if (SfxVolume <= 0f)
            {
                Debug.LogWarning("AUDIO: SFX muted (Settings -> Sound Effects is OFF)");
                return;
            }
            sfxSource.PlayOneShot(clip, SfxVolume);
        }

        public void PlayTap() { Play(clipTap); }
        public void PlayButton() { Play(clipButton); }
        public void PlayX() { Play(clipX); }
        public void PlayPlace() { Play(clipPlace); }
        public void PlayHint() { Play(clipHint); }
        public void PlayTray() { Play(clipTray); }
        public void PlayHeart() { Play(clipHeart); }
        public void PlayUnlock() { Play(clipUnlock); }

        public void PlayInvalid()
        {
            Play(clipInvalid);
            if (SettingsSave.VibrationOn && Application.isMobilePlatform) Handheld.Vibrate();
        }

        public void PlayWin()
        {
            if (stingWin != null && SettingsSave.MusicOn) stingSource.PlayOneShot(stingWin, 0.9f);
            else Play(AudioKit.Arpeggio(new float[] { 523f, 659f, 784f, 1046f }, 0.14f));
        }

        public void PlayBossWin()
        {
            if (stingWin != null && SettingsSave.MusicOn) stingSource.PlayOneShot(stingWin, 1f);
            else Play(AudioKit.Arpeggio(new float[] { 523f, 659f, 784f, 1046f, 1318f, 1568f }, 0.13f));
        }
    }
}