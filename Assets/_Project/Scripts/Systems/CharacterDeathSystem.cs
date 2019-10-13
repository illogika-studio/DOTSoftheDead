using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class CharacterDeathSystem : JobComponentSystem
{
    private GameDataSystem gameDataSystem;
    private PreTransformGroupBarrier preTransformGroupBarrier;

    //[BurstCompile]
    struct CharacterDeathSystemJob : IJobForEachWithEntity<Character, Health, Translation>
    {
        [ReadOnly]
        public ComponentDataFromEntity<DropOnDeath> dropOnDeath;

        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        public void Execute( Entity entity, int index, [ReadOnly] ref Character character, [ReadOnly] ref Health health, [ReadOnly] ref Translation pos)
        {
            if(health.Value <= 0f)
            {
                if (dropOnDeath.Exists(entity))
                {
                    DropOnDeath dod = dropOnDeath[entity];
                    Entity inst = entityCommandBuffer.Instantiate(index, dod.toDrop);
                    entityCommandBuffer.SetComponent(index, inst, new Translation { Value = pos.Value });
                }
                entityCommandBuffer.DestroyEntity(index, entity);
            }
        }
    } 

    protected override void OnCreate()
    {
        base.OnCreate();

        gameDataSystem = World.GetOrCreateSystem<GameDataSystem>();
        preTransformGroupBarrier = World.GetOrCreateSystem<PreTransformGroupBarrier>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        CharacterDeathSystemJob characterDeathSystemJob = new CharacterDeathSystemJob();
        characterDeathSystemJob.entityCommandBuffer = preTransformGroupBarrier.CreateCommandBuffer().ToConcurrent();
        characterDeathSystemJob.dropOnDeath = GetComponentDataFromEntity<DropOnDeath>(true);
        inputDependencies = characterDeathSystemJob.Schedule(this, inputDependencies);
        preTransformGroupBarrier.AddJobHandleForProducer(inputDependencies);

        return inputDependencies;
    }
}