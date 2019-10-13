using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct FollowingPickUp : IComponentData
{
    public float maxSpeed;
}

[Serializable]
public struct HealthPickup : IComponentData
{
    public float restoredAmount;
}

[Serializable]
public struct WeaponBonusPickUp : IComponentData
{
    public float additionnalFireRate;
    public float bulletPerShotMultiplier;
}