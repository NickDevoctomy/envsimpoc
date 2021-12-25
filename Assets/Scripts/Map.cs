using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Map : MonoBehaviour
{
    [Range(2,200)] public int Width = 100;
    [Range(2, 200)] public int Height = 100;
    public int Seed;

    public GameObject LandTerrainPrefab;
    public GameObject RockTerrainPrefab;
    public GameObject GrassCoveringPrefab;
    public Material WaterMaterial;

    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    private GameObject _terrain;
    private GameObject _coverings;
    private GameObject _water;
    private TileType[,] _terrainTiles;
    private List<List<Point>> _zones;
    private List<Point> _allZonedPoints;

    void Start()
    {
        Generate();
    }

    void Awake()
    {
        Generate();
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = UnityEngine.Color.red;
        //foreach (var curPoint in _test)
        //{
        //    var location = new Vector3(curPoint.X, 2, curPoint.Y);
        //    Gizmos.DrawSphere(location, 0.1f);
        //}
    }

    public void Generate()
    {
        CleanUp();
        CreateTiles();
        CreateTileCoverings();
        CreateWater();
        InitialiseZones();
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
        var terrainLayer = _perlinNoiseMapGenerator.Generate(
            Seed,
            Width,
            Height);

        var terrainIndex = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var height = terrainLayer[terrainIndex];
                var tileType = GetTileTypeFromHeight(height);
                if (tileType == TileType.Rock)
                {
                    CreateTile(TileType.Land, new Vector2(x, y));
                }
                var tile = CreateTile(tileType, new Vector2(x, y));
                _terrainTiles[x, y] = tileType;
                terrainIndex += 1;
            }
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
                if (height > 0.2f && height < 0.75f)
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
        var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
        water.name = "Water";
        water.transform.parent = _water.transform;
        water.transform.localScale = new Vector3(Width, 1, Height);
        water.transform.position = new Vector3(-0.5f + (Width / 2), GetYOffsetFromTileType(TileType.Water), -0.5f + (Height / 2));
        var render = water.GetComponent<Renderer>();
        render.material = WaterMaterial;
    }

    private GameObject CreateTile(
        TileType tileType,
        Vector2 location)
    {
        if(tileType == TileType.Water)
        {
            return null;
        }

        var tile = GameObject.Instantiate(GetPrefabFromTileType(tileType));
        tile.name = $"{location.x}-{location.y}_{tileType}";
        tile.transform.parent = _terrain.transform;
        tile.transform.position = new Vector3(location.x, GetYOffsetFromTileType(tileType), location.y);
        return tile;
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

    private float GetYOffsetFromTileType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Water:
                {
                    return 0f;
                }

            case TileType.Land:
                {
                    return 1f;
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

    private void InitialiseZones()
    {
        _zones = new List<List<Point>>();
        _allZonedPoints = new List<Point>();
        var curPoint = GetNextUnzonedPoint();
        while (curPoint != null)
        {
            var curPointType = _terrainTiles[curPoint.GetValueOrDefault().X, curPoint.GetValueOrDefault().Y];
            var curZone = GetTileTypeZoneFromPoint(
                curPoint.GetValueOrDefault(),
                curPointType == TileType.Water || curPointType == TileType.Land ?   
                    new List<TileType> { TileType.Water, TileType.Land } :
                    new List<TileType> { TileType.Rock },
                _terrainTiles);
            _zones.Add(curZone);
            _allZonedPoints.AddRange(curZone);
            curPoint = GetNextUnzonedPoint();
        }
    }

    private Point? GetNextUnzonedPoint()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var curPoint = new Point(x, y);
                if(!_allZonedPoints.Contains(curPoint))
                {
                    return curPoint;
                }
            }
        }

        return null;
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
        var checkedPoints = new List<Point>();
        var pointsToCheck = eligableNeigbours.ToList();
        while(pointsToCheck.Count > 0)
        {
            var points = pointsToCheck.ToArray();
            foreach(var curPoint in points)
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

        var touchingSameTypePairs = touching.Where(x => x.Value != null && types.Contains(x.Value.GetValueOrDefault())).ToList();
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

        points.Add(location, tileType);
    }
}
