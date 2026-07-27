using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const int SampleRate = 44100;
    private const float MusicDuration = 8f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip shootClip;
    private AudioClip hitClip;
    private AudioClip enemyDeathClip;
    private AudioClip pickupClip;
    private AudioClip wallHitClip;
    private AudioClip gameOverClip;
    private AudioClip musicClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.25f;

        GenerateClips();
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    private void GenerateClips()
    {
        shootClip = RetroAudioSynth.GenerateTone("Shoot", 880f, 0.06f, RetroAudioSynth.WaveShape.Square, 0.15f);
        hitClip = RetroAudioSynth.GenerateTone("Hit", 220f, 0.05f, RetroAudioSynth.WaveShape.Triangle, 0.2f);
        enemyDeathClip = RetroAudioSynth.GenerateSweep("EnemyDeath", 500f, 80f, 0.2f, 0.25f);
        pickupClip = RetroAudioSynth.GenerateSweep("Pickup", 400f, 900f, 0.15f, 0.2f);
        wallHitClip = RetroAudioSynth.GenerateNoiseBurst("WallHit", 0.15f, 0.3f);
        gameOverClip = RetroAudioSynth.GenerateSweep("GameOver", 400f, 60f, 0.8f, 0.3f);
        musicClip = GenerateMusicLoop();
    }

    private static AudioClip GenerateMusicLoop()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * MusicDuration);
        float[] samples = new float[sampleCount];

        float root = Mathf.Round(130.81f * MusicDuration) / MusicDuration;
        float fifth = Mathf.Round(196.0f * MusicDuration) / MusicDuration;
        float lfoFrequency = 1f / MusicDuration;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float lfo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * lfoFrequency * t);
            float rootWave = Mathf.Sin(2f * Mathf.PI * root * t);
            float fifthWave = Mathf.Sin(2f * Mathf.PI * fifth * t) * 0.6f;
            samples[i] = (rootWave + fifthWave) * 0.08f * lfo;
        }

        AudioClip clip = AudioClip.Create("Music", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayShoot() => sfxSource.PlayOneShot(shootClip);
    public void PlayHit() => sfxSource.PlayOneShot(hitClip);
    public void PlayEnemyDeath() => sfxSource.PlayOneShot(enemyDeathClip);
    public void PlayPickup() => sfxSource.PlayOneShot(pickupClip);
    public void PlayWallHit() => sfxSource.PlayOneShot(wallHitClip);
    public void PlayGameOver() => sfxSource.PlayOneShot(gameOverClip);
}
