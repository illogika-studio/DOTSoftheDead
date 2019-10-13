using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class DamagePlayerOverTimeSystem : JobComponentSystem
{
    [BurstCompile]
    struct DamagePlayerOverTimeSystemJob : IJobForEach<PlayerCharacter, Health>
    {
        public float deltaTime;

        public void Execute([ReadOnly] ref PlayerCharacter playerCharacter, ref Health health)
        {
            health.Value -= (1 * deltaTime);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new DamagePlayerOverTimeSystemJob();
        job.deltaTime = UnityEngine.Time.deltaTime;
        return job.Schedule(this, inputDependencies);
    }
}