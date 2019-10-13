using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class MoveTowardTargetSystem : JobComponentSystem
{
    [BurstCompile]
    struct CharacterMoveTowardTargetSystemJob : IJobForEachWithEntity<Character, MoveToTarget, CharacterInputs>
    {
        public float deltaTime;
        [ReadOnly]
        public ComponentDataFromEntity<Translation> TranslationGroup;

        public void Execute(Entity entity, int index, 
            [ReadOnly] ref Character character, 
            ref MoveToTarget target, 
            ref CharacterInputs characterInputs)
        {
            float3 direction = math.normalizesafe(target.TargetPosition - TranslationGroup[entity].Value);
            float3 movement = direction;

            if (target.TargetEntity == Entity.Null)
            {
                movement = movement * target.MoveSpeedMultiplierWhenNoTarget;
            }
            else if (!TranslationGroup.Exists(target.TargetEntity))
            {
                target.TargetEntity = Entity.Null;
            }

            characterInputs.LookDirection = direction;
            characterInputs.MoveVector = movement;
        }
    }

    [BurstCompile]
    struct PickUpMoveTowardTargetSystemJob : IJobForEachWithEntity<MoveToTarget, Translation, FollowingPickUp>
    {
        public float deltaTime;

        public void Execute(Entity entity, int index, [ReadOnly] ref MoveToTarget target, ref Translation pos, [ReadOnly] ref FollowingPickUp followingPickUp)
        {
            if (target.TargetEntity != Entity.Null)
            {
                float3 direction = math.normalizesafe(target.TargetPosition - pos.Value);
                float ratio = math.distancesq(target.TargetPosition, pos.Value) / target.DistanceTargetDetectionSqr;
                pos.Value = pos.Value + direction * deltaTime * followingPickUp.maxSpeed *  (1 - ratio);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var characterJob = new CharacterMoveTowardTargetSystemJob();
        characterJob.deltaTime = UnityEngine.Time.deltaTime;
        characterJob.TranslationGroup = GetComponentDataFromEntity<Translation>(true);
        inputDependencies = characterJob.Schedule(this, inputDependencies);

        var pickupJob = new PickUpMoveTowardTargetSystemJob();
        pickupJob.deltaTime = UnityEngine.Time.deltaTime;
        inputDependencies = pickupJob.Schedule(this, inputDependencies);

        return inputDependencies;
    }
}