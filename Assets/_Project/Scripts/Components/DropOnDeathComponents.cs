using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DropOnDeath : IComponentData
{
    public Entity toDrop;
}
