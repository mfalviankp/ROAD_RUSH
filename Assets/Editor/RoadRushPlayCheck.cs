using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
internal static class RoadRushPlayCheck
{
    private const string Pending = "RoadRush.PlayCheck.Pending";
    private static Keyboard keyboard;
    private static bool failed;

    static RoadRushPlayCheck() => EditorApplication.playModeStateChanged += OnMode;

    [MenuItem("ROAD RUSH/Run Play Mode checks")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before starting the checks.");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        SessionState.SetBool(Pending, true);
        SessionState.SetBool(Pending + ".RecordExists", PlayerPrefs.HasKey(GameManager.RecordKey));
        SessionState.SetInt(Pending + ".Record", PlayerPrefs.GetInt(GameManager.RecordKey));
        PlayerPrefs.SetInt(GameManager.RecordKey, 0);
        EditorApplication.isPlaying = true;
    }

    private static void OnMode(PlayModeStateChange mode)
    {
        if (!SessionState.GetBool(Pending, false)) return;
        if (mode == PlayModeStateChange.EnteredPlayMode)
        {
            failed = false;
            var game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (game != null)
            {
                var host = new GameObject(nameof(RoadRushPlayCheck));
                UnityEngine.Object.DontDestroyOnLoad(host);
                host.AddComponent<RoadRushUI>().StartCoroutine(Report(Verify()));
            }
            else Finish(new Exception("GameManager missing from scene."));
        }
        if (mode != PlayModeStateChange.EnteredEditMode) return;

        if (SessionState.GetBool(Pending + ".RecordExists", false))
            PlayerPrefs.SetInt(GameManager.RecordKey, SessionState.GetInt(Pending + ".Record", 0));
        else
            PlayerPrefs.DeleteKey(GameManager.RecordKey);
        PlayerPrefs.Save();
        SessionState.EraseBool(Pending);
        if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetBool(Pending + ".Failed", true) ? 1 : 0);
    }

    private static IEnumerator Report(IEnumerator checks)
    {
        // Drive nested enumerators here so an assertion always restores the local record.
        var stack = new System.Collections.Generic.Stack<IEnumerator>();
        stack.Push(checks);
        while (stack.Count != 0)
        {
            object current = null;
            Exception failure = null;
            bool more = false;
            try
            {
                more = stack.Peek().MoveNext();
                if (more) current = stack.Peek().Current;
            }
            catch (Exception error) { failure = error; }
            if (failure != null)
            {
                Finish(failure);
                yield break;
            }
            if (!more) stack.Pop();
            else if (current is IEnumerator nested) stack.Push(nested);
            else yield return current;
        }
        Finish(null);
    }

    private static IEnumerator Verify()
    {
        yield return null;
        var game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<LaneSwitching>();
        var traffic = UnityEngine.Object.FindAnyObjectByType<TrafficSpawner>();
        keyboard = InputSystem.AddDevice<Keyboard>();
        keyboard.MakeCurrent();

        Check(game.State == GameManager.ScreenState.Menu && Time.timeScale == 0, "menu before race");
        Check(SceneManager.GetActiveScene().path == "Assets/Scenes/Gameplay.unity", "correct scene");
        foreach (string facility in new[] { "Koperasi Desa", "Polsek", "Dapur MBG" })
        {
            var landmark = GameObject.Find(facility);
            Check(landmark != null && landmark.GetComponentsInChildren<TextMesh>().Length == 2,
                "roadside facility and its signs: " + facility);
        }
        Check(GameObject.Find("MULAI BALAP") == EventSystem.current.currentSelectedGameObject, "menu keyboard focus");
        Submit("Credits");
        Check(game.State == GameManager.ScreenState.Credits &&
            !string.IsNullOrWhiteSpace(GameObject.Find("Team credits")?.GetComponent<Text>().text),
            "credits and team text");
        Submit("Kembali");
        Submit("MULAI BALAP");
        Check(game.State == GameManager.ScreenState.Running && Time.timeScale == 1, "start button");

        yield return KeyPress(Key.A);
        yield return new WaitForSecondsRealtime(.35f);
        float left = player.Origin.x - player.laneDistance;
        Check(player.Lane == 0 && Mathf.Abs(player.transform.position.x - left) < .03f, "A reaches left lane");
        yield return KeyPress(Key.A);
        Check(player.Lane == 0, "lane bounded");
        yield return KeyPress(Key.D);
        yield return new WaitForSecondsRealtime(.35f);
        Check(player.Lane == 1 && Mathf.Abs(player.transform.position.x - player.Origin.x) < .03f, "D returns to center");
        yield return KeyPress(Key.RightArrow);
        yield return new WaitForSecondsRealtime(.35f);
        Check(player.Lane == 2, "right arrow reaches third lane");

        yield return KeyPress(Key.Escape);
        double paused = game.Distance;
        Check(game.State == GameManager.ScreenState.Paused && Time.timeScale == 0, "Escape pauses");
        yield return new WaitForSecondsRealtime(.20f);
        Check(game.Distance == paused, "score and traffic clock frozen");
        Submit("Lanjutkan");
        yield return null;
        Check(game.State == GameManager.ScreenState.Running && Time.timeScale == 1, "resume button");

        Check(traffic.Active.Count >= 1, "traffic spawned");
        var target = traffic.Active.OrderBy(car => car.Z).First();
        int targetLane = Mathf.RoundToInt((target.transform.position.x - player.Origin.x) / player.laneDistance) + 1;
        for (int attempt = 0; attempt < 3 && player.Lane != targetLane; attempt++)
            yield return KeyPress(player.Lane < targetLane ? Key.D : Key.A);
        Check(player.Lane == targetLane, "can steer toward a test vehicle");

        float limit = Time.realtimeSinceStartup + 9;
        while (game.State == GameManager.ScreenState.Running && Time.realtimeSinceStartup < limit) yield return null;
        Check(game.State == GameManager.ScreenState.GameOver && Time.timeScale == 0, "physical trigger leads to Game Over");
        int score = game.Score;
        Check(score > 0 && game.Record == score && PlayerPrefs.GetInt(GameManager.RecordKey) == score, "record saved");
        game.Crash();
        Check(game.NewRecord && game.Record == score && game.Score == score, "repeated impact ignored");
        Check(GameObject.Find("Main lagi   R") != null, "Game Over UI visible");

        Submit("Main lagi   R");
        Check(game.State == GameManager.ScreenState.Running && game.Score == 0 &&
            game.Distance == 0 && game.Record == score && traffic.Active.Count == 0 && player.Lane == 1,
            "restart resets run and keeps record");
        var seen = new System.Collections.Generic.HashSet<int>();
        var voices = new System.Collections.Generic.HashSet<EnemyMovement.SoundKind>();
        float fleetDeadline = Time.realtimeSinceStartup + 5f;
        while (seen.Count < 5 && Time.realtimeSinceStartup < fleetDeadline)
        {
            foreach (var car in traffic.Active)
            {
                seen.Add(car.FleetIndex);
                voices.Add(car.Sound);
                Check(car.GetComponent<BoxCollider>().size.x < player.laneDistance, "vehicle fits within its lane");
                Check(car.GetComponentsInChildren<RollingWheel>().Length >= 2, "vehicle has rolling wheels");
            }
            yield return null;
        }
        Check(seen.Count == 5 && voices.Count == 5, "all five vehicle types spawn with distinct sounds");
        var rolling = traffic.Active.First();
        var wheel = rolling.GetComponentInChildren<RollingWheel>().transform;
        var wheelAngle = wheel.localRotation;
        float carPosition = rolling.Z;
        yield return new WaitForSecondsRealtime(.12f);
        Check(rolling.Z < carPosition && Quaternion.Angle(wheelAngle, wheel.localRotation) > 4f &&
            game.RoadSpeed > game.Speed, "vehicles advance along the road and their wheels turn");
        var nearby = traffic.Active.Select(car => car.GetComponent<AudioSource>())
            .FirstOrDefault(source => source != null && source.isPlaying);
        Check(nearby != null && nearby.clip != null && nearby.spatialBlend == 1 && nearby.loop,
            "nearby vehicle plays a spatial audio loop");
        game.Pause();
        yield return null;
        yield return null;
        Check(!nearby.isPlaying, "traffic audio stops when paused");
        game.Menu();
        int savedBeforeReload = game.Record;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        yield return null;
        yield return null;
        game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        Check(game.Record == savedBeforeReload && game.State == GameManager.ScreenState.Menu, "record loaded in fresh scene");

        var rng = new System.Random(2026);
        int previous = 0;
        for (int i = 0; i < 2500; i++)
        {
            int row = TrafficSpawner.NextMask(rng, i / 2500f, previous);
            Check(row is > 0 and < 7 && row != previous, "safe randomized row");
            previous = row;
        }
        Debug.Log("ROAD_RUSH_DESA_PLAY_CHECK_OK roadside facilities, five traffic voices, spatial audio, pause, collision, restart, 2500 rows");
    }

    private static IEnumerator KeyPress(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        yield return null;
    }

    private static void Submit(string name)
    {
        var obj = GameObject.Find(name);
        Check(obj != null, "visible button " + name);
        obj.GetComponent<Button>().OnSubmit(new BaseEventData(EventSystem.current));
    }

    private static void Check(bool pass, string label)
    {
        if (!pass) throw new InvalidOperationException(label);
    }

    private static void Finish(Exception error)
    {
        failed = error != null;
        if (failed) Debug.LogError("ROAD_RUSH_PLAY_CHECK_FAILED " + error);
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        SessionState.SetBool(Pending + ".Failed", failed);
        EditorApplication.isPlaying = false;
    }
}
