using System.Collections.Generic;

internal class EffectLayerManager
{
    private Dictionary<string, Dictionary<int, float?[,]>> _floatLayers = new Dictionary<string, Dictionary<int, float?[,]>>();
    private Map _map;

    public EffectLayerManager(Map map)
    {
        _map = map;
    }

    public Dictionary<int, float?[,]> GetLayer(string name)
    {
        if (!_floatLayers.ContainsKey(name))
        {
            return null;
        }

        return _floatLayers[name];
    }

    public float?[,]? GetLayer(string name, int zone)
    {
        if(!_floatLayers.ContainsKey(name))
        {
            return null;
        }

        if(!_floatLayers[name].ContainsKey(zone))
        {
            return null;
        }

        return _floatLayers[name][zone];
    }

    public void CreateFloatLayer(
        string name,
        float defaultValue)
    {
        if(_floatLayers.ContainsKey(name))
        {
            return;
        }

        _floatLayers.Add(name, new Dictionary<int, float?[,]>());

        for(var i = 0; i < _map.Zones.Count; i++)
        {
            var zoneTiles = _map.Zones[i];
            var layer = new float?[_map.Width, _map.Height];
            foreach (var curTile in zoneTiles)
            {
                layer[curTile.X, curTile.Y] = defaultValue;
            }

            _floatLayers[name].Add(i, layer);
        }
    }
}
