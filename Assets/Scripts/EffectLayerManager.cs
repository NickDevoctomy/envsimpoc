using System.Collections.Generic;

//TODO: Refactor this to remove zones, only need layers

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

        var layer = new T[_map.Width, _map.Height];
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                if (_map.Monitors[x, y] != null)
                {
                    layer[x, y] = _map.Monitors[x, y].GetComponent<T>();
                }
            }
        }
        newLayer.Add(0, layer);
    }
}
