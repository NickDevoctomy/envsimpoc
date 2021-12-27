using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public Map? Map { get; private set; }

    private EffectLayerManager _effectLayerManager;
    private TemperatureEffectLayerManagerEffector? _temperatureEffectLayerManagerEffector;

    void Start()
    {
        Map = GetComponent<Map>();
        _effectLayerManager = new EffectLayerManager(Map);
        _effectLayerManager.CreateFloatLayer("temperature", 0.0f);
        _temperatureEffectLayerManagerEffector = new TemperatureEffectLayerManagerEffector();
    }

    void Update()
    {
        if (Map == null || _temperatureEffectLayerManagerEffector == null)
        {
            return;
        }

        var tempLayer = _effectLayerManager.GetLayer("temperature");
        if (_temperatureEffectLayerManagerEffector.IsReady)
        {
            _temperatureEffectLayerManagerEffector.Update(Map, tempLayer);
            ApplyToMonitors(tempLayer);
        }
    }

    public void Test()
    {
        var layer = _effectLayerManager.GetLayer("temperature");
        layer[0][0, 0] = 100f;
    }

    private void ApplyToMonitors(Dictionary<int, float?[,]> layer)
    {
        foreach(var curZone in layer.Keys)
        {
            var zone = layer[curZone];
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    if (Map.Monitors[x, y] != null && zone[x, y] != null)
                    {
                        Map.Monitors[x, y].GetComponent<Monitor>().Temperature = zone[x, y].GetValueOrDefault();
                    }
                }
            }
        }
    }
}