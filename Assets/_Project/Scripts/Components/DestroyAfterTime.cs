using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DestroyAfterTime : IComponentData
{
    public float Time;
}
