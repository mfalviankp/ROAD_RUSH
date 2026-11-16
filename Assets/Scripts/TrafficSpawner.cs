using System.Collections.Generic;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    public const float MinimumGap = 33.5f;
    public const float VillageTrafficSpeed = 4.5f;
    private const int AllLanes = 0b111;

    public GameObject enemyCarPrefab;
    [SerializeField] private float spawnZ = 64f;
    [SerializeField] private Material[] paints;
    [SerializeField] private GameObject[] extraVehicles;
    private readonly List<EnemyMovement> active = new();
    private GameObject[] fleet;
    private Stack<EnemyMovement>[] pools;
    private int[] rotation;
    private int nextVehicle;
    private System.Random random;
    private float untilNext;
    private int lastMask;
    private bool initialRows;
    public IReadOnlyList<EnemyMovement> Active => active;

    private void Awake()
    {
        int extraCount = extraVehicles?.Length ?? 0;
        fleet = new GameObject[1 + extraCount];
        fleet[0] = enemyCarPrefab;
        if (extraCount > 0) extraVehicles.CopyTo(fleet, 1);
        rotation = new int[fleet.Length];
        pools = new Stack<EnemyMovement>[fleet.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new Stack<EnemyMovement>();
            rotation[i] = i;
        }
    }

    public void ResetTraffic(int seed)
    {
        foreach (var car in active) ReturnToPool(car);
        active.Clear();
        random = new System.Random(seed);
        nextVehicle = rotation.Length;
        lastMask = 0;
        initialRows = true;
        untilNext = 0;
    }

    public int Step(float distance, float difficulty, LaneSwitching player)
    {
        int overtakes = 0;
        float wheelTravel = VillageTrafficSpeed * Time.fixedDeltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var car = active[i];
            float front = car.Z + car.FrontOffset;
            if (!car.Passed && front < player.Origin.z - 1.8f)
            {
                car.Passed = true;
                overtakes++;
            }

            if (front < player.Origin.z - 19f)
            {
                ReturnToPool(car);
                active.RemoveAt(i);
                continue;
            }
            car.Step(distance, wheelTravel);
        }

        if (initialRows)
        {
            PlaceRow(player, 0b101, spawnZ - 39f);
            PlaceRow(player, NextMask(random, difficulty, lastMask), spawnZ);
            initialRows = false;
            untilNext = RowGap(difficulty);
        }
        else
        {
            untilNext -= distance;
            if (untilNext <= 0)
            {
                PlaceRow(player, NextMask(random, difficulty, lastMask), spawnZ);
                untilNext = RowGap(difficulty);
            }
        }
        return overtakes;
    }

    private float RowGap(float difficulty) =>
        Mathf.Lerp(37.5f, MinimumGap, difficulty) + (float)random.NextDouble() * 4.5f;

    private void PlaceRow(LaneSwitching player, int mask, float z)
    {
        lastMask = mask;
        for (int lane = 0; lane < 3; lane++)
        {
            if ((mask & (1 << lane)) == 0) continue;
            int index = ChooseVehicle();
            var pool = pools[index];
            var car = pool.Count > 0 ? pool.Pop() :
                Instantiate(fleet[index], transform).GetComponent<EnemyMovement>();
            car.FleetIndex = index;
            var at = player.Origin + Vector3.right * ((lane - 1) * player.laneDistance);
            at.z = z;
            var color = index == 0 && paints.Length > 0 ? paints[random.Next(paints.Length)] : null;
            car.Spawn(at, color);
            active.Add(car);
        }
    }

    private int ChooseVehicle()
    {
        if (nextVehicle == rotation.Length)
        {
            for (int i = rotation.Length - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                (rotation[i], rotation[other]) = (rotation[other], rotation[i]);
            }
            nextVehicle = 0;
        }
        return rotation[nextVehicle++];
    }

    public static int NextMask(System.Random rng, float difficulty, int previous)
    {
        // Every row has an opening and shared closing speed keeps the clearance constant.
        bool pair = rng.NextDouble() < Mathf.Lerp(.57f, .88f, difficulty);
        int lane = rng.Next(3);
        int mask = pair ? AllLanes ^ (1 << lane) : 1 << lane;
        if (mask != previous) return mask;
        lane = (lane + rng.Next(1, 3)) % 3;
        return pair ? AllLanes ^ (1 << lane) : 1 << lane;
    }

    private void ReturnToPool(EnemyMovement car)
    {
        car.gameObject.SetActive(false);
        pools[car.FleetIndex].Push(car);
    }
}
