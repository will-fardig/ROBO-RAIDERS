using System.Collections.Generic;
using UnityEngine;

// Zombies-style rounds: break -> spawn the round -> clear it -> break -> ...
public class WaveManager : MonoBehaviour
{
    public enum State { Idle, Intermission, Spawning, Clearing }

    [Header("Setup")]
    [SerializeField] private WaveConfig config;
    [SerializeField] private EnemySpawnDirector director;

    [Header("Start")]
    [SerializeField] private float firstIntermission = 3f;
    [Tooltip("Treat the runners already placed in the scene as wave 1.")]
    [SerializeField] private bool adoptSceneEnemiesAsWave1 = true;
    [SerializeField] private bool enemiesHuntPlayer = true;

    [Header("Live (read only)")]
    [SerializeField] private State state = State.Idle;
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int leftToSpawn = 0;
    [SerializeField] private int aliveCount = 0;
    [SerializeField] private WaveParams currentParams;

    public State CurrentState => state;
    public int CurrentWave => currentWave;
    public int EnemiesRemaining => leftToSpawn + alive.Count;
    public float IntermissionTimeLeft => state == State.Intermission ? Mathf.Max(0f, timer) : 0f;

    private readonly HashSet<GameObject> alive = new HashSet<GameObject>();
    private float timer;

    private void Awake()
    {
        if (director == null) director = FindFirstObjectByType<EnemySpawnDirector>();
    }

    private void OnEnable()
    {
        EnemyHealth.Died += OnEnemyDied;
        if (director != null) director.EnemySpawned += OnEnemySpawned;
    }

    private void OnDisable()
    {
        EnemyHealth.Died -= OnEnemyDied;
        if (director != null) director.EnemySpawned -= OnEnemySpawned;
    }

    private void Start()
    {
        if (config == null) { Debug.LogError("WaveManager has no WaveConfig!", this); enabled = false; return; }
        if (director != null)
        {
            director.TestMode = false;
            director.useBudget = false;
        }

        if (adoptSceneEnemiesAsWave1 && AdoptSceneEnemies()) return;
        BeginIntermission(firstIntermission);
    }

    private void Update()
    {
        alive.RemoveWhere(e => e == null);  // anything destroyed some other way
        aliveCount = alive.Count;

        switch (state)
        {
            case State.Intermission:
                timer -= Time.deltaTime;
                if (timer <= 0f) StartWave(currentWave + 1);
                break;

            case State.Spawning:
                timer -= Time.deltaTime;
                if (timer <= 0f) timer = TrySpawnGroup() ? currentParams.spawnInterval : 0.25f;
                if (leftToSpawn <= 0) state = State.Clearing;
                break;

            case State.Clearing:
                if (alive.Count == 0) EndWave();
                break;
        }
    }

    // --- Round flow ---
    private void BeginIntermission(float seconds)
    {
        state = State.Intermission;
        timer = seconds;
        Debug.Log($"[Waves] Break: {seconds:0.#}s until wave {currentWave + 1}");
    }

    private void StartWave(int wave)
    {
        currentWave = wave;
        currentParams = config.Evaluate(wave);
        leftToSpawn = currentParams.enemyCount;
        timer = 0f;  // first group right away
        state = State.Spawning;
        Debug.Log($"[Waves] START {currentParams}");
    }

    private void EndWave()
    {
        Debug.Log($"[Waves] Wave {currentWave} CLEARED");
        BeginIntermission(currentParams.intermission);
    }

    // --- Spawning ---
    private bool TrySpawnGroup()
    {
        if (director == null || leftToSpawn <= 0) return false;

        int room = currentParams.maxAlive - alive.Count;
        if (room <= 0) return false;

        int size = Random.Range(currentParams.groupSizeMin, currentParams.groupSizeMax + 1);
        size = Mathf.Min(size, leftToSpawn, room);

        int point = PickSpawnPoint();
        if (point < 0) return false;

        var formation = size == 1
            ? EnemySpawnDirector.Formation.Single
            : (EnemySpawnDirector.Formation)Random.Range(1, director.FormationCount);

        int spawned = director.SpawnGroup(point, 0, formation, size);
        leftToSpawn -= spawned;
        return spawned > 0;
    }

    private int PickSpawnPoint()
    {
        bool[] valid = director.GetValidPoints();
        List<int> good = new List<int>();
        for (int i = 0; i < valid.Length; i++) if (valid[i]) good.Add(i);
        return good.Count == 0 ? -1 : good[Random.Range(0, good.Count)];
    }

    private void OnEnemySpawned(GameObject enemy)
    {
        alive.Add(enemy);
        RunnerChase r = enemy.GetComponent<RunnerChase>();
        if (r != null) r.huntPlayer = enemiesHuntPlayer;
    }

    private void OnEnemyDied(GameObject enemy)
    {
        alive.Remove(enemy);
    }

    // The 10 hand-placed runners become wave 1
    private bool AdoptSceneEnemies()
    {
        RunnerChase[] placed = FindObjectsByType<RunnerChase>(FindObjectsSortMode.None);
        if (placed.Length == 0) return false;

        currentWave = 1;
        currentParams = config.Evaluate(1);
        currentParams.enemyCount = placed.Length;
        foreach (RunnerChase r in placed) OnEnemySpawned(r.gameObject);

        state = State.Clearing;
        Debug.Log($"[Waves] START wave 1 with {placed.Length} hand-placed runners");
        return true;
    }
}