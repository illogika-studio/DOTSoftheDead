using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class HealthBarSystem : JobComponentSystem
{
    [BurstCompile]
    struct HealthBarSystemJob : IJobForEach<HealthBar, NonUniformScale>
    {
        public float DeltaTime;

        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Health> HealthFromEntity;

        public void Execute([ReadOnly] ref HealthBar healthBar, ref NonUniformScale scale)
        {
            if (HealthFromEntity.Exists(healthBar.HealthEntity))
            {
                Health health = HealthFromEntity[healthBar.HealthEntity];

                scale.Value.x = health.Value / health.MaxValue;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        HealthBarSystemJob healthBarSystemJob = new HealthBarSystemJob();
        healthBarSystemJob.DeltaTime = UnityEngine.Time.deltaTime;
        healthBarSystemJob.HealthFromEntity = GetComponentDataFromEntity<Health>();
        inputDependencies = healthBarSystemJob.Schedule(this, inputDependencies);

        return inputDependencies;
    }
}