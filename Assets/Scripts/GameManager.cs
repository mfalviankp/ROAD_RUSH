using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum ScreenState { Menu, Credits, Running, Paused, GameOver }
    public const string RecordKey = "RoadRush.HighScore.v1";
    public const float TopSpeed = 27f;
    public float RoadSpeed => Speed + TrafficSpawner.VillageTrafficSpeed;

    [SerializeField] private LaneSwitching player;
    [SerializeField] private TrafficSpawner traffic;
    [SerializeField] private HighwayPresentation highway;
    [SerializeField] private string[] credits = {
        "[Nama anggota]   /   [NIM anggota]"
    };

    private RoadRushUI ui;
    private RoadAudio audioFx;
    private int overtakes;
    private float elapsed;
    public ScreenState State { get; private set; }
    public double Distance { get; private set; }
    private double RoadDistance => Distance + elapsed * TrafficSpawner.VillageTrafficSpeed;
    public float Speed { get; private set; }
    public float Difficulty => Mathf.Clamp01(elapsed / 115f);
    public int Score => (int)(Distance * .45) + overtakes * 30;
    public int Record { get; private set; }
    public bool NewRecord { get; private set; }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Record = Mathf.Max(0, PlayerPrefs.GetInt(RecordKey, 0));
        audioFx = gameObject.AddComponent<RoadAudio>();
        ui = gameObject.AddComponent<RoadRushUI>();
        ui.Create(this, credits);
        Show(ScreenState.Menu);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
        {
            if (State == ScreenState.Running) Pause();
            else if (State == ScreenState.Paused) Resume();
            else if (keyboard.escapeKey.wasPressedThisFrame && 
                (State == ScreenState.Credits || State == ScreenState.GameOver)) Menu();
            return;
        }
        if (State == ScreenState.GameOver && keyboard.rKey.wasPressedThisFrame) StartRun();
        if (State == ScreenState.Running && player.ReadSteering()) audioFx.Steer();
    }

    private void FixedUpdate()
    {
        if (State != ScreenState.Running) return;
        float delta = Time.fixedDeltaTime;
        elapsed += delta;
        Speed = Mathf.Lerp(14f, TopSpeed, Difficulty);
        float step = Speed * delta;
        Distance += step;
        player.Step(delta);
        overtakes += traffic.Step(step, Difficulty, player);
    }

    private void LateUpdate()
    {
        float renderDistance = State == ScreenState.Running
            ? Mathf.Clamp((float)(Time.timeAsDouble - Time.fixedTimeAsDouble), 0f, Time.fixedDeltaTime) * RoadSpeed
            : 0f;
        highway.Present(RoadDistance + renderDistance, State, player.LateralMotion);
        ui.Refresh(Score, Record, RoadSpeed, player.Lane);
        bool running = State == ScreenState.Running;
        audioFx.SetEngine(running, RoadSpeed);
        audioFx.RefreshTraffic(traffic.Active, running, RoadSpeed);
    }

    public void StartRun()
    {
        if (State == ScreenState.Running) return;
        ResetRun();
        Show(ScreenState.Running);
    }

    private void ResetRun()
    {
        Distance = elapsed = 0;
        Speed = 14f;
        overtakes = 0;
        NewRecord = false;
        player.ResetLane();
        traffic.ResetTraffic(System.Environment.TickCount);
    }

    public void Menu()
    {
        SaveRecord();
        ResetRun();
        Show(ScreenState.Menu);
    }

    public void Credits() => Show(ScreenState.Credits);

    public void Pause()
    {
        if (State == ScreenState.Running) Show(ScreenState.Paused);
    }

    public void Resume()
    {
        if (State == ScreenState.Paused) Show(ScreenState.Running);
    }

    public void Crash()
    {
        if (State != ScreenState.Running) return;
        NewRecord = SaveRecord();
        audioFx.Impact();
        Show(ScreenState.GameOver);
    }

    public void Quit()
    {
        SaveRecord();
        Application.Quit();
    }

    private bool SaveRecord()
    {
        if (Score <= Record) return false;
        Record = Score;
        PlayerPrefs.SetInt(RecordKey, Record);
        PlayerPrefs.Save();
        return true;
    }

    private void Show(ScreenState state)
    {
        State = state;
        Time.timeScale = state == ScreenState.Running ? 1 : 0;
        ui.Show(state);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) Pause();
    }

    private void OnApplicationQuit() => SaveRecord();

    private void OnDestroy() => Time.timeScale = 1;
}
