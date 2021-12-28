using System.Collections.Generic;

internal class EffectLayerManager
{
    private Dictionary<string, object> _layers = new Dictionary<string, object>();
    private Map _map;

    public EffectLayerManager(Map map)
    {
        _map = map;
    }

    public T[,] GetLayer<T>(string name)
    {
        if(!_layers.ContainsKey(name))
        {
            return null;
        }

        return (T[,])_layers[name];
    }

    public void CreateLayer<T>(string name)
    {
        if(_layers.ContainsKey(name))
        {
            return;
        }

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
        _layers.Add(name, layer);
    }
}
