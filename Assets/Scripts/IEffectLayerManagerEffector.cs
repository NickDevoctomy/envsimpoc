using System.Collections.Generic;

internal interface IEffectLayerManagerEffector
{
    public string EffectLayer { get; }
    public void Update(Map map, Dictionary<int, float?[,]> zones);
}