using UnityEngine;

public class HighwayPresentation : MonoBehaviour
{
    public const float TileLength = 36f;
    [SerializeField] private Transform[] tiles;
    [SerializeField] private Transform chaseCamera;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerArt;
    private float cameraX;
    private float lean;
    private double lastDistance;
    private RollingWheel[] wheels;

    private void Awake() => wheels = playerArt.GetComponentsInChildren<RollingWheel>();

    public void Present(double distance, GameManager.ScreenState state, float lateralMotion)
    {
        float travel = (float)(distance - lastDistance);
        lastDistance = distance;
        if (travel > 0)
            foreach (var wheel in wheels) wheel.Advance(travel);

        float span = TileLength * tiles.Length;
        float shift = (float)(distance % span);
        for (int i = 0; i < tiles.Length; i++)
        {
            float z = Mathf.Repeat(i * TileLength - shift, span) + player.position.z - 36f;
            tiles[i].position = new Vector3(0, 0, z);
        }

        bool menu = state == GameManager.ScreenState.Menu || state == GameManager.ScreenState.Credits;
        float time = Time.unscaledDeltaTime;
        cameraX = Mathf.Lerp(cameraX, player.position.x * .52f + (menu ? -3.9f : 0f),
            1f - Mathf.Exp(-7f * time));
        lean = Mathf.Lerp(lean, Mathf.Clamp(lateralMotion, -15f, 15f), 1f - Mathf.Exp(-10f * time));
        playerArt.localRotation = Quaternion.Euler(0, lean * .49f, -lean * .32f);
        chaseCamera.position = new Vector3(cameraX, 3.95f, player.position.z - 7.7f);
        chaseCamera.rotation = Quaternion.Euler(12.2f, menu ? -2.4f : 0f, 0);
    }
}
