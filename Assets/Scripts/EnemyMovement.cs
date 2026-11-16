using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class EnemyMovement : MonoBehaviour
{
    public enum SoundKind { Car, Bus, Truck, Motorcycle, Becak }

    [SerializeField] private SoundKind sound;
    private AudioSource rollingAudio;
    private Rigidbody body;
    private Renderer paint;
    private RollingWheel[] wheels;
    public bool Passed { get; set; }
    public int FleetIndex { get; set; }
    public float Z => body.position.z;
    public float FrontOffset { get; private set; }
    public SoundKind Sound => sound;

    public void ConfigureSound(SoundKind kind) => sound = kind;

    public void SetRollingAudio(AudioClip clip, float volume, float pitch)
    {
        if (clip == null)
        {
            if (rollingAudio != null && rollingAudio.isPlaying) rollingAudio.Stop();
            return;
        }

        if (rollingAudio == null)
        {
            rollingAudio = gameObject.AddComponent<AudioSource>();
            rollingAudio.playOnAwake = false;
            rollingAudio.loop = true;
            rollingAudio.spatialBlend = 1;
            rollingAudio.dopplerLevel = 0;
            rollingAudio.rolloffMode = AudioRolloffMode.Linear;
            rollingAudio.minDistance = 3;
            rollingAudio.maxDistance = RoadAudio.TrafficRadius;
            rollingAudio.spread = 25;
        }
        rollingAudio.volume = volume;
        rollingAudio.pitch = pitch;
        if (rollingAudio.clip != clip) rollingAudio.clip = clip;
        if (!rollingAudio.isPlaying) rollingAudio.Play();
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        var collider = GetComponent<BoxCollider>();
        FrontOffset = collider.center.z + collider.size.z * .5f;
        paint = transform.Find("EnemyBody")?.GetComponent<Renderer>();
        wheels = GetComponentsInChildren<RollingWheel>();
    }

    public void Spawn(Vector3 position, Material color)
    {
        // Teleport while parked so interpolation starts at this row rather than the prior lane.
        transform.position = position;
        gameObject.SetActive(true);
        body.position = position;
        if (paint != null && color != null) paint.sharedMaterial = color;
        Passed = false;
    }

    public void Step(float distance, float wheelTravel)
    {
        body.MovePosition(body.position + Vector3.back * distance);
        foreach (var wheel in wheels) wheel.Advance(wheelTravel);
    }
}
