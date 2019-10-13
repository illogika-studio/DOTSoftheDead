using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MoveToTarget : IComponentData
{
    [NonSerialized]
    public Entity TargetEntity;
    [NonSerialized]
    public float3 TargetPosition;
    [NonSerialized]
    public float3 RandomMin;
    [NonSerialized]
    public float3 RandomMax;
    public float DistanceTargetDetectionSqr;
    public float DistanceReassignTargetSqr;
    public float MoveSpeedMultiplierWhenNoTarget;

}
