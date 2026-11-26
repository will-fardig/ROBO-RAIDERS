using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// spawns enemies at points on a grid
// the RL agent will call ExecuteAction() later to decide where/what/how many
// for now theres a test mode that just spawns random stuff so we can see it working
public class EnemySpawnDirector : MonoBehaviour
{
    // group shapes
    public enum Formation { Single, Cluster, Line, Ring }

    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;   // slot 0 = runner, add more later
    [SerializeField] private int[] enemyCosts;            // cost for each

    [Header("Grid")]
    [SerializeField] private Tilemap groundTilemap;      
    [SerializeField] private int columns = 8;
    [SerializeField] private int rows = 6;
    [SerializeField] private float edgePadding = 1f;      // keep points away from the walls

    [Header("Spawn Rules")]
    [SerializeField] private float minDistFromPlayer = 5f; // dont spawn right on top of him
    [SerializeField] private int maxEnemies = 40;
    [SerializeField] private int maxGroupSize = 8;
    [SerializeField] private float spacing = 0.8f;         // gap between enemies in a group
    [SerializeField] private float pointCooldown = 2f;     // wait before same point can spawn again
    [SerializeField] private LayerMask obstacleLayer;     

    [Header("Budget")]
    public float budget = 10f;
    [SerializeField] private float maxBudget = 30f;
    [SerializeField] private float budgetPerSecond = 1f;
    private float startBudget;

    [Header("Testing")]
    [SerializeField] private bool testMode = true;  // TURN OFF when the AI takes over
    [SerializeField] private float testSpawnTime = 3f;
    private float testTimer = 0f;

    // grid stuff
    private Vector2[] points;
    private float[] cooldowns;
    private Bounds mapBounds;

    // tracking
    private List<GameObject> aliveEnemies = new List<GameObject>();
    public int totalSpawned = 0;
    public int totalKilled = 0;

    private Spaceman spaceman;

    // handy for the agent later
    public int PointCount => points.Length;
    public int EnemyTypeCount => enemyPrefabs.Length;
    public int FormationCount => 4;
    public int AliveCount => aliveEnemies.Count;

    void Awake()
    {
        startBudget = budget;
        MakeGrid();
    }

    void Start()
    {
        spaceman = FindFirstObjectByType<Spaceman>();
    }

