using UnityEngine;

namespace AnimalGrid.Audio
{
    /// <summary>
    /// Synthesizes simple SFX waveforms at runtime - zero audio assets needed.
    /// </summary>
    public static class AudioKit
    {
        public const int SampleRate = 44100;

        public static AudioClip Blip(float freq, float duration, float volume = 0.5f)
        {
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = 1f - (i / (float)samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * volume;
            }
            var clip = AudioClip.Create("blip", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Sweep(float startFreq, float endFreq, float duration, float volume = 0.5f)
        {
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / SampleRate;
                data[i] = Mathf.Sin(phase) * (1f - t) * volume;
            }
            var clip = AudioClip.Create("sweep", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Arpeggio(float[] freqs, float noteLength, float volume = 0.4f)
        {
            int per = (int)(SampleRate * noteLength);
            var data = new float[per * freqs.Length];
            for (int n = 0; n < freqs.Length; n++)
            {
                for (int i = 0; i < per; i++)
                {
                    float t = i / (float)SampleRate;
                    float envelope = 1f - (i / (float)per);
                    data[n * per + i] = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * envelope * volume;
                }
            }
            var clip = AudioClip.Create("arp", data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}