using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : MonoBehaviour
{
    public float Temperature = 0;

    private MeshRenderer _meshRenderer;

    private static object _lock = new object();
    private static Dictionary<int, Material> _materials;
    private int _lastRounded = -1;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        CacheMaterials();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMaterial();
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
                material.color = new Color(redComp, 0, blueComp, 1);
                _materials.Add(i, material);
            }
        }
    }

    private void UpdateMaterial()
    {
        var _currentRounded = Mathf.RoundToInt(Temperature);
        if(_currentRounded != _lastRounded)
        {
            if(_currentRounded > 100)
            {
                _currentRounded = 100;
            }

            var material = _materials[_currentRounded];
            if (_meshRenderer.material != material)
            {
                _meshRenderer.material = material;
            }
            _lastRounded = _currentRounded;
        }
    }
}
