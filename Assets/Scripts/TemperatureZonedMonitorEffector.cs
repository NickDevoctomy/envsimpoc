using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class TemperatureEffectLayerManagerEffector : IEffectLayerManagerEffector<Monitor>
{
    public string EffectLayer => "temperature";
    public int UpdateFrequency => 200;
    public bool IsReady => !(_lastTick != null && Environment.TickCount - _lastTick.GetValueOrDefault() < UpdateFrequency);

    private long? _lastTick = null;

    public void Update(Map map, Monitor[,] layer)
    {
        //var toUpdate = new List<Monitor>();
        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                if (layer[x, y] == null)
                {
                    continue;
                }

                var curTemp = layer[x, y];
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

                    //if (!toUpdate.Contains(neighbour))
                    //{
                    //    toUpdate.Add(neighbour);
                    //}
                }
                //if(!toUpdate.Contains(curTemp))
                //{
                //    toUpdate.Add(curTemp);
                //}
            }
        }

        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                if (layer[x, y] == null)
                {
                    continue;
                }

                layer[x, y].ApplyNextTemperature();
            }
        }

        //toUpdate.ForEach(x => x.ApplyNextTemperature());

        _lastTick = Environment.TickCount;
    }
}
