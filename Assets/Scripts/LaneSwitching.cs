using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class LaneSwitching : MonoBehaviour
{
    public float laneDistance = 3f;
    [SerializeField, Min(.12f)] private float laneChangeTime = .29f;

    private Rigidbody body;
    private Vector3 origin;
    private float startX;
    private float elapsed;
    private float duration;
    public int Lane { get; private set; } = 1;
    public Vector3 Origin => origin;
    public float LateralMotion { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        origin = body.position;
    }

    public bool ReadSteering()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;
        bool left = keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
        bool right = keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;
        if (left == right) return false;

        int next = Mathf.Clamp(Lane + (left ? -1 : 1), 0, 2);
        if (next == Lane) return false;
        Lane = next;
        startX = body.position.x;
        elapsed = 0;
        duration = Mathf.Max(.10f, laneChangeTime * Mathf.Abs(TargetX - startX) / laneDistance);
        return true;
    }

    private float TargetX => origin.x + (Lane - 1) * laneDistance;

    public void Step(float delta)
    {
        if (elapsed >= duration)
        {
            LateralMotion = 0;
            return;
        }

        elapsed = Mathf.Min(duration, elapsed + delta);
        float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        float x = Mathf.Lerp(startX, TargetX, t);
        LateralMotion = (x - body.position.x) / delta;
        body.MovePosition(new Vector3(x, origin.y, origin.z));
    }

    public void ResetLane()
    {
        Lane = 1;
        elapsed = duration = 0;
        LateralMotion = 0;
        body.position = origin;
    }
}
