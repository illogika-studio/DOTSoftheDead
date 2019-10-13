using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Character : IComponentData
{
    public float MoveSpeed;
    public float MoveSharpness;
    public float OrientSharpness;

    public float DecollisionDamping;

    [NonSerialized]
    public float3 StoredImpulse;
    [NonSerialized]
    public Entity WeaponHoldPointEntity;
    [NonSerialized]
    public Entity ActiveRangeWeaponEntity;
    [NonSerialized]
    public Entity ActiveMeleeWeaponEntity;
}

[Serializable]
public struct PlayerCharacter : IComponentData
{
}


[Serializable]
public struct ZombieCharacter : IComponentData
{
}

[Serializable]
public struct CharacterInputs : IComponentData
{
    public float3 MoveVector;
    public float3 LookDirection;
    public bool Attacking;
}

[Serializable]
public struct CharacterAnimator : IComponentData
{
    public float BobFrequency;
    public float BobAmount;

    public float3 BobMoveValue;

    public Entity CharacterEntity;
}

[Serializable]
public struct Health : IComponentData
{
    public float MaxValue;
    [NonSerialized]
    public float Value;
}

[Serializable]
public struct HealthBar : IComponentData
{
    [NonSerialized]
    public Entity HealthEntity;
}