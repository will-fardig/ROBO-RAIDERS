using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Grid of the map + "which way to the player" for every cell.
// Rebuilt a few times a second; every runner just looks up its own cell.
public class FlowField : MonoBehaviour
{
    public static FlowField Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;   // Terrain

    [Header("Blockers")]
    [SerializeField] private LayerMask blockerLayers;            // Walls
    [Range(0.1f, 1f)]
    [SerializeField] private float blockCheckSize = 0.8f;

    [Header("Target")]
    [SerializeField] private Transform target;                   // auto-finds the Spaceman
    [SerializeField] private float updateInterval = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool drawGrid = true;
    [SerializeField] private bool drawArrows = true;

    private BoundsInt bounds;
    private int width, height;
    private bool[] walkable;
    private int[] dist;        // steps to the target, -1 = can't reach
    private int[] next;        // index of the neighbour cell to walk to, -1 = none
    private readonly Queue<int> queue = new Queue<int>();
    private float timer;

    // 8 directions: right, left, up, down, then diagonals
    private static readonly int[] DX = { 1, -1, 0, 0, 1, 1, -1, -1 };
    private static readonly int[] DY = { 0, 0, 1, -1, 1, -1, 1, -1 };

    public bool IsBuilt => walkable != null;

    private void Awake()
    {
        Instance = this;
        BuildGrid();
    }

    private void Start()
    {
        if (target == null)
        {
            Spaceman s = FindFirstObjectByType<Spaceman>();
            if (s != null) target = s.transform;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = updateInterval;
        if (target != null) BuildField(target.position);
    }

    // ---------- Grid (walls) ----------
    [ContextMenu("Build Grid")]
    public void BuildGrid()
    {
        if (floorTilemap == null) { Debug.LogError("FlowField needs the floor tilemap!", this); return; }

        floorTilemap.CompressBounds();
        bounds = floorTilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;
        walkable = new bool[width * height];
        dist = new int[width * height];
        next = new int[width * height];

        Physics2D.SyncTransforms();
        Vector2 box = floorTilemap.layoutGrid.cellSize * blockCheckSize;
        int open = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int cell = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                bool floor = floorTilemap.HasTile(cell);
                bool blocked = floor && Physics2D.OverlapBox(floorTilemap.GetCellCenterWorld(cell), box, 0f, blockerLayers) != null;
                walkable[x + y * width] = floor && !blocked;
                if (walkable[x + y * width]) open++;
            }
        }
        Debug.Log($"[FlowField] Grid {width}x{height}, {open} walkable cells");
    }

    // ---------- Field (directions) ----------
    private void BuildField(Vector2 targetWorld)
    {
        if (!IsBuilt) return;
        for (int i = 0; i < dist.Length; i++) { dist[i] = -1; next[i] = -1; }
        if (!WorldToLocal(targetWorld, out int tx, out int ty)) return;

        // Flood outward from the player: each cell learns how many steps away it is
        int start = tx + ty * width;
        dist[start] = 0;
        queue.Clear();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            int c = queue.Dequeue();
            int cx = c % width, cy = c / width;
            for (int d = 0; d < 8; d++)
            {
                int nx = cx + DX[d], ny = cy + DY[d];
                if (!CanStep(cx, cy, nx, ny, d)) continue;
                int n = nx + ny * width;
                if (dist[n] != -1) continue;
                dist[n] = dist[c] + 1;
                queue.Enqueue(n);
            }
        }

        // Each cell points at its neighbour that's closest to the player
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int c = x + y * width;
                if (dist[c] <= 0) continue;   // unreachable, or the player's own cell
                int best = -1, bestDist = dist[c];
                for (int d = 0; d < 8; d++)
                {
                    int nx = x + DX[d], ny = y + DY[d];
                    if (!CanStep(x, y, nx, ny, d)) continue;
                    int n = nx + ny * width;
                    if (dist[n] >= 0 && dist[n] < bestDist) { bestDist = dist[n]; best = n; }
                }
                next[c] = best;
            }
        }
    }

    // Can you walk from (x,y) to the neighbour (nx,ny)? Diagonals can't cut wall corners.
    private bool CanStep(int x, int y, int nx, int ny, int d)
    {
        if (!IsWalkable(nx, ny)) return false;
        if (d >= 4 && (!IsWalkable(nx, y) || !IsWalkable(x, ny))) return false;
        return true;
    }

    // ---------- What runners call ----------
    // Direction to walk from this world position. Zero = no route (or already at the player).
    public Vector2 GetDirection(Vector2 worldPos)
    {
        if (!IsBuilt || !WorldToLocal(worldPos, out int x, out int y)) return Vector2.zero;
        int n = next[x + y * width];
        if (n < 0) return Vector2.zero;
        Vector2 toNext = LocalToWorld(n % width, n / width) - worldPos;
        return toNext.sqrMagnitude > 0.0001f ? toNext.normalized : Vector2.zero;
    }

    // ---------- Helpers ----------
    public bool WorldToLocal(Vector2 world, out int x, out int y)
    {
        Vector3Int cell = floorTilemap.WorldToCell(world);
        x = cell.x - bounds.xMin;
        y = cell.y - bounds.yMin;
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public Vector2 LocalToWorld(int x, int y)
    {
        return floorTilemap.GetCellCenterWorld(new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0));
    }

    public bool IsWalkable(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height && walkable[x + y * width];
    }

    // ---------- Scene view ----------
    private void OnDrawGizmosSelected()
    {
        if (floorTilemap == null) return;
        if (!IsBuilt) BuildGrid();

        Vector3 size = floorTilemap.layoutGrid.cellSize * 0.9f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int c = x + y * width;
                Vector2 pos = LocalToWorld(x, y);

                if (drawGrid)
                {
                    Vector3Int cell = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                    if (walkable[c] || floorTilemap.HasTile(cell))
                    {
                        Gizmos.color = walkable[c] ? new Color(0f, 1f, 0f, 0.1f) : new Color(1f, 0f, 0f, 0.4f);
                        Gizmos.DrawCube(pos, size);
                    }
                }

                if (drawArrows && Application.isPlaying && next[c] >= 0)
                {
                    Vector2 to = LocalToWorld(next[c] % width, next[c] / width);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(pos, Vector2.Lerp(pos, to, 0.6f));
                }
            }
        }
    }
}