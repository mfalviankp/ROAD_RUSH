using System.Collections.Generic;
using UnityEngine;

public class RoadAudio : MonoBehaviour
{
    private AudioSource engine;
    private AudioSource effects;
    private AudioClip steer, impact;
    private AudioListener listener;
    private AudioClip[] trafficClips;
    public const float TrafficRadius = 38f;

    private static readonly float[] TrafficVolumes = { .23f, .39f, .36f, .31f, .20f };

    private void Awake()
    {
        listener = FindAnyObjectByType<AudioListener>();
        trafficClips = new[]
        {
            EngineLoop("Mesin mobil traffic", 67, .38f, .19f),
            EngineLoop("Diesel bus", 37, .45f, .12f),
            EngineLoop("Diesel truk", 48, .36f, .26f),
            EngineLoop("Mesin sepeda motor", 102, .32f, .28f),
            Clip("Roda dan rantai becak", 1f, (t, u, n) =>
                .12f * Wave(12 * t) * Wave(53 * t) + .19f * n +
                (.28f * Wave(650 * t) + .16f * Wave(947 * t)) *
                Mathf.Pow(Mathf.Max(0, Wave(8 * t)), 12))
        };
        engine = gameObject.AddComponent<AudioSource>();
        engine.playOnAwake = false;
        engine.clip = Clip("Engine", 1f, (t, u, n) =>
            .54f * Wave(54 * t) + .19f * Wave(108 * t) + .09f * Wave(162 * t) + .035f * n);
        engine.loop = true;
        engine.volume = 0;
        engine.Play();
        effects = gameObject.AddComponent<AudioSource>();
        effects.playOnAwake = false;
        steer = Clip("Lane switch", .12f, (t, u, n) =>
            (Wave(t * (350 - 900 * t)) * .32f + n * .12f) * Mathf.Sin(Mathf.PI * u) * (1 - u));
        impact = Clip("Impact", .48f, (t, u, n) =>
            (Wave(t * (65 - 50 * t)) * .43f + n * .49f) * Mathf.Min(t * 150f, 1f) * Mathf.Pow(1 - u, 3));
    }

    public void SetEngine(bool running, float speed)
    {
        engine.pitch = .75f + speed / 55f;
        engine.volume = Mathf.MoveTowards(engine.volume, running ? .14f : 0f, Time.unscaledDeltaTime * .6f);
    }

    public void RefreshTraffic(IReadOnlyList<EnemyMovement> traffic, bool running, float roadSpeed)
    {
        if (listener == null) return;
        Vector3 ear = listener.transform.position;
        for (int i = 0; i < traffic.Count; i++)
        {
            var car = traffic[i];
            Vector3 offset = car.transform.position - ear;
            if (!running || offset.sqrMagnitude >= TrafficRadius * TrafficRadius)
            {
                car.SetRollingAudio(null, 0, 1);
                continue;
            }
            int kind = (int)car.Sound;
            float pitch = .82f + roadSpeed * .009f + Mathf.Clamp(offset.z * .012f, -.12f, .12f);
            car.SetRollingAudio(trafficClips[kind], TrafficVolumes[kind], pitch);
        }
    }

    private static AudioClip EngineLoop(string name, int fundamental, float bass, float harmonics) =>
        Clip(name, 1f, (t, u, n) =>
            bass * Wave(fundamental * t + .06f * Wave(7 * t)) +
            harmonics * Wave(fundamental * 2 * t) +
            .09f * Wave(fundamental * 4 * t) * Wave(16 * t) + .05f * n);

    public void Steer() => effects.PlayOneShot(steer, .43f);
    public void Impact() => effects.PlayOneShot(impact, .64f);

    private static float Wave(float turns) => Mathf.Sin(turns * 2 * Mathf.PI);

    private static AudioClip Clip(string clipName, float seconds,
        System.Func<float, float, float, float> generate)
    {
        const int rate = 22050;
        var random = new System.Random(155);
        var samples = new float[Mathf.RoundToInt(seconds * rate)];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = generate((float)i / rate, (float)i / samples.Length, (float)random.NextDouble() * 2 - 1);
        var clip = AudioClip.Create(clipName, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        foreach (var clip in trafficClips) Destroy(clip);
        Destroy(engine.clip);
        Destroy(steer);
        Destroy(impact);
    }
}
