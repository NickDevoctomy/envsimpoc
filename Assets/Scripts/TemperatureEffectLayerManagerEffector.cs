using System;
using System.Collections.Generic;

public class TemperatureEffectLayerManagerEffector : IEffectLayerManagerEffector
    {
        public string EffectLayer => "temperature";
        public int UpdateFrequency => 200;
        public bool IsReady => !(_lastTick != null && Environment.TickCount - _lastTick.GetValueOrDefault() < UpdateFrequency);

        private long? _lastTick = null;

        public void Update(Map map, Dictionary<int, float?[,]> zones)
        {
            foreach(var curZone in zones.Keys)
            {
                ProcessZone(map, zones, curZone);
            }

            _lastTick = Environment.TickCount;
        }

    private void ProcessZone(Map map, Dictionary<int, float?[,]> zones, int zone)
    {
        var zonePoints = zones[zone];
        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                if (!zonePoints[x, y].HasValue)
                {
                    continue;
                }

                var curTemp = zonePoints[x, y];
                var neighbours = new List<Tuple<int, int>>();
                if (x > 0 && zonePoints[x - 1, y].HasValue)
                {
                    neighbours.Add(new Tuple<int, int>(x - 1, y));
                }

                if (x < (map.Width - 1) && zonePoints[x + 1, y].HasValue)
                {
                    neighbours.Add(new Tuple<int, int>(x + 1, y));
                }

                if (y > 0 && zonePoints[x, y - 1].HasValue)
                {
                    neighbours.Add(new Tuple<int, int>(x, y - 1));
                }

                if (y < (map.Height - 1) && zonePoints[x, y + 1].HasValue)
                {
                    neighbours.Add(new Tuple<int, int>(x, y + 1));
                }

                neighbours.ForEach(n =>
                {
                    var neighbourTemp = zonePoints[n.Item1, n.Item2];
                    var difference = curTemp - neighbourTemp;
                    if (difference > 0)
                    {
                        var transfer = difference * 0.25f;
                        zonePoints[n.Item1, n.Item2] += transfer;
                        zonePoints[x, y] -= transfer;
                    }
                });
            }
        }
    }
}
