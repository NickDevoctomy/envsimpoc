using System.Collections.Generic;

internal class EffectLayerManager
{
    private Dictionary<string, object> _layers = new Dictionary<string, object>();
    private Map _map;

    public EffectLayerManager(Map map)
    {
        _map = map;
    }

    public Dictionary<int, T[,]> GetLayer<T>(string name)
    {
        if (!_layers.ContainsKey(name))
        {
            return null;
        }

        return (Dictionary<int, T[,]>)_layers[name];
    }

    public T[,] GetLayer<T>(string name, int zone)
    {
        if(!_layers.ContainsKey(name))
        {
            return null;
        }

        var layer = (Dictionary<int, T[,]>)_layers[name];

        if (!layer.ContainsKey(zone))
        {

            return null;
        }

        return layer[zone];
    }

    public void CreateLayer<T>(string name)
    {
        if(_layers.ContainsKey(name))
        {
            return;
        }

        var newLayer = new Dictionary<int, T[,]>();
        _layers.Add(name, newLayer);

        for(var i = 0; i < _map.Zones.Count; i++)
        {
            var zoneTiles = _map.Zones[i];
            var layer = new T[_map.Width, _map.Height];
            foreach (var curTile in zoneTiles)
            {
                if(_map.Monitors[curTile.X, curTile.Y] != null)
                {
                    layer[curTile.X, curTile.Y] = _map.Monitors[curTile.X, curTile.Y].GetComponent<T>();
                }
            }

            newLayer.Add(i, layer);
        }
    }
}
