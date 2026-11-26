using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Grid of the map + "which way to the player" for every cell.
// You can have several (e.g. "default" for normal enemies, "big" for large ones);
// each enemy reads the one named in its movement script.
public class FlowField : MonoBehaviour
{
    private static readonly Dictionary<string, FlowField> fields = new Dictionary<string, FlowField>();

    // The normal field (old code that used Instance keeps working)
    public static FlowField Instance => Get("default");

    // Field by name, falling back to "default" if that name doesn't exist
    public static FlowField Get(string id)
    {
        if (!string.IsNullOrEmpty(id) && fields.TryGetValue(id, out FlowField f) && f != null) return f;
        return fields.TryGetValue("default", out FlowField d) ? d : null;
    }

    [Header("Identity")]
    [SerializeField] private string fieldId = "default";

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;   // Terrain

    [Header("Grid")]
    [Tooltip("1 = one cell per tile. 2 = half-tile cells (needed for big enemies to line up in doorways).")]
    [Range(1, 4)]
    [SerializeField] private int cellsPerTile = 1;

    [Header("Blockers")]
    [SerializeField] private LayerMask blockerLayers;            // Walls
    [Tooltip("Size (world units) of the box that has to be wall-free for a cell to be walkable. Roughly the enemy's body width.")]
    [Range(0.1f, 4f)]
    [SerializeField] private float blockCheckSize = 0.8f;

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float updateInterval = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool drawGrid = true;
    [SerializeField] private bool drawArrows = true;

    private Vector2 origin;     // world position of the grid's bottom-left corner
    private float cellSize;     // world units per cell
    private int width, height;
    private bool[] walkable;
    private int[] dist;
    private int[] next;
    private readonly Queue<int> queue = new Queue<int>();
    private float timer;

    private static readonly int[] DX = { 1, -1, 0, 0, 1, 1, -1, -1 };
    private static readonly int[] DY = { 0, 0, 1, -1, 1, -1, 1, -1 };

    public bool IsBuilt => walkable != null;

    private void Awake()
    {
        fields[fieldId] = this;
        BuildGrid();
    }

    private void OnDestroy()
    {
        if (fields.TryGetValue(fieldId, out FlowField f) && f == this) fields.Remove(fieldId);
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
        BoundsInt b = floorTilemap.cellBounds;
        Vector2 minW = floorTilemap.CellToWorld(b.min);
        Vector2 maxW = floorTilemap.CellToWorld(b.max);

        cellSize = floorTilemap.layoutGrid.cellSize.x / cellsPerTile;
        origin = minW;
        width = Mathf.CeilToInt((maxW.x - minW.x) / cellSize);
        height = Mathf.CeilToInt((maxW.y - minW.y) / cellSize);

        walkable = new bool[width * height];
        dist = new int[width * height];
        next = new int[width * height];

        Physics2D.SyncTransforms();
        Vector2 box = Vector2.one * blockCheckSize;
        int open = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 center = LocalToWorld(x, y);
                bool floor = floorTilemap.HasTile(floorTilemap.WorldToCell(center));
                bool blocked = floor && Physics2D.OverlapBox(center, box, 0f, blockerLayers) != null;
                walkable[x + y * width] = floor && !blocked;
                if (walkable[x + y * width]) open++;
            }
        }
        Debug.Log($"[FlowField:{fieldId}] Grid {width}x{height} ({cellSize:0.##}u cells), {open} walkable");
    }

    // ---------- Field (directions) ----------
    private void BuildField(Vector2 targetWorld)
    {
        if (!IsBuilt) return;
        for (int i = 0; i < dist.Length; i++) { dist[i] = -1; next[i] = -1; }
        if (!WorldToLocal(targetWorld, out int tx, out int ty)) return;

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

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int c = x + y * width;
                if (dist[c] <= 0) continue;
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

    private bool CanStep(int x, int y, int nx, int ny, int d)
    {
        if (!IsWalkable(nx, ny)) return false;
        if (d >= 4 && (!IsWalkable(nx, y) || !IsWalkable(x, ny))) return false;
        return true;
    }

    // ---------- What enemies call ----------
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
        x = Mathf.FloorToInt((world.x - origin.x) / cellSize);
        y = Mathf.FloorToInt((world.y - origin.y) / cellSize);
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public Vector2 LocalToWorld(int x, int y)
    {
        return origin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);
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

        Vector3 size = Vector3.one * cellSize * 0.9f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int c = x + y * width;
                Vector2 pos = LocalToWorld(x, y);

                if (drawGrid && (walkable[c] || floorTilemap.HasTile(floorTilemap.WorldToCell(pos))))
                {
                    Gizmos.color = walkable[c] ? new Color(0f, 1f, 0f, 0.1f) : new Color(1f, 0f, 0f, 0.4f);
                    Gizmos.DrawCube(pos, size);
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