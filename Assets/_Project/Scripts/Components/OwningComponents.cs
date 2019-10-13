using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct OwningPlayer : IComponentData
{
    public Entity PlayerEntity;
}

[Serializable]
public struct OwningAI : IComponentData
{
    public Entity AIEntity;
}
