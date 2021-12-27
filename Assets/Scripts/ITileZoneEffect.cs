using System.Collections.Generic;

internal interface ITileZoneEffect
{
    void Update(Dictionary<int, float?[,]> zones);

    int UpdateFrequency { get; }

    bool IsReady { get; }
}