    void Update()
    {
        // budget goes up over time
        budget += budgetPerSecond * Time.deltaTime;
        if (budget > maxBudget) budget = maxBudget;

        // remove dead enemies from the list
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
                totalKilled++;
            }
        }

        // random spawning for testing
        if (testMode == true)
        {
            testTimer -= Time.deltaTime;
            if (testTimer <= 0)
            {
                testTimer = testSpawnTime;
                RandomSpawn();
            }
        }
    }

    // Grid
    void MakeGrid()
    {
        points = new Vector2[columns * rows];
        cooldowns = new float[columns * rows];

        if (groundTilemap == null)
        {
            if (Application.isPlaying) Debug.LogError("No ground tilemap on the spawn director!");
            return;
        }

        mapBounds = CamFollow.GetTilemapWorldBounds(groundTilemap);

        float width = mapBounds.size.x - edgePadding * 2;
        float height = mapBounds.size.y - edgePadding * 2;
        float cellW = width / columns;
        float cellH = height / rows;

        
        // index goes left to right, bottom to top (index = row * columns + col)
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float x = mapBounds.min.x + edgePadding + cellW * (c + 0.5f);
                float y = mapBounds.min.y + edgePadding + cellH * (r + 0.5f);
                points[r * columns + c] = new Vector2(x, y);
            }
        }
    }

    // Agent will call this later
    public int ExecuteAction(int point, int type, int formation, int size)
    {
        if (point < 0 || point >= points.Length) return 0; // do nothing
        return SpawnGroup(point, type, (Formation)formation, size + 1);
    }

    // returns how many actually spawned
    public int SpawnGroup(int point, int type, Formation formation, int count)
    {
        if (CanSpawnAt(point) == false) return 0;
        if (type < 0 || type >= enemyPrefabs.Length) return 0;
        if (enemyPrefabs[type] == null) return 0;

        if (formation == Formation.Single) count = 1;

        // limits
        if (count > maxGroupSize) count = maxGroupSize;
        if (count > maxEnemies - aliveEnemies.Count) count = maxEnemies - aliveEnemies.Count;

        int cost = GetCost(type);
        if (cost > 0)
        {
            int canAfford = Mathf.FloorToInt(budget / cost);
            if (count > canAfford) count = canAfford;
        }
        if (count <= 0) return 0;

        // which way is the player
        Vector2 center = points[point];
        Vector2 toPlayer = Vector2.right;
        if (spaceman != null) toPlayer = ((Vector2)spaceman.transform.position - center).normalized;

        int spawned = 0;
        int tries = 0;
        // in case some spots are blocked
        while (spawned < count && tries < count * 3)
        {
            Vector2 pos = center + GetOffset(formation, tries, count, toPlayer);
            tries++;

            if (SpotIsClear(pos) == false) continue;

            GameObject enemy = Instantiate(enemyPrefabs[type], pos, Quaternion.identity);
            aliveEnemies.Add(enemy);
            budget -= cost;
            spawned++;
        }

        if (spawned > 0)
        {
            cooldowns[point] = Time.time + pointCooldown;
            totalSpawned += spawned;
            Debug.Log($"Spawned {spawned} enemies at point {point} ({formation})");
        }
        return spawned;
    }

    // where each enemy goes in the group
    Vector2 GetOffset(Formation formation, int i, int count, Vector2 toPlayer)
    {
        if (formation == Formation.Line)
        {
            // sideways line facing the player
            Vector2 side = new Vector2(-toPlayer.y, toPlayer.x);
            return side * (i - (count - 1) / 2f) * spacing;
        }
        else if (formation == Formation.Ring)
        {
            // circle around the point
            float radius = Mathf.Max(spacing, spacing * count / 6f);
            float angle = i * (360f / count) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        else
        {
            // cluster
            if (i == 0) return Vector2.zero;
            float angle = i * 137.5f * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spacing * Mathf.Sqrt(i);
        }
    }

    // Checks if can spawn
    public bool CanSpawnAt(int point)
    {
        if (point < 0 || point >= points.Length) return false;
        if (aliveEnemies.Count >= maxEnemies) return false;
        if (Time.time < cooldowns[point]) return false;

        if (spaceman != null)
        {
            float dist = Vector2.Distance(points[point], spaceman.transform.position);
            if (dist < minDistFromPlayer) return false;
        }

        return SpotIsClear(points[point]);
    }

    bool SpotIsClear(Vector2 pos)
    {
        if (groundTilemap == null) return true;

        // has to be on an actual tile
        if (groundTilemap.HasTile(groundTilemap.WorldToCell(pos)) == false) return false;

        // not in a wall
        if (obstacleLayer != 0 && Physics2D.OverlapCircle(pos, 0.4f, obstacleLayer) != null) return false;

        return true;
    }

    int GetCost(int type)
    {
        if (enemyCosts == null || type >= enemyCosts.Length) return 1; // default if i forgot to set it
        return enemyCosts[type];
    }

  
    // where the agent can pick
    public bool[] GetValidPoints()
    {
        bool[] valid = new bool[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            valid[i] = CanSpawnAt(i);
        }
        return valid;
    }

    //gets info for agent
    public List<float> GetInfo()
    {
        List<float> info = new List<float>();

        info.Add(budget / maxBudget);
        info.Add((float)aliveEnemies.Count / maxEnemies);
        if (spaceman != null) info.Add((float)spaceman.Lives / spaceman.MaxLives);
        else info.Add(0);

        float mapSize = mapBounds.size.magnitude;

        for (int i = 0; i < points.Length; i++)
        {
            info.Add(CanSpawnAt(i) ? 1 : 0);
            info.Add(Mathf.Clamp01(EnemiesNear(points[i], 3f) / (float)maxGroupSize));

            if (spaceman != null)
                info.Add(Mathf.Clamp01(Vector2.Distance(points[i], spaceman.transform.position) / mapSize));
            else
                info.Add(1);
        }
        return info;
    }

    int EnemiesNear(Vector2 pos, float range)
    {
        int count = 0;
        foreach (GameObject e in aliveEnemies)
        {
            if (e != null && Vector2.Distance(e.transform.position, pos) < range) count++;
        }
        return count;
    }

    // resets everything
    public void ResetDirector()
    {
        foreach (GameObject e in aliveEnemies)
        {
            if (e != null) Destroy(e);
        }
        aliveEnemies.Clear();
        budget = startBudget;
        totalSpawned = 0;
        totalKilled = 0;
        for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] = 0;
    }

    // test mode
    void RandomSpawn()
    {
        // find all the points we can use
        List<int> goodPoints = new List<int>();
        for (int i = 0; i < points.Length; i++)
        {
            if (CanSpawnAt(i)) goodPoints.Add(i);
        }
        if (goodPoints.Count == 0) return;

        int point = goodPoints[Random.Range(0, goodPoints.Count)];
        int type = Random.Range(0, enemyPrefabs.Length);
        int formation = Random.Range(0, FormationCount);
        int size = Random.Range(0, 4); // 1 to 4 enemies

        ExecuteAction(point, type, formation, size);
    }

    
}