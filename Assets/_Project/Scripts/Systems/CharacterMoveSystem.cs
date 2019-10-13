using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.Assertions;

public class CharacterMoveSystem : JobComponentSystem
{
    public Transform CameraTransform;

    [BurstCompile]
    struct PlayerInputsToCharacterInputsJob : IJobForEach<Character, CharacterInputs, OwningPlayer>
    {
        public float3 CameraPlanarForwardDirection;
        public float3 CameraPlanarRightDirection;
        public quaternion CameraPlanarRotation;
        [ReadOnly]
        public ComponentDataFromEntity<GameplayInputs> GameplayInputsFromEntity;

        public void Execute([ReadOnly] ref Character character, ref CharacterInputs characterInputs, [ReadOnly] ref OwningPlayer owningPlayer)
        {
            GameplayInputs i = GameplayInputsFromEntity[owningPlayer.PlayerEntity];

            characterInputs.MoveVector = (CameraPlanarRightDirection * i.Move.x) + (CameraPlanarForwardDirection * i.Move.y);
            characterInputs.LookDirection = math.mul(CameraPlanarRotation, new float3(i.Look.x, 0f, i.Look.y));
            characterInputs.Attacking = i.Shoot == 1f;
        }
    } 

    [BurstCompile]
    struct CharacterMoveJob : IJobForEach<Character, CharacterInputs, PhysicsVelocity, Rotation>
    {
        public float deltaTime;

        public unsafe void Execute(ref Character character, [ReadOnly] ref CharacterInputs characterInputs, ref PhysicsVelocity velocity, ref Rotation rotation)
        {
            // Velocity
            float3 targetPlanarVel = characterInputs.MoveVector * character.MoveSpeed;
            velocity.Linear = math.lerp(velocity.Linear, targetPlanarVel, 1f - math.exp(-character.MoveSharpness * deltaTime));
            velocity.Linear += character.StoredImpulse / character.DecollisionDamping;
            character.StoredImpulse = default;
            velocity.Linear.y = 0;

            // Rotation
            if (math.lengthsq(characterInputs.LookDirection) > 0f)
            {
                quaternion smoothedRotation = math.slerp(rotation.Value, quaternion.LookRotationSafe(characterInputs.LookDirection, math.up()), 1f - math.exp(-character.OrientSharpness * deltaTime));
                rotation.Value = smoothedRotation;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        if (!CameraTransform)
            return inputDependencies;

        // TODO: convert to use the cameraData entity
        Vector3 cameraPlanarForward = Vector3.ProjectOnPlane(CameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraPlanarRight = Vector3.ProjectOnPlane(CameraTransform.right, Vector3.up).normalized;
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarForward, Vector3.up);

        PlayerInputsToCharacterInputsJob playerInputsToCharacterInputsJob = new PlayerInputsToCharacterInputsJob();
        playerInputsToCharacterInputsJob.CameraPlanarForwardDirection = cameraPlanarForward;
        playerInputsToCharacterInputsJob.CameraPlanarRightDirection = cameraPlanarRight;
        playerInputsToCharacterInputsJob.CameraPlanarRotation = cameraPlanarRotation;
        playerInputsToCharacterInputsJob.GameplayInputsFromEntity = GetComponentDataFromEntity<GameplayInputs>();
        inputDependencies = playerInputsToCharacterInputsJob.Schedule(this, inputDependencies);

        CharacterMoveJob characterMoveSystemJob = new CharacterMoveJob();
        characterMoveSystemJob.deltaTime = Time.deltaTime;
        inputDependencies = characterMoveSystemJob.Schedule(this, inputDependencies);

        return inputDependencies;
    }
}