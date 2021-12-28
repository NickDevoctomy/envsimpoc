using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Monitor : MonoBehaviour
{
    public enum Neighbour
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public float Temperature = 0f;
    public Point? Location;
    public IReadOnlyDictionary<Neighbour, Monitor> Neighbours => _neighbours;
    public IReadOnlyList<Monitor> MonitorNeighbours => _neighbours.Values.ToList();

    private MeshRenderer _meshRenderer;
    private static object _lock = new object();
    private static Dictionary<int, Material> _materials;
    private int _lastRounded = -1;
    private Dictionary<Neighbour, Monitor> _neighbours = new Dictionary<Neighbour, Monitor>();
    private bool _neighboursSet = false;
    private float _nextTemperature = 0f;
    private int _currentRounded = 0;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        CacheMaterials();
        _nextTemperature = Temperature;
    }

    void Update()
    {
        UpdateMaterial();
    }

    public void IncreaseTemp(float value)
    {
        _nextTemperature += value;
    }

    public void DecreaseTemp(float value)
    {
        _nextTemperature -= value;
    }

    public void ApplyNextTemperature()
    {
        Temperature = _nextTemperature;
        _currentRounded = Mathf.RoundToInt(Temperature);
    }

    public void SetAllNeighbours(Map map, Monitor[,] layer)
    {
        if (_neighboursSet)
        {
            return;
        }

        int x = Location.GetValueOrDefault().X;
        int y = Location.GetValueOrDefault().Y;

        if (x > 0 && layer[x - 1, y] != null)
        {
            SetNeighbour(Monitor.Neighbour.East, layer[x - 1, y]);
        }

        if (x < (map.Width - 1) && layer[x + 1, y] != null)
        {
            SetNeighbour(Monitor.Neighbour.West, layer[x + 1, y]);
        }

        if (y > 0 && layer[x, y - 1] != null)
        {
            SetNeighbour(Monitor.Neighbour.South, layer[x, y - 1]);
        }

        if (y < (map.Height - 1) && layer[x, y + 1] != null)
        {
            SetNeighbour(Monitor.Neighbour.North, layer[x, y + 1]);
        }

        _neighboursSet = true;
    }

    public void SetNeighbour(Neighbour neighbour, Monitor monitor)
    {
        if(_neighbours.ContainsKey(neighbour))
        {
            _neighbours.Remove(neighbour);
        }

        _neighbours.Add(neighbour, monitor);
    }

    private void CacheMaterials()
    {
        lock(_lock)
        {
            if(_materials != null)
            {
                return;
            }

            _materials = new Dictionary<int, Material>();
            for (var i = 0; i <= 100; i++)
            {
                Material material = new Material(Shader.Find("Transparent/Diffuse"));
                var redComp = i > 0 ? (float)i / (float)100 : 0f;
                var blueComp = 1f - redComp;
                material.color = new UnityEngine.Color(redComp, 0, blueComp, 1);
                _materials.Add(i, material);
            }
        }
    }

    private void UpdateMaterial()
    {
        if(_currentRounded != _lastRounded)
        {
            if(_currentRounded > 100)
            {
                _currentRounded = 100;
            }

            _meshRenderer.sharedMaterial = _materials[_currentRounded];
            _lastRounded = _currentRounded;
        }
    }
}
