using System;

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
                var curTemp = layer[x, y];
                if (curTemp == null || !curTemp.IsAwake)
                {
                    continue;
                }

                var neighbours = curTemp.MonitorNeighbours;
                for (var n = 0; n < neighbours.Count; n++)
                {
                    var neighbour = neighbours[n];
                    var difference = curTemp.Temperature - neighbour.Temperature;
                    if (difference > 0)
                    {
                        var transfer = difference * 0.25f;
                        neighbour.IncreaseTemp(transfer);
                        curTemp.DecreaseTemp(transfer);
                    }

                }
            }
        }

        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var curTemp = layer[x, y];
                if (layer[x, y] == null || !curTemp.IsAwake)
                {
                    continue;
                }

                curTemp.ApplyNextTemperature();
            }
        }

        _lastTick = Environment.TickCount;
    }
}
