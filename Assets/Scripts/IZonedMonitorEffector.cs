using System.Collections.Generic;

internal interface IEffectLayerManagerEffector<T>
{
    public string EffectLayer { get; }
    public void Update(Map map, T[,] layer);
}