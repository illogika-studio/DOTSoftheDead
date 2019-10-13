using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class DestroyAfterTimeSystem : JobComponentSystem
{
    public EndSimulationEntityCommandBufferSystem endSimulationSystem;

    //[BurstCompile]
    struct DestroyAfterTimeSystemJob : IJobForEachWithEntity<DestroyAfterTime>
    {
        public float DeltaTime;
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;

        public void Execute(Entity entity, int index, ref DestroyAfterTime destroyAfterTime)
        {
            destroyAfterTime.Time -= DeltaTime;

            if (destroyAfterTime.Time <= 0f)
            {
                EntityCommandBuffer.DestroyEntity(index, entity);
            }
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        endSimulationSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        DestroyAfterTimeSystemJob destroyAfterTimeSystemJob = new DestroyAfterTimeSystemJob();
        destroyAfterTimeSystemJob.DeltaTime = UnityEngine.Time.deltaTime;
        destroyAfterTimeSystemJob.EntityCommandBuffer = endSimulationSystem.CreateCommandBuffer().ToConcurrent();
        inputDependencies = destroyAfterTimeSystemJob.Schedule(this, inputDependencies);
        endSimulationSystem.AddJobHandleForProducer(inputDependencies);

        return inputDependencies;
    }
}