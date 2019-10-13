using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct RangeWeapon : IComponentData
{
    public float FiringRate;
    public float BulletAmountPerShot;

    [NonSerialized]
    public Entity ProjectileEntity;
    [NonSerialized]
    public Entity ShootPointEntity;
    [NonSerialized]
    public float LastTimeShot;
}

[Serializable]
public struct MeleeWeapon : IComponentData
{
    public float AttackRangeSqr;

    [NonSerialized]
    public Entity AttackProjectileEntity;

    public float3 CenterPoint;

    public float AttackCooldown;

    [NonSerialized]
    public float LastTimeAttack;

}

[Serializable]
public struct AttackInputs : IComponentData
{
    public bool AttackDown;
    public bool AttackUp;
    public bool AttackHeld;
}
