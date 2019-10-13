using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DeviceInputEvent<T> where T : struct
{
    public int DeviceId;
    public T InputValue;
}

public class PlayerInputSystem : JobComponentSystem, InputActions.IGameplayActions
{
    public InputActions InputActions;

    public NativeList<DeviceInputEvent<float2>> MoveInputs;
    public NativeList<DeviceInputEvent<float2>> LookInputs;
    public NativeList<DeviceInputEvent<float>> ShootInputs;
    public NativeList<DeviceInputEvent<float>> MeleeInputs;
    public NativeList<DeviceInputEvent<float>> ReturnInputs;
    public NativeList<DeviceInputEvent<float>> ActionInputs;

    public EntityQuery PlayersQuery;

    // TODO: why doesn't this work with burst?
    //[BurstCompile]
    struct GameplayInputsJob : IJobForEach_BCC<InputDeviceIdBufferElement, PlayerTag, GameplayInputs>
    {
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<DeviceInputEvent<float2>> MoveInputs;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<DeviceInputEvent<float2>> LookInputs;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<DeviceInputEvent<float>> ShootInputs;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<DeviceInputEvent<float>> MeleeInputs;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<DeviceInputEvent<float>> ReturnInputs;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<DeviceInputEvent<float>> ActionInputs;

        public void Execute([ReadOnly] DynamicBuffer<InputDeviceIdBufferElement> inputDeviceIdBuffer, [ReadOnly] ref PlayerTag player, ref GameplayInputs gameplayInputs)
        {
            foreach (InputDeviceIdBufferElement playerDeviceId in inputDeviceIdBuffer)
            {
                foreach (DeviceInputEvent<float2> e in MoveInputs)
                {
                    if (e.DeviceId == playerDeviceId.DeviceId)
                    {
                        gameplayInputs.Move = e.InputValue;
                    }
                }

                foreach (DeviceInputEvent<float2> e in LookInputs)
                {
                    if (e.DeviceId == playerDeviceId.DeviceId)
                    {
                        gameplayInputs.Look = e.InputValue;
                    }
                }

                foreach (DeviceInputEvent<float> e in ShootInputs)
                {
                    if (e.DeviceId == playerDeviceId.DeviceId)
                    {
                        if (gameplayInputs.Shoot == 0f && e.InputValue == 1f)
                        {
                            gameplayInputs.ShootPressed = true;
                        }
                        else
                        {
                            gameplayInputs.ShootPressed = false;
                        }

                        if (gameplayInputs.Shoot == 1f && e.InputValue == 0f)
                        {
                            gameplayInputs.ShootReleased = true;
                        }
                        else
                        {
                            gameplayInputs.ShootReleased = false;
                        }

                        gameplayInputs.Shoot = e.InputValue;
                    }
                }

                foreach (DeviceInputEvent<float> e in MeleeInputs)
                {
                    if (e.DeviceId == playerDeviceId.DeviceId)
                    {
                        if (gameplayInputs.Melee == 0f && e.InputValue == 1f)
                        {
                            gameplayInputs.MeleePressed = true;
                        }
                        else
                        {
                            gameplayInputs.MeleePressed = false;
                        }

                        if (gameplayInputs.Melee == 1f && e.InputValue == 0f)
                        {
                            gameplayInputs.MeleeReleased = true;
                        }
                        else
                        {
                            gameplayInputs.MeleeReleased = false;
                        }

                        gameplayInputs.Melee = e.InputValue;
                    }
                }

                foreach (DeviceInputEvent<float> e in ReturnInputs)
                {
                    if (e.DeviceId == playerDeviceId.DeviceId)
                    {
                        if (gameplayInputs.Return == 0f && e.InputValue == 1f)
                        {
                            gameplayInputs.ReturnPressed = true;
                        }
                        else
                        {
                            gameplayInputs.ReturnPressed = false;
                        }

                        if (gameplayInputs.Return == 1f && e.InputValue == 0f)
                        {
                            gameplayInputs.ReturnReleased = true;
                        }
                        else
                        {
                            gameplayInputs.ReturnReleased = false;
                        }

                        gameplayInputs.Return = e.InputValue;
                    }
                }

                foreach (DeviceInputEvent<float> e in ActionInputs)
                {
                    if (e.DeviceId == playerDeviceId.DeviceId)
                    {
                        if (gameplayInputs.Action == 0f && e.InputValue == 1f)
                        {
                            gameplayInputs.ActionPressed = true;
                        }
                        else
                        {
                            gameplayInputs.ActionPressed = false;
                        }

                        if (gameplayInputs.Action == 1f && e.InputValue == 0f)
                        {
                            gameplayInputs.ActionReleased = true;
                        }
                        else
                        {
                            gameplayInputs.ActionReleased = false;
                        }

                        gameplayInputs.Action = e.InputValue;
                    }
                }
            }
        }
    }

