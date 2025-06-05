using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MIConvexHull;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    public int roomCount = 10;
    public Vector2 minRoomSize = new Vector2(4, 4);
    public Vector2 maxRoomSize = new Vector2(10, 10);
    public Vector2 dungeonSize = new Vector2(50, 50);
    public float minSeparation = 0.2f;  // Minimum movement per step
    public int hallwayWidth = 3;
    public int gridSize = 1;

    [Header("Rendering Settings")]
    [SerializeField] private Transform _floorParent; // Parent object for floor tiles
    [SerializeField] private GameObject _floorTilePrefab; // Prefab with SpriteRenderer
    [SerializeField] private bool _showDebugGizmos = false; // Toggle for visualization

    [Header("Floor Tile Settings")]
    [SerializeField] private List<Sprite> floorTypeASprites; // 5/8 chance variants
    [SerializeField] private List<Sprite> floorTypeBSprites; // 2/8 chance variants
    [SerializeField] private List<Sprite> floorTypeCSprites; // 1/8 chance variants

    private Dictionary<Vector2Int, int> _floorTileTypes = new Dictionary<Vector2Int, int>(); // 1=A, 2=B, 3=C
    private List<Vector2Int> _orderedWalkableTiles; // Cache processing order

    [Header("Wall Rendering Settings")]
    [SerializeField] private Transform _wallParent; // Parent object for wall tiles
    [SerializeField] private GameObject _wallTilePrefab; // Prefab with SpriteRenderer

    [Header("Wall Tile Settings")]
    [SerializeField] private List<Sprite> wallTop; // All top wall variants
    [SerializeField] private List<Sprite> wallSideLeft; // All left side variants
    [SerializeField] private List<Sprite> wallSideRight; // All right side variants
    [SerializeField] private List<Sprite> wallBottm; // All bottom variants
    [SerializeField] private List<Sprite> wallInnerCornerDownLeft; // All inner corner variants
    [SerializeField] private List<Sprite> wallInnerCornerDownRight; // All inner corner variants
    [SerializeField] private List<Sprite> wallDiagonalCornerDownLeft; // All diagonal corner variants
    [SerializeField] private List<Sprite> wallDiagonalCornerDownRight; // All diagonal corner variants
    [SerializeField] private List<Sprite> wallDiagonalCornerUpLeft; // All diagonal corner variants
    [SerializeField] private List<Sprite> wallDiagonalCornerUpRight; // All diagonal corner variants
    [SerializeField] private List<Sprite> wallFull; // Full wall variants (optional)

    private bool[,] walkableGrid;
    private int gridWidth, gridHeight;
    private Vector2 gridOrigin;

    private List<Room> rooms = new List<Room>();
    private HashSet<Room> mainRooms = new HashSet<Room>();
    private List<Corridor> corridors = new List<Corridor>();
    private List<Room> newlyAddedNonMainRooms = new List<Room>();
    private HashSet<Vector2Int> walkableTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();
    public bool GenerationComplete { get; private set; }

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossAltarPrefab;
    public List<Room> bossRooms { get; private set; } = new List<Room>();

    [Header("Boss Barrier Settings")]
    [SerializeField] private Vector2 barrierSize = new Vector2(10f, 10f); // Square dimensions
    [SerializeField] private GameObject fencePrefab; // Your fence visual

    private Dictionary<BossAltar, List<GameObject>> activeBarriers = new Dictionary<BossAltar, List<GameObject>>();

    //[SerializeField] private Transform Fanceparent;

    void Start()
    {
        GenerateRooms();
        InitializeGrid();
        SeparateRooms();
        SelectMainRooms();
        GenerateTriangulation();
        AddNonMainRooms();
        UpdateWalkableGrid();
        IdentifyBossRooms();
        MarkStartEndRooms(); // <-- Add this line
        GenerationComplete = true;
    }

    void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            float width = Random.Range(minRoomSize.x, maxRoomSize.x);
            float height = Random.Range(minRoomSize.y, maxRoomSize.y);
            Vector2 position = new Vector2(Random.Range(-dungeonSize.x / 2, dungeonSize.x / 2),
                                           Random.Range(-dungeonSize.y / 2, dungeonSize.y / 2));

            Room newRoom = new Room(position, width, height);
            rooms.Add(newRoom);

            // Mark room in grid
            MarkAreaAsWalkable(newRoom.position, newRoom.width, newRoom.height);
        }
    }

    void SeparateRooms()
    {
        bool hasOverlap;
        int iterations = 0;
        int maxIterations = 100;

        //Vector2 dungeonCenter = new Vector2(transform.position.x, transform.position.y);

        do
        {
            hasOverlap = false;

            foreach (var roomA in rooms)
            {
                foreach (var roomB in rooms)
                {
                    if (roomA == roomB) continue;

                    if (AreOverlapping(roomA, roomB, out float overlapX, out float overlapY))
                    {
                        hasOverlap = true;

                        Vector2 moveDirection = (roomA.position - roomB.position).normalized;

                        if (overlapX < overlapY)
                        {
                            roomA.position.x += moveDirection.x * Mathf.Max(overlapX, minSeparation);
                        }
                        else
                        {
                            roomA.position.y += moveDirection.y * Mathf.Max(overlapY, minSeparation);
                        }
                    }
                }
            }

            iterations++;
        }
        while (hasOverlap && iterations < maxIterations);
    }
    void SelectMainRooms()
    {
        int mainRoomCount = Mathf.CeilToInt(rooms.Count * 0.3f);
        mainRooms = new HashSet<Room>(rooms.OrderByDescending(r => r.width * r.height).Take(mainRoomCount));
    }
    void MarkStartEndRooms()
    {
        // Clear previous marks
        foreach (var room in rooms)
        {
            room.IsStartRoom = false;
            room.IsEndRoom = false;
        }

        // 1. Use largest room as initial candidate (your original approach)
        var startCandidate = rooms.OrderByDescending(r => r.width * r.height).First();

        // 2. Find farthest room from it (end room)
        var endRoom = FindFarthestRoom(startCandidate);
        endRoom.IsEndRoom = true;

        // 3. Find farthest from end room (true start room)
        var trueStartRoom = FindFarthestRoom(endRoom);
        trueStartRoom.IsStartRoom = true;
    }

    void IdentifyBossRooms()
    {
        bossRooms.Clear();
        var potentialBossRooms = mainRooms.OrderBy(r => r.width * r.height).Take(4).ToList();

        foreach (Room room in potentialBossRooms)
        {
            if (bossAltarPrefab == null) continue;

            GameObject altar = Instantiate(bossAltarPrefab, room.position, Quaternion.identity);
            BossAltar bossAltar = altar.GetComponent<BossAltar>();

            if (bossAltar != null)
            {
                bossAltar.bossRoom = room;
                bossRooms.Add(room);
                //Debug.Log($"walkableTiles count: {walkableTiles.Count}");

                RegisterBossAltar(bossAltar); // This creates the barriers
                altar.SetActive(false); // Start hidden
            }
        }
    }

    public void StartBossWave(int bossIndex)
    {
        //Debug.Log("start boss wave");
        if (bossIndex >= 0 && bossIndex < bossRooms.Count)
        {
            // Find all existing altars (created at dungeon generation)
            BossAltar[] altars = FindObjectsOfType<BossAltar>(true); // Include inactive
            //Debug.Log($"Found {altars.Length} boss altars, trying to activate index {bossIndex}");

            if (bossIndex < altars.Length)
            {
                // Activate the corresponding altar
                altars[bossIndex].gameObject.SetActive(true);
                altars[bossIndex].bossWaveIndex = bossIndex; // Add this field to BossAltar
                //Debug.Log("bactivate alter");
                altars[bossIndex].ActivateAltar();
            }
        }
    }

    public void RegisterBossAltar(BossAltar altar)
    {
        if (altar == null || altar.bossRoom == null) return;

        Vector2 center = altar.bossRoom.position;
        float width = altar.bossRoom.width;
        float height = altar.bossRoom.height;

        List<GameObject> fences = new List<GameObject>();

        // Calculate boundaries (integer positions)
        int left = Mathf.FloorToInt(center.x - width / 2);
        int right = Mathf.CeilToInt(center.x + width / 2);
        int bottom = Mathf.FloorToInt(center.y - height / 2);
        int top = Mathf.CeilToInt(center.y + height / 2);

        //Debug.Log($"=== Starting Barrier Placement for Room at {center} ===");
        //Debug.Log($"Boundaries - Left:{left} Right:{right} Bottom:{bottom} Top:{top}");

        // Place barriers only at boundaries
        for (int x = left; x <= right; x++)
        {
            Vector2Int bottomPos = new Vector2Int(x, bottom);
            Vector2Int topPos = new Vector2Int(x, top);

            //Debug.Log($"Checking bottom tile at {bottomPos} - Walkable: {walkableTiles.Contains(bottomPos)}");
            TryPlaceBarrier(x, bottom, fences);

            //Debug.Log($"Checking top tile at {topPos} - Walkable: {walkableTiles.Contains(topPos)}");
            TryPlaceBarrier(x, top, fences);
        }

        for (int y = bottom + 1; y < top; y++)
        {
            Vector2Int leftPos = new Vector2Int(left, y);
            Vector2Int rightPos = new Vector2Int(right, y);

            //Debug.Log($"Checking left tile at {leftPos} - Walkable: {walkableTiles.Contains(leftPos)}");
            TryPlaceBarrier(left, y, fences);

            //Debug.Log($"Checking right tile at {rightPos} - Walkable: {walkableTiles.Contains(rightPos)}");
            TryPlaceBarrier(right, y, fences);
        }

        //Debug.Log($"=== Finished Barrier Placement - Created {fences.Count} barriers ===");
        activeBarriers[altar] = fences;
    }

    void TryPlaceBarrier(int x, int y, List<GameObject> fences)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (walkableTiles.Contains(pos))
        {
            //Debug.Log($"Placing barrier at {pos} (Walkable)");
            GameObject fence = Instantiate(fencePrefab, new Vector3(x, y, 0), Quaternion.identity);
            fence.SetActive(false);
            fences.Add(fence);
        }
        else
        {
            //Debug.Log($"Skipping barrier at {pos} (Not Walkable)");
        }
    }

    public void ActivateBarriers(BossAltar altar)
    {
        if (activeBarriers.TryGetValue(altar, out var fences))
        {
            foreach (var fence in fences)
                fence.SetActive(true);
        }
    }

    public void DeactivateBarriers(BossAltar altar)
    {
        if (activeBarriers.TryGetValue(altar, out var fences))
        {
            foreach (var fence in fences)
            {
                fence.SetActive(false);
                Destroy(fence);
            }
        }
    }

    private Room FindFarthestRoom(Room start)
    {
        // Fallback if no corridors
        if (corridors.Count == 0) return start;

        var visited = new Dictionary<Room, int>();
        var queue = new Queue<Room>();
        visited[start] = 0;
        queue.Enqueue(start);

        Room farthest = start;
        int maxDistance = 0;

        List<string> connectionLog = new List<string>();

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            foreach (var corridor in corridors)
            {
                Room neighbor = null;
                if (corridor.A == current) neighbor = corridor.B;
                if (corridor.B == current) neighbor = corridor.A;

                if (neighbor != null && !visited.ContainsKey(neighbor))
                {
                    int distance = visited[current] + 1;
                    visited[neighbor] = distance;
                    connectionLog.Add($"{current.position} -> {neighbor.position} (distance: {distance})");

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthest = neighbor;
                    }
                    queue.Enqueue(neighbor);
                }
            }
        }
        return farthest;
    }

    bool AreOverlapping(Room a, Room b, out float overlapX, out float overlapY)
    {
        float deltaX = Mathf.Abs(a.position.x - b.position.x);
        float deltaY = Mathf.Abs(a.position.y - b.position.y);

        float totalWidth = (a.width + b.width) / 2;
        float totalHeight = (a.height + b.height) / 2;

        overlapX = totalWidth - deltaX;
        overlapY = totalHeight - deltaY;

        return overlapX > 0 && overlapY > 0;
    }

    void GenerateTriangulation()
    {
        var nodes = mainRooms.Select(r => new RoomNode(r)).ToList();
        var triangulation = DelaunayTriangulation<RoomNode, DefaultTriangulationCell<RoomNode>>.Create(nodes, 1e-10);

        List<Edge> edges = new List<Edge>();

        // Convert triangulation into a weighted graph
        foreach (var cell in triangulation.Cells)
        {
            AddEdge(edges, cell.Vertices[0].RoomData, cell.Vertices[1].RoomData);
            AddEdge(edges, cell.Vertices[1].RoomData, cell.Vertices[2].RoomData);
            AddEdge(edges, cell.Vertices[2].RoomData, cell.Vertices[0].RoomData);
        }

        // Generate the MST
        List<Edge> mst = KruskalsAlgorithm(edges);

        // Add some extra connections for loops
        int extraEdges = Mathf.CeilToInt(edges.Count * 0f); // 20% extra connections
        mst.AddRange(edges.OrderBy(e => Random.value).Take(extraEdges));

        // Create corridors from MST
        foreach (var edge in mst)
        {
            ConnectRooms(edge.A, edge.B);
        }
    }

    void ConnectRooms(Room a, Room b)
    {
        Vector2 start = a.position;
        Vector2 end = b.position;

        if (Random.value > 0.5f)
        {
            corridors.Add(new Corridor(a, b, start, new Vector2(end.x, start.y)));
            corridors.Add(new Corridor(a, b, new Vector2(end.x, start.y), end));

            // Mark corridors in grid
            MarkCorridorAsWalkable(start, new Vector2(end.x, start.y));
            MarkCorridorAsWalkable(new Vector2(end.x, start.y), end);
        }
        else
        {
            corridors.Add(new Corridor(a, b, start, new Vector2(start.x, end.y)));
            corridors.Add(new Corridor(a, b, new Vector2(start.x, end.y), end));

            // Mark corridors in grid
            MarkCorridorAsWalkable(start, new Vector2(start.x, end.y));
            MarkCorridorAsWalkable(new Vector2(start.x, end.y), end);
        }
    }

    List<Edge> KruskalsAlgorithm(List<Edge> edges)
    {
        edges.Sort((a, b) => a.Weight.CompareTo(b.Weight)); // Sort edges by weight
        Dictionary<Room, Room> parent = new Dictionary<Room, Room>();

        foreach (var room in mainRooms) parent[room] = room; // Initialize sets

        List<Edge> mst = new List<Edge>();

        foreach (var edge in edges)
        {
            Room rootA = Find(parent, edge.A);
            Room rootB = Find(parent, edge.B);

            if (rootA != rootB) // If they are in different sets, connect them
            {
                mst.Add(edge);
                parent[rootA] = rootB; // Merge sets
            }
        }
        return mst;
    }

    void AddEdge(List<Edge> edges, Room a, Room b)
    {
        float distance = Vector2.Distance(a.position, b.position);
        edges.Add(new Edge(a, b, distance));
    }


    Room Find(Dictionary<Room, Room> parent, Room room)
    {
        if (parent[room] != room)
            parent[room] = Find(parent, parent[room]); // Path compression
        return parent[room];
    }

    void AddNonMainRooms()
    {
        newlyAddedNonMainRooms.Clear(); // Reset list

        foreach (var room in rooms)
        {
            if (mainRooms.Contains(room)) continue; // Skip main rooms

            // Check if the room touches a hallway
            bool touchesHallway = corridors.Any(corridor => IsTouchingHallway(room, corridor));
            if (!touchesHallway) continue;

            // Check if the room overlaps with any main room
            bool overlapsMain = mainRooms.Any(mainRoom => AreOverlapping(room, mainRoom, out _, out _));
            if (overlapsMain) continue;

            // If it touches a hallway and doesn't overlap, add it
            newlyAddedNonMainRooms.Add(room);
        }
    }
    bool IsTouchingHallway(Room room, Corridor corridor)
    {
        float roomLeft = room.position.x - room.width / 2;
        float roomRight = room.position.x + room.width / 2;
        float roomBottom = room.position.y - room.height / 2;
        float roomTop = room.position.y + room.height / 2;

        float corridorLeft = Mathf.Min(corridor.start.x, corridor.end.x);
        float corridorRight = Mathf.Max(corridor.start.x, corridor.end.x);
        float corridorBottom = Mathf.Min(corridor.start.y, corridor.end.y);
        float corridorTop = Mathf.Max(corridor.start.y, corridor.end.y);

        bool overlapsX = roomRight >= corridorLeft && roomLeft <= corridorRight;
        bool overlapsY = roomTop >= corridorBottom && roomBottom <= corridorTop;

        return overlapsX && overlapsY;
    }
    void InitializeGrid()
    {
        gridWidth = Mathf.CeilToInt(dungeonSize.x / gridSize);
        gridHeight = Mathf.CeilToInt(dungeonSize.y / gridSize);
        gridOrigin = new Vector2(-dungeonSize.x / 2, -dungeonSize.y / 2);
        walkableGrid = new bool[gridWidth, gridHeight];
    }
    void MarkAreaAsWalkable(Vector2 pos, float width, float height)
    {
        int startX = Mathf.FloorToInt((pos.x - width / 2 - gridOrigin.x) / gridSize);
        int endX = Mathf.CeilToInt((pos.x + width / 2 - gridOrigin.x) / gridSize);
        int startY = Mathf.FloorToInt((pos.y - height / 2 - gridOrigin.y) / gridSize);
        int endY = Mathf.CeilToInt((pos.y + height / 2 - gridOrigin.y) / gridSize);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                    walkableGrid[x, y] = true;
            }
        }
    }
    void MarkCorridorAsWalkable(Vector2 start, Vector2 end)
    {
        int startX = Mathf.FloorToInt((start.x - gridOrigin.x) / gridSize);
        int startY = Mathf.FloorToInt((start.y - gridOrigin.y) / gridSize);
        int endX = Mathf.FloorToInt((end.x - gridOrigin.x) / gridSize);
        int endY = Mathf.FloorToInt((end.y - gridOrigin.y) / gridSize);

        int minX = Mathf.Min(startX, endX);
        int maxX = Mathf.Max(startX, endX);
        int minY = Mathf.Min(startY, endY);
        int maxY = Mathf.Max(startY, endY);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                    walkableGrid[x, y] = true;
            }
        }
    }
    void UpdateWalkableGrid()
    {
        walkableTiles.Clear();

        // Mark all main and non-main rooms as walkable
        foreach (var room in mainRooms.Concat(newlyAddedNonMainRooms))
        {
            for (int x = Mathf.FloorToInt(room.position.x - room.width / 2); x < Mathf.CeilToInt(room.position.x + room.width / 2); x++)
            {
                for (int y = Mathf.FloorToInt(room.position.y - room.height / 2); y < Mathf.CeilToInt(room.position.y + room.height / 2); y++)
                {
                    walkableTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        // Mark hallways as walkable
        foreach (var corridor in corridors)
        {
            Vector2Int start = new Vector2Int(Mathf.RoundToInt(corridor.start.x), Mathf.RoundToInt(corridor.start.y));
            Vector2Int end = new Vector2Int(Mathf.RoundToInt(corridor.end.x), Mathf.RoundToInt(corridor.end.y));

            foreach (var point in GetCorridorTiles(start, end))
            {
                walkableTiles.Add(point);
            }
        }
        CalculateWallTiles();
        AssignFloorTileTypes(); // Add this line
        RenderFloorTiles();
        RenderWallTiles();
    }

    List<Vector2Int> GetCorridorTiles(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        if (start.x == end.x) // Vertical hallway
        {
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            for (int y = minY; y <= maxY; y++)
            {
                for (int i = -hallwayWidth / 2; i <= hallwayWidth / 2; i++)
                {
                    tiles.Add(new Vector2Int(start.x + i, y));
                }
            }
        }
        else if (start.y == end.y) // Horizontal hallway
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            for (int x = minX; x <= maxX; x++)
            {
                for (int i = -hallwayWidth / 2; i <= hallwayWidth / 2; i++)
                {
                    tiles.Add(new Vector2Int(x, start.y + i));
                }
            }
        }

        return tiles;
    }
    void CalculateWallTiles()
    {
        wallTiles.Clear();

        // Directions for checking adjacent tiles
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up + Vector2Int.left,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.down + Vector2Int.left,
        Vector2Int.down + Vector2Int.right
        };

        foreach (Vector2Int walkableTile in walkableTiles)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = walkableTile + dir;

                // If this neighbor isn't walkable, it's a potential wall
                if (!walkableTiles.Contains(neighbor))
                {
                    wallTiles.Add(neighbor);
                }
            }
        }
    }
    void AssignFloorTileTypes()
    {
        _floorTileTypes.Clear();
        _orderedWalkableTiles = walkableTiles.OrderBy(t => t.y).ThenBy(t => t.x).ToList();

        foreach (Vector2Int tile in _orderedWalkableTiles)
        {
            // Calculate probabilities with neighbor influence
            float[] probs = CalculateTileProbabilities(tile);
            _floorTileTypes[tile] = SelectTileType(probs);
        }
    }

    float[] CalculateTileProbabilities(Vector2Int tile)
    {
        float[] probs = { 6f / 10f, 3f / 10f, 1f / 10f }; // Base chances

        // Check 4 key neighbors (left, up-left, up, up-right)
        Vector2Int[] neighborOffsets = {
        Vector2Int.left,
        Vector2Int.up + Vector2Int.left,
        Vector2Int.up,
        Vector2Int.up + Vector2Int.right
    };

        foreach (var offset in neighborOffsets)
        {
            if (_floorTileTypes.TryGetValue(tile + offset, out int neighborType))
            {
                probs[1] += neighborType == 2 ? 0.10f : 0f; // Boost B
                probs[2] += neighborType == 3 ? 0.10f : 0f; // Boost C
            }
        }

        // Keep probabilities balanced
        float total = probs.Sum();
        return new[] { probs[0] / total, probs[1] / total, probs[2] / total };
    }

    int SelectTileType(float[] probabilities)
    {
        float rand = Random.Range(0f, 1f);
        return rand < probabilities[0] ? 1 :
               rand < probabilities[0] + probabilities[1] ? 2 : 3;
    }

    void RenderFloorTiles()
    {
        // Clear existing tiles if any
        foreach (Transform child in _floorParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new tiles
        foreach (var kvp in _floorTileTypes)
        {
            Vector2Int position = kvp.Key;
            int type = kvp.Value;

            GameObject tile = Instantiate(_floorTilePrefab,
                                        new Vector3(position.x, position.y, 0),
                                        Quaternion.identity,
                                        _floorParent);

            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            renderer.sprite = GetRandomSpriteForType(type);
        }
    }

    Sprite GetRandomSpriteForType(int type)
    {
        return type switch
        {
            1 => floorTypeASprites[Random.Range(0, floorTypeASprites.Count)],
            2 => floorTypeBSprites[Random.Range(0, floorTypeBSprites.Count)],
            3 => floorTypeCSprites[Random.Range(0, floorTypeCSprites.Count)],
            _ => floorTypeASprites[0] // fallback
        };
    }
    private void RenderWallTiles()
    {
        // Clear existing walls
        foreach (Transform child in _wallParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Vector2Int wallPos in wallTiles)
        {
            // First check 4-direction patterns
            int pattern4Bit = WallPatterns.GetPattern4Bit(wallPos, walkableTiles);
            Sprite wallSprite = GetWallSprite(pattern4Bit, false);

            // If no 4-bit match found, check 8-direction patterns
            if (wallSprite == null)
            {
                int pattern8Bit = WallPatterns.GetPattern8Bit(wallPos, walkableTiles);
                wallSprite = GetWallSprite(pattern8Bit, true);
            }

            // Create wall object if sprite found
            if (wallSprite != null)
            {
                GameObject wall = Instantiate(_wallTilePrefab,
                                           new Vector3(wallPos.x, wallPos.y, 0),
                                           Quaternion.identity,
                                           _wallParent);
                wall.GetComponent<SpriteRenderer>().sprite = wallSprite;
            }
        }
    }

    private Sprite GetWallSprite(int pattern, bool is8Direction)
    {
        if (is8Direction)
        {
            if (WallPatterns.wallInnerCornerDownLeft.Contains(pattern))
                return GetRandomSprite(wallInnerCornerDownLeft);
            if (WallPatterns.wallInnerCornerDownRight.Contains(pattern))
                return GetRandomSprite(wallInnerCornerDownRight);
            if (WallPatterns.wallDiagonalCornerDownLeft.Contains(pattern))
                return GetRandomSprite(wallDiagonalCornerDownLeft);
            if (WallPatterns.wallDiagonalCornerDownRight.Contains(pattern))
                return GetRandomSprite(wallDiagonalCornerDownRight);
            if (WallPatterns.wallDiagonalCornerUpLeft.Contains(pattern))
                return GetRandomSprite(wallDiagonalCornerUpLeft);
            if (WallPatterns.wallDiagonalCornerUpRight.Contains(pattern))
                return GetRandomSprite(wallDiagonalCornerUpRight);
            if (WallPatterns.wallFullEightDirections.Contains(pattern))
                return GetRandomSprite(wallFull);
            if (WallPatterns.wallBottmEightDirections.Contains(pattern))
                return GetRandomSprite(wallBottm);
        }
        else
        {
            if (WallPatterns.wallTop.Contains(pattern))
                return GetRandomSprite(wallTop);
            if (WallPatterns.wallSideLeft.Contains(pattern))
                return GetRandomSprite(wallSideLeft);
            if (WallPatterns.wallSideRight.Contains(pattern))
                return GetRandomSprite(wallSideRight);
            if (WallPatterns.wallBottm.Contains(pattern))
                return GetRandomSprite(wallBottm);
            if (WallPatterns.wallFull.Contains(pattern))
                return GetRandomSprite(wallFull);
        }
        return null; // Default fallback
    }

    private Sprite GetRandomSprite(List<Sprite> sprites)
    {
        return sprites[Random.Range(0, sprites.Count)];
    }
    public Room GetStartRoom()
    {
        return rooms.FirstOrDefault(r => r.IsStartRoom);
    }

    void OnDrawGizmos()
    {
        if (!_showDebugGizmos || rooms == null) return;

        // Draw all regular rooms
        foreach (var room in rooms)
        {
            if (room.IsStartRoom)
            {
                Gizmos.color = new Color(1, 0.4f, 0.6f); // Pink
            }
            else if (room.IsEndRoom)
            {
                Gizmos.color = Color.blue;
            }
            else if (mainRooms.Contains(room))
            {
                Gizmos.color = Color.yellow;
            }
            else if (newlyAddedNonMainRooms.Contains(room))
            {
                Gizmos.color = Color.cyan;
            }
            else
            {
                Gizmos.color = Color.gray;
            }

            Gizmos.DrawWireCube(room.position, new Vector3(room.width, room.height, 0));
        }

        // Rest of your existing Gizmo code for corridors and tiles...
        foreach (var corridor in corridors)
        {
            Gizmos.color = Color.red;
            Vector2 direction = (corridor.end - corridor.start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * corridor.width / 2;

            Vector3 p1 = corridor.start + perpendicular;
            Vector3 p2 = corridor.start - perpendicular;
            Vector3 p3 = corridor.end + perpendicular;
            Vector3 p4 = corridor.end - perpendicular;

            Gizmos.DrawLine(p1, p3);
            Gizmos.DrawLine(p2, p4);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p3, p4);
        }
        //if (bossRooms != null && bossRooms.Count > 0)
        //{
        //    foreach (var bossRoom in bossRooms)
        //    {
        //        // Draw room boundaries we're checking
        //        Gizmos.color = Color.yellow;
        //        Vector2 center = bossRoom.position;
        //        float width = bossRoom.width;
        //        float height = bossRoom.height;

        //        int left = Mathf.FloorToInt(center.x - width / 2);
        //        int right = Mathf.CeilToInt(center.x + width / 2);
        //        int bottom = Mathf.FloorToInt(center.y - height / 2);
        //        int top = Mathf.CeilToInt(center.y + height / 2);

        //        // Draw boundary lines
        //        Gizmos.DrawLine(new Vector3(left, bottom, 0), new Vector3(right, bottom, 0));
        //        Gizmos.DrawLine(new Vector3(left, top, 0), new Vector3(right, top, 0));
        //        Gizmos.DrawLine(new Vector3(left, bottom, 0), new Vector3(left, top, 0));
        //        Gizmos.DrawLine(new Vector3(right, bottom, 0), new Vector3(right, top, 0));

        //        // Draw all tiles being checked
        //        for (int x = left; x <= right; x++)
        //        {
        //            Vector2Int bottomPos = new Vector2Int(x, bottom);
        //            Vector2Int topPos = new Vector2Int(x, top);

        //            Gizmos.color = walkableTiles.Contains(bottomPos) ? Color.green : Color.red;
        //            Gizmos.DrawCube(new Vector3(x, bottom, 0), Vector3.one * 0.8f);

        //            Gizmos.color = walkableTiles.Contains(topPos) ? Color.green : Color.red;
        //            Gizmos.DrawCube(new Vector3(x, top, 0), Vector3.one * 0.8f);
        //        }

        //        for (int y = bottom + 1; y < top; y++)
        //        {
        //            Vector2Int leftPos = new Vector2Int(left, y);
        //            Vector2Int rightPos = new Vector2Int(right, y);

        //            Gizmos.color = walkableTiles.Contains(leftPos) ? Color.green : Color.red;
        //            Gizmos.DrawCube(new Vector3(left, y, 0), Vector3.one * 0.8f);

        //            Gizmos.color = walkableTiles.Contains(rightPos) ? Color.green : Color.red;
        //            Gizmos.DrawCube(new Vector3(right, y, 0), Vector3.one * 0.8f);
        //        }
        //    }
        //}
    }
}
