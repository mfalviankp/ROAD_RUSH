using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal static class RoadRushSetup
{
    private const string ScenePath = "Assets/Scenes/Gameplay.unity";
    private const string EnemyPath = "Assets/Prefabs/EnemyCar.prefab";
    private const string RoadRoot = "Village Pass";

    [MenuItem("ROAD RUSH/Prepare Gameplay")]
    public static void Prepare()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var player = scene.GetRootGameObjects().Single(o => o.name == "PlayerCar").GetComponent<LaneSwitching>();
        var spawner = scene.GetRootGameObjects().Single(o => o.name == "TrafficSpawner").GetComponent<TrafficSpawner>();
        var camera = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Camera>(true))
            .Single(c => c.CompareTag("MainCamera"));
        if (AssetDatabase.GetAssetPath(spawner.enemyCarPrefab) != EnemyPath)
            throw new InvalidOperationException("TrafficSpawner points to an unexpected prefab.");

        var art = new RoadRushArt();
        Transform playerArt = art.BuildVehicle(player.transform, true);
        ConfigurePhysics(player.gameObject, true);
        var enemy = PrefabUtility.LoadPrefabContents(EnemyPath);
        try
        {
            art.BuildVehicle(enemy.transform, false);
            ConfigurePhysics(enemy, false);
            PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(enemy);
        }

        var fleet = art.BuildVillageFleet();
        var paintProperty = new SerializedObject(spawner);
        var variants = paintProperty.FindProperty("extraVehicles");
        variants.arraySize = fleet.Length;
        for (int i = 0; i < fleet.Length; i++)
            variants.GetArrayElementAtIndex(i).objectReferenceValue = fleet[i];
        var paints = paintProperty.FindProperty("paints");
        paints.arraySize = art.TrafficPaints.Length;
        for (int i = 0; i < paints.arraySize; i++)
            paints.GetArrayElementAtIndex(i).objectReferenceValue = art.TrafficPaints[i];
        paintProperty.FindProperty("spawnZ").floatValue = player.transform.position.z + 84;
        paintProperty.ApplyModifiedPropertiesWithoutUndo();

        var roots = scene.GetRootGameObjects();
        var archived = roots.SingleOrDefault(o => o.name == "Original Highway");
        if (archived == null)
        {
            archived = new GameObject("Original Highway");
            foreach (var obj in roots)
            {
                if (obj.name == "Road" || obj.name.StartsWith("Grass_", StringComparison.Ordinal) ||
                    obj.name.StartsWith("LaneMarking_", StringComparison.Ordinal) ||
                    obj.name.StartsWith("YellowLine_", StringComparison.Ordinal))
                    obj.transform.SetParent(archived.transform, true);
            }
            archived.SetActive(false);
        }

        var priorRoad = roots.SingleOrDefault(o => o.name == RoadRoot || o.name == "Desert Pass");
        if (priorRoad != null) UnityEngine.Object.DestroyImmediate(priorRoad);
        var road = new GameObject(RoadRoot);
        var highway = road.AddComponent<HighwayPresentation>();
        Transform[] tiles = art.Highway(road.transform, player.transform.position.z);
        Assign(highway, "chaseCamera", camera.transform);
        Assign(highway, "player", player.transform);
        Assign(highway, "playerArt", playerArt);
        var serializedHighway = new SerializedObject(highway);
        var tileArray = serializedHighway.FindProperty("tiles");
        tileArray.arraySize = tiles.Length;
        for (int i = 0; i < tiles.Length; i++) tileArray.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
        serializedHighway.ApplyModifiedPropertiesWithoutUndo();

        camera.transform.SetParent(null, true);
        camera.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 3.95f, -7.7f),
            Quaternion.Euler(12.2f, 0, 0));
        camera.fieldOfView = 62;
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 280f;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.allowHDR = true;
        camera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.FastApproximateAntialiasing;

        roots = scene.GetRootGameObjects();
        var sun = roots.Select(o => o.GetComponent<Light>()).First(l => l != null && l.type == LightType.Directional);
        sun.transform.rotation = Quaternion.Euler(39, -30, 0);
        sun.color = new Color(1f, .77f, .61f);
        sun.intensity = 1.65f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = .83f;
        RenderSettings.sun = sun;
        RenderSettings.skybox = art.Sky();
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = .0095f;
        RenderSettings.fogColor = new Color(.68f, .39f, .34f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.52f, .57f, .73f);
        RenderSettings.ambientEquatorColor = new Color(.45f, .35f, .38f);
        RenderSettings.ambientGroundColor = new Color(.28f, .22f, .25f);

        var managerObject = roots.SingleOrDefault(o => o.name == "GameManager") ?? new GameObject("GameManager");
        var manager = Ensure<GameManager>(managerObject);
        Assign(manager, "player", player);
        Assign(manager, "traffic", spawner);
        Assign(manager, "highway", highway);
        Assign(Ensure<PlayerCollision>(player.gameObject), "game", manager);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Gameplay scene could not be saved.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = false;
        AssetDatabase.SaveAssets();
        Debug.Log("ROAD_RUSH_SETUP_OK");
    }

    private static void ConfigurePhysics(GameObject car, bool player)
    {
        var box = Ensure<BoxCollider>(car);
        foreach (var collider in car.GetComponentsInChildren<Collider>(true)) collider.enabled = collider == box;
        box.isTrigger = player;
        box.center = new Vector3(0, .65f, 0);
        box.size = new Vector3(1.61f, .90f, 3.18f);
        var body = Ensure<Rigidbody>(car);
        body.useGravity = false;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    private static T Ensure<T>(GameObject gameObject) where T : Component =>
        gameObject.TryGetComponent<T>(out var component) ? component : gameObject.AddComponent<T>();

    private static void Assign(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        var property = new SerializedObject(target);
        property.FindProperty(name).objectReferenceValue = value;
        property.ApplyModifiedPropertiesWithoutUndo();
    }

    [MenuItem("ROAD RUSH/Build Windows x64")]
    public static void BuildWindows()
    {
        string configured = Environment.GetEnvironmentVariable("ROAD_RUSH_BUILD_PATH");
        string location = string.IsNullOrWhiteSpace(configured)
            ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Build-Windows-Desa", "ROAD_RUSH.exe"))
            : configured;
        Directory.CreateDirectory(Path.GetDirectoryName(location));
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new NotSupportedException("Windows Standalone build support is missing.");
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = location,
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (result.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"Windows build failed: {result.summary.result} ({result.summary.totalErrors} errors)");
        Debug.Log($"ROAD_RUSH_BUILD_OK {location} {result.summary.totalSize:N0} bytes");
    }
}
