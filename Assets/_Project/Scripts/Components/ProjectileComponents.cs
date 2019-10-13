using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

[Serializable]
public struct DamageProjectile : IComponentData
{
    public float Speed;
    [NonSerialized]
    public float3 PreviousPosition;

    public float Damage;
    public float Radius;
}

[Serializable]
public struct DamageArea : IComponentData
{
    public float Damage;
    public float Radius;
    public uint CollisionFilter;

    public bool SingleUse;
    [NonSerialized]
    public bool WasUsed;
}
