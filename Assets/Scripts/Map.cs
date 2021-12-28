using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Range(2, 200)] public int Width = 200;
    [Range(2, 200)] public int Height = 200;
    public int Seed;

    public GameObject LandTerrainPrefab;
    public GameObject RockTerrainPrefab;
    public GameObject GrassCoveringPrefab;
    public GameObject WaterPlanePrefab;
    public GameObject MonitorNodePrefab;
    public Material WaterBedMaterial;
    public Material LandMaterial;

    public GameObject[,] Monitors { get; private set; }

    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    private float[] _terrainPoints;
    private GameObject _terrain;
    private GameObject _coverings;
    private GameObject _water;
    private GameObject _nodes;
    private TileType[,] _terrainTiles;
    private List<List<Point>> _islands;

    private Dictionary<Point, GameObject> _allLand;
    private Dictionary<Point, GameObject> _allBedRock;

    void Start()
    {
        Generate();
    }

    void Awake()
    {
        Generate();
    }

    public void Generate()
    {
        CleanUp();
        CreateTiles();
        CreateTileCoverings();
        CreateWater();
        InitialiseIslands();
        MergeAllIslands();
        CreateMonitorNodes();
    }

    private void CleanUp()
    {
        _terrain = AssureEmpty("Tiles");
        _coverings = AssureEmpty("Covering");
        _water = AssureEmpty("Water");
    }

    private GameObject AssureEmpty(string name)
    {
        var existing = transform.Find(name);
        if (existing != null)
        {
            GameObject.DestroyImmediate(existing.gameObject);
            existing = null;
        }

        var empty = new GameObject(name);
        empty.transform.parent = transform;
        return empty;
    }

    private void CreateTiles()
    {
        _terrainTiles = new TileType[Width, Height];
        _terrainPoints = _perlinNoiseMapGenerator.Generate(
            Seed,
            Width,
            Height);
        _allLand = new Dictionary<Point, GameObject>();
        _allBedRock = new Dictionary<Point, GameObject>();

        var terrainIndex = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var height = _terrainPoints[terrainIndex];
                var tileType = GetTileTypeFromHeight(height);
                if (tileType == TileType.Rock)
                {
                    var landTile = CreateTile(TileType.Land, new Vector2(x, y), height);
                    _allLand.Add(new Point(x, y), landTile);
                }
                var tile = CreateTile(tileType, new Vector2(x, y), height);
                switch(tileType)
                {
                    case TileType.Land:
                        {
                            _allLand.Add(new Point(x, y), tile);
                            break;
                        }

                    case TileType.Water:
                        {
                            _allBedRock.Add(new Point(x, y), tile);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }               

                _terrainTiles[x, y] = tileType;
                terrainIndex += 1;
            }
        }
    }

    public void InitialiseIslands()
    {
        var allLandCopy = _allLand.ToDictionary(x => x.Key, x => x.Value);
        _islands = new List<List<Point>>();
        foreach(var curPoint in allLandCopy.Keys.ToArray())
        {
            if(!allLandCopy.ContainsKey(curPoint))
            {
                continue;
            }

            var curIslandPoints = GetTileTypeZoneFromPoint(
                curPoint,
                new List<TileType> { TileType.Land, TileType.Rock },
                _terrainTiles);
            _islands.Add(curIslandPoints);
            curIslandPoints.ForEach(x => allLandCopy.Remove(x));
        }
    }

    public void MergeAllIslands()
    {
        for(int i = 0; i < _islands.Count; i++)
        {
            MergeIsland(i);
        }
    }

    public void MergeIsland(int island)
    {
        if(island >= 0 && island < _islands.Count)
        {
            var islandPoints = _islands[island];
            var allTiles = islandPoints.Select(x => _allLand[x].transform).ToList();
            var allMeshFilters = allTiles.Select(x => x.transform.Find("Cube").GetComponent<MeshFilter>()).ToList();
            CombineInstance[] combine = new CombineInstance[allMeshFilters.Count];
            for(var i = 0; i < allMeshFilters.Count; i++)
            {
                combine[i].mesh = allMeshFilters[i].sharedMesh;
                combine[i].transform = allMeshFilters[i].transform.localToWorldMatrix;
                allMeshFilters[i].gameObject.SetActive(false);
            }
            var merged = new GameObject($"Island{island}");
            merged.transform.parent = _terrain.transform;
            var mergedMeshFilter = merged.AddComponent<MeshFilter>();
            mergedMeshFilter.mesh = new Mesh();
            mergedMeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mergedMeshFilter.sharedMesh.CombineMeshes(combine);
            var mergedMeshRenderer = merged.AddComponent<MeshRenderer>();
            mergedMeshRenderer.material = LandMaterial;
            allTiles.ForEach(x => GameObject.DestroyImmediate(x.gameObject));
            merged.gameObject.SetActive(true);
        }
    }

    private void CreateTileCoverings()
    {
        var grassLayer = _perlinNoiseMapGenerator.Generate(
            Seed + 1,
            Width,
            Height);
        var grassIndex = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var height = grassLayer[grassIndex];
                if (height > 0.2f && height < 0.6f)
                {
                    if (_terrainTiles[x, y] == TileType.Land)
                    {
                        var location = new Vector2(x, y);
                        var grass = GameObject.Instantiate(GrassCoveringPrefab);
                        grass.name = $"{location.x}-{location.y}_GrassCovering";
                        grass.transform.parent = _coverings.transform;
                        grass.transform.position = new Vector3(location.x, 2f, location.y);
                    }
                }

                grassIndex += 1;
            }
        }
    }

    private void CreateWater()
    {
        var waterSurface = Instantiate(WaterPlanePrefab);
        waterSurface.name = "WaterSurface";
        waterSurface.transform.parent = _water.transform;
        waterSurface.transform.localScale = new Vector3((Width / 100) * 4, 1, (Height / 100) * 2);
        waterSurface.transform.position = new Vector3(Width / 2, 0.9f, Height / 2);


        var bedRockMeshFilters = _allBedRock.Values.Select(x => x.transform.Find("Cube").GetComponent<MeshFilter>()).ToList();
        CombineInstance[] combine = new CombineInstance[bedRockMeshFilters.Count];
        for (var i = 0; i < bedRockMeshFilters.Count; i++)
        {
            combine[i].mesh = bedRockMeshFilters[i].sharedMesh;
            combine[i].transform = bedRockMeshFilters[i].transform.localToWorldMatrix;
            bedRockMeshFilters[i].gameObject.SetActive(false);
        }

        var mergedBedRock = new GameObject("BedRock");
        mergedBedRock.transform.parent = _terrain.transform;
        var mergedMeshFilter = mergedBedRock.AddComponent<MeshFilter>();
        mergedMeshFilter.mesh = new Mesh();
        mergedMeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mergedMeshFilter.sharedMesh.CombineMeshes(combine);
        var mergedMeshRenderer = mergedBedRock.AddComponent<MeshRenderer>();
        mergedMeshRenderer.material = WaterBedMaterial;
        mergedBedRock.gameObject.SetActive(true);
        _allBedRock.Values.ToList().ForEach(x => GameObject.DestroyImmediate(x.gameObject));
    }

    private GameObject CreateTile(
        TileType tileType,
        Vector2 location,
        float terrainPoint)
    {
        var tile = GameObject.Instantiate(GetPrefabFromTileType(tileType));
        tile.name = $"{location.x}-{location.y}_{tileType}";
        tile.transform.parent = _terrain.transform;
        tile.transform.position = new Vector3(location.x, GetYOffsetFromTileType(tileType, terrainPoint), location.y);
        if(tileType == TileType.Land)
        {
            tile.transform.localScale = new Vector3(1, 10, 1);
        }

        return tile;
    }

    private void CreateMonitorNodes()
    {
        Monitors = new GameObject[Width, Height];
        _nodes = AssureEmpty("Nodes");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if(_terrainTiles[x,y] != TileType.Rock)
                {
                    var monitorNode = Instantiate(MonitorNodePrefab);
                    monitorNode.transform.parent = _nodes.transform;
                    monitorNode.transform.position = new Vector3(x, 2.0f, y);
                    monitorNode.GetComponent<Monitor>().Location = new Point(x, y);
                    Monitors[x, y] = monitorNode;
                }
            }
        }
    }

    private TileType GetTileTypeFromHeight(float height)
    {
        if(height < 0.5f)
        {
            return TileType.Water;
        }
        else if(height < 0.75f)
        {
            return TileType.Land;
        }
        else
        {
            return TileType.Rock;
        }
    }

    private GameObject GetPrefabFromTileType(TileType tileType)
    {
        switch(tileType)
        {
            case TileType.Water:
                {
                    return LandTerrainPrefab;
                }

            case TileType.Land:
                {
                    return LandTerrainPrefab;
                }

            case TileType.Rock:
                {
                    return RockTerrainPrefab;
                }

            default:
                {
                    throw new System.NotImplementedException($"Tile type of '{tileType}' not implemented.");
                }
        }
    }

    private float GetYOffsetFromTileType(
        TileType tileType,
        float terrainPoint)
    {
        switch (tileType)
        {
            case TileType.Water:
                {
                    return -(terrainPoint * 10) - 1f;
                }

            case TileType.Land:
                {
                    return 1f - 4.5f;
                }

            case TileType.Rock:
                {
                    return 2f;
                }

            default:
                {
                    throw new System.NotImplementedException($"Tile type of '{tileType}' not implemented.");
                }
        }
    }

    private List<Point> GetTileTypeZoneFromPoint(
        Point location,
        List<TileType> tileTypes,
        TileType[,] terrainTiles)
    {
        var eligableNeigbours = GetTouchingOfType(
            location,
            terrainTiles,
            tileTypes,
            false);
        var checkedPoints = new List<Point>{ location };
        var pointsToCheck = eligableNeigbours.ToList();
        while(pointsToCheck.Count > 0)
        {
            var points = pointsToCheck.ToArray();
            foreach (var curPoint in points)
            {
                pointsToCheck.Remove(curPoint);

                if (checkedPoints.Contains(curPoint))
                {
                    continue;
                }

                var nextEligableNeighbours = GetTouchingOfType(
                    curPoint,
                    terrainTiles,
                    tileTypes,
                    false);
                var nextToCheck = nextEligableNeighbours.Where(x => !checkedPoints.Contains(x)).ToList();
                pointsToCheck.AddRange(nextToCheck);
                checkedPoints.Add(curPoint);
            }
        }

        return checkedPoints;
    }

    private List<Point> GetTouchingOfType(
        Point location,
        TileType[,] terrainTiles,
        List<TileType> types,
        bool includeDiagonal)
    {
        var touching = new Dictionary<Point, TileType?>();
        var tileType = terrainTiles[location.X, location.Y];

        AddTileTypeAtLocation(new Point(location.X, location.Y + 1), touching);
        AddTileTypeAtLocation(new Point(location.X + 1, location.Y), touching);
        AddTileTypeAtLocation(new Point(location.X, location.Y - 1), touching);
        AddTileTypeAtLocation(new Point(location.X - 1, location.Y), touching);
        if (includeDiagonal)
        {
            AddTileTypeAtLocation(new Point(location.X + 1, location.Y + 1), touching);
            AddTileTypeAtLocation(new Point(location.X + 1, location.Y - 1), touching);
            AddTileTypeAtLocation(new Point(location.X - 1, location.Y - 1), touching);
            AddTileTypeAtLocation(new Point(location.X - 1, location.Y + 1), touching);
        }

        var touchingSameTypePairs = touching.Where(x => types.Contains(x.Value.GetValueOrDefault())).ToList();
        var touchingSameType = touchingSameTypePairs.Select(x => x.Key).ToList();
        return touchingSameType;
    }

    private void AddTileTypeAtLocation(
        Point location,
        Dictionary<Point, TileType?> points)
    {
        var tileType = default(TileType?);
        if (!(location.X < 0 || location.X > Width - 1 ||
            location.Y < 0 || location.Y > Height - 1))
        {
            tileType = _terrainTiles[location.X, location.Y];
        }

        if(tileType != null)
        {
            points.Add(location, tileType);
        }
    }
}
