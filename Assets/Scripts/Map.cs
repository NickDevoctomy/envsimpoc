using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map Instance => _instance;

    [Range(2, 400)] public int Width = 200;
    [Range(2, 400)] public int Height = 200;
    public int Seed;

    public GameObject LandTerrainPrefab;
    public GameObject RockTerrainPrefab;
    public GameObject GrassCoveringPrefab;
    public GameObject WaterPlanePrefab;
    public GameObject MonitorNodePrefab;
    public Material WaterBedMaterial;
    public Material LandMaterial;

    public GameObject[,] Monitors { get; private set; }
    public List<GameObject> MonitorsList { get; private set; }
    public List<List<GameObject>> MonitorsGrouped { get; private set; }
    public List<GameObject> MonitorGroups { get; private set; }
    public List<List<GameObject>> TilesGrouped { get; private set; }
    public List<GameObject> TileGroups { get; private set; }


    private static Map _instance;
    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    private float[] _terrainPoints;
    private GameObject _tiles;
    private GameObject _tileGroups;
    private GameObject _coverings;
    private GameObject _water;
    private GameObject _nodes;
    private TileType[,] _terrainTiles;
    private List<GameObject>[,] _allTerrain;
    private GameObject[,] _tileCoverings;

    private Dictionary<Point, GameObject> _allLand;
    private Dictionary<Point, GameObject> _allBedRock;

    public Map()
    {
        _instance = this;
    }

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
        GroupTilesAndCoverings();
        CreateWater();
        CreateMonitorNodes();
        GroupMonitors();
    }

    private void CleanUp()
    {
        _tiles = AssureEmpty("Tiles");
        _tileGroups = AssureEmpty("TileGroups");
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
        _allTerrain = new List<GameObject>[Width, Height];

        var terrainIndex = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var height = _terrainPoints[terrainIndex];
                var tileType = GetTileTypeFromHeight(height);
                if (tileType == TileType.Rock)
                {
                    var landTile = CreateTile(TileType.Land, new Point(x, y), height);
                    _allLand.Add(new Point(x, y), landTile);
                }
                var tile = CreateTile(tileType, new Point(x, y), height);
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

    private void GroupTilesAndCoverings()
    {
        TileGroups = new List<GameObject>();
        TilesGrouped = new List<List<GameObject>>();
        int maxX = Width / 20;
        int maxY = Height / 20;
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                var tileGroup = new GameObject($"{x}-{y}_TileGroup");
                var groupedTiles = new List<GameObject>();
                for (int subX = x * 20; subX < (x * 20) + 20; subX++)
                {
                    for (int subY = y * 20; subY < (y * 20) + 20; subY++)
                    {
                        var tiles = _allTerrain[subX, subY];
                        groupedTiles.AddRange(tiles);
                        var covering = _tileCoverings[subX, subY];
                        if (covering != null)
                        {
                            groupedTiles.Add(covering);
                        }
                    }
                }

                TilesGrouped.Add(groupedTiles);
                var centre = (groupedTiles[groupedTiles.Count - 1].transform.position - groupedTiles[0].transform.position) / 2;
                tileGroup.transform.position = groupedTiles[0].transform.position + centre;
                tileGroup.transform.parent = _tileGroups.transform;
                foreach (var monitor in groupedTiles)
                {
                    monitor.transform.parent = tileGroup.transform;
                }
                tileGroup.SetActive(false);
                TileGroups.Add(tileGroup);
            }
        }

        GameObject.DestroyImmediate(_tiles);
    }

    private void CreateTileCoverings()
    {
        _tileCoverings = new GameObject[Width, Height];
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
                        _tileCoverings[x, y] = grass;
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
    }

    private GameObject CreateTile(
        TileType tileType,
        Point location,
        float terrainPoint)
    {
        var tile = GameObject.Instantiate(GetPrefabFromTileType(tileType));
        tile.name = $"{location.X}-{location.Y}_{tileType}";
        tile.transform.parent = _tiles.transform;
        tile.transform.position = new Vector3(location.X, GetYOffsetFromTileType(tileType, terrainPoint), location.Y);
        if(tileType == TileType.Land)
        {
            tile.transform.localScale = new Vector3(1, 10, 1);
        }

        if(_allTerrain[location.X, location.Y] == null)
        {
            _allTerrain[location.X, location.Y] = new List<GameObject>();
        }
        _allTerrain[location.X, location.Y].Add(tile);

        return tile;
    }

    private void CreateMonitorNodes()
    {
        Monitors = new GameObject[Width, Height];
        MonitorsList = new List<GameObject>();
        _nodes = AssureEmpty("Nodes");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if(_terrainTiles[x,y] != TileType.Rock)
                {
                    var monitorNode = Instantiate(MonitorNodePrefab);
                    monitorNode.name = $"{x}-{y}_Monitor";
                    monitorNode.transform.parent = _nodes.transform;
                    monitorNode.transform.position = new Vector3(x, 2.0f, y);
                    monitorNode.GetComponent<Monitor>().Location = new Point(x, y);
                    Monitors[x, y] = monitorNode;
                    MonitorsList.Add(monitorNode);
                }
            }
        }
        Monitor.CacheMaterials();
    }

    private void GroupMonitors()
    {
        MonitorGroups = new List<GameObject>();
        MonitorsGrouped = new List<List<GameObject>>();
        int maxX = Width / 20;
        int maxY = Height / 20;
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                var monitorGroup = new GameObject($"{x}-{y}_MonitorGroup");
                var groupedMonitors = new List<GameObject>();
                for (int subX = x * 20; subX < (x * 20) + 20; subX++)
                {
                    for (int subY = y * 20; subY < (y * 20) + 20; subY++)
                    {
                        if (Monitors[subX, subY] != null)
                        {
                            groupedMonitors.Add(Monitors[subX, subY]);
                        }
                    }
                }

                MonitorsGrouped.Add(groupedMonitors);
                var centre = (groupedMonitors[groupedMonitors.Count - 1].transform.position - groupedMonitors[0].transform.position) / 2;
                monitorGroup.transform.position = groupedMonitors[0].transform.position + centre;
                monitorGroup.transform.parent = _nodes.transform;
                foreach (var monitor in groupedMonitors)
                {
                    monitor.transform.parent = monitorGroup.transform;
                }
                monitorGroup.SetActive(false);
                MonitorGroups.Add(monitorGroup);
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
