using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public struct InputDeviceIdBufferElement : IBufferElementData
{
    public int DeviceId;
}

[Serializable]
public struct GameplayInputs : IComponentData
{
    public float2 Move;
    public float2 Look;
    public float Shoot;
    public float Melee;
    public float Return;
    public float Action;

    // events
    public bool ShootPressed;
    public bool ShootReleased;
    public bool MeleePressed;
    public bool MeleeReleased;
    public bool ReturnPressed;
    public bool ReturnReleased;
    public bool ActionPressed;
    public bool ActionReleased;
}

