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
}
