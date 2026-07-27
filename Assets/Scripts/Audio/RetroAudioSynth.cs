using UnityEngine;

public static class RetroAudioSynth
{
    public enum WaveShape { Sine, Square, Triangle }

    private const int SampleRate = 44100;

    public static AudioClip GenerateTone(string name, float frequency, float duration, WaveShape shape, float volume = 0.3f)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Pow(1f - (float)i / sampleCount, 2f);
            samples[i] = Waveform(shape, frequency, t) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    public static AudioClip GenerateSweep(string name, float startFrequency, float endFrequency, float duration, float volume = 0.3f)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFrequency, endFrequency, t);
            phase += freq / SampleRate;
            float envelope = Mathf.Pow(1f - t, 1.5f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * phase) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    public static AudioClip GenerateNoiseBurst(string name, float duration, float volume = 0.3f)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        System.Random rng = new System.Random();

        for (int i = 0; i < sampleCount; i++)
        {
            float envelope = Mathf.Pow(1f - (float)i / sampleCount, 2f);
            samples[i] = ((float)rng.NextDouble() * 2f - 1f) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    private static float Waveform(WaveShape shape, float frequency, float t)
    {
        return shape switch
        {
            WaveShape.Sine => Mathf.Sin(2f * Mathf.PI * frequency * t),
            WaveShape.Square => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * t)),
            WaveShape.Triangle => Mathf.PingPong(frequency * t * 4f, 2f) - 1f,
            _ => 0f
        };
    }

    private static AudioClip CreateClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