    public Entity CreatePlayer(List<int> deviceIds)
    {
        Entity playerEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(playerEntity, new PlayerTag());
        EntityManager.AddComponentData(playerEntity, new GameplayInputs());

        DynamicBuffer<InputDeviceIdBufferElement> deviceIdsBuffer = EntityManager.AddBuffer<InputDeviceIdBufferElement>(playerEntity);
        foreach (var deviceId in deviceIds)
        {
            deviceIdsBuffer.Add(new InputDeviceIdBufferElement() { DeviceId = deviceId });
        }
        
        return playerEntity;
    }

    public void DestroyPlayer(Entity playerEntity)
    {
        EntityManager.DestroyEntity(playerEntity);
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        InputActions = new InputActions();
        InputActions.Gameplay.SetCallbacks(this);

        MoveInputs = new NativeList<DeviceInputEvent<float2>>(Allocator.Persistent);
        LookInputs = new NativeList<DeviceInputEvent<float2>>(Allocator.Persistent);
        ShootInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);
        MeleeInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);
        ReturnInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);
        ActionInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);

        PlayersQuery = EntityManager.CreateEntityQuery(typeof(PlayerTag));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (MoveInputs.IsCreated)
        {
            MoveInputs.Dispose();
        }
        if (LookInputs.IsCreated)
        {
            LookInputs.Dispose();
        }
        if (ShootInputs.IsCreated)
        {
            ShootInputs.Dispose();
        }
        if (MeleeInputs.IsCreated)
        {
            MeleeInputs.Dispose();
        }
        if (ReturnInputs.IsCreated)
        {
            ReturnInputs.Dispose();
        }
        if (ActionInputs.IsCreated)
        {
            ActionInputs.Dispose();
        }
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        InputActions.Enable();
    }

    protected override void OnStopRunning()
    {
        base.OnStartRunning();

        InputActions.Disable();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        GameplayInputsJob job = new GameplayInputsJob();
        job.MoveInputs = MoveInputs;
        job.LookInputs = LookInputs;
        job.ShootInputs = ShootInputs;
        job.MeleeInputs = MeleeInputs;
        job.ReturnInputs = ReturnInputs;
        job.ActionInputs = ActionInputs;
        inputDependencies = job.Schedule(this, inputDependencies);

        // TODO: we should Complete() and clear lists at end of frame? Not sure
        inputDependencies.Complete();

        // Clear input lists
        MoveInputs.Clear();
        LookInputs.Clear();
        ShootInputs.Clear();
        MeleeInputs.Clear();
        ReturnInputs.Clear();
        ActionInputs.Clear();

        return inputDependencies;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        int deviceId = context.control.device.deviceId;

        DeviceInputEvent<float2> e = new DeviceInputEvent<float2>();
        e.DeviceId = deviceId;
        e.InputValue = context.ReadValue<Vector2>();

        MoveInputs.Add(e);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        int deviceId = context.control.device.deviceId;

        DeviceInputEvent<float2> e = new DeviceInputEvent<float2>();
        e.DeviceId = deviceId;
        e.InputValue = context.ReadValue<Vector2>();

        LookInputs.Add(e);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        int deviceId = context.control.device.deviceId;

        DeviceInputEvent<float> e = new DeviceInputEvent<float>();
        e.DeviceId = deviceId;
        e.InputValue = context.ReadValue<float>();

        ShootInputs.Add(e);
    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        int deviceId = context.control.device.deviceId;

        DeviceInputEvent<float> e = new DeviceInputEvent<float>();
        e.DeviceId = deviceId;
        e.InputValue = context.ReadValue<float>();

        MeleeInputs.Add(e);
    }

    public void OnReturn(InputAction.CallbackContext context)
    {
        int deviceId = context.control.device.deviceId;

        DeviceInputEvent<float> e = new DeviceInputEvent<float>();
        e.DeviceId = deviceId;
        e.InputValue = context.ReadValue<float>();

        ReturnInputs.Add(e);
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        int deviceId = context.control.device.deviceId;

        DeviceInputEvent<float> e = new DeviceInputEvent<float>();
        e.DeviceId = deviceId;
        e.InputValue = context.ReadValue<float>();

        ActionInputs.Add(e);
    }
}