using System;
using System.Linq;

public class TemperatureEffectLayerManagerEffector : IEffectLayerManagerEffector<Monitor>
{
    public string EffectLayer => "temperature";
    public int UpdateFrequency => 200;
    public bool IsReady => !(_lastTick != null && Environment.TickCount - _lastTick.GetValueOrDefault() < UpdateFrequency);

    private long? _lastTick = null;

    public void Update(Map map, Monitor[,] layer)
    {
        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                if (layer[x, y] == null)
                {
                    continue;
                }

                var curTemp = layer[x, y];
                curTemp.Neighbours.Values.ToList().ForEach(n =>
                {
                    var neighbourTemp = layer[n.Location.GetValueOrDefault().X, n.Location.GetValueOrDefault().Y];
                    var difference = curTemp.Temperature - neighbourTemp.Temperature;
                    if (difference > 0)
                    {
                        var transfer = difference * 0.25f;
                        neighbourTemp.Temperature += transfer;
                        curTemp.Temperature -= transfer;
                    }
                });
            }
        }

        _lastTick = Environment.TickCount;
    }
}
