using UnityEngine;

public class RollingWheel : MonoBehaviour
{
    [SerializeField, Min(.1f)] private float radius = .35f;
    private float spin;

    public void Configure(float size) => radius = size;

    public void Advance(float distance)
    {
        spin = Mathf.Repeat(spin + distance / radius * Mathf.Rad2Deg, 360);
        transform.localRotation = Quaternion.Euler(spin, 0, 0);
    }
}
