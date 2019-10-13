using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AssignTargetClosestPlayerSystem : JobComponentSystem
{
    public EntityQuery PlayersQuery;

    [BurstCompile]
    struct AssignTargetClosestPlayerSystemJob : IJobForEach<Translation, MoveToTarget>
    {
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<Translation> playerTranslations;
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<Entity> playerEntities;

        public void Execute([ReadOnly] ref Translation translation, ref MoveToTarget target)
        {
            float distToTarget = float.PositiveInfinity;
            if (target.TargetEntity != Entity.Null)
            {
                target.TargetEntity = Entity.Null;
                target.TargetPosition = translation.Value;
            }

            for (int i = 0; i < playerTranslations.Length; i++)
            {
                float distToPlayer = math.distancesq(playerTranslations[i].Value, translation.Value);
                if (distToPlayer < target.DistanceTargetDetectionSqr)
                {
                    if (distToPlayer < distToTarget)
                    {
                        target.TargetEntity = playerEntities[i];
                        target.TargetPosition = playerTranslations[i].Value;
                        distToTarget = distToPlayer;
                    }
                }
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var jobAssign = new AssignTargetClosestPlayerSystemJob();

        jobAssign.playerTranslations = PlayersQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        jobAssign.playerEntities = PlayersQuery.ToEntityArray(Allocator.TempJob);

        inputDependencies = jobAssign.Schedule(this, inputDependencies);

        return inputDependencies;
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PlayersQuery = GetEntityQuery(ComponentType.ReadOnly<CharacterInputs>(), ComponentType.ReadOnly<PlayerCharacter>(), ComponentType.ReadOnly<Translation>());
    }
}