using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ProjectileMoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct ProjectileMoveSystemJob : IJobForEach<Translation, Rotation, DamageProjectile>
    {
        public float deltaTime;

        public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation, ref DamageProjectile projectile)
        {
            projectile.PreviousPosition = translation.Value;
            translation.Value += math.forward(rotation.Value) * projectile.Speed * deltaTime;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        ProjectileMoveSystemJob projectileMoveSystemJob = new ProjectileMoveSystemJob();
        projectileMoveSystemJob.deltaTime = UnityEngine.Time.deltaTime;
        inputDependencies = projectileMoveSystemJob.Schedule(this, inputDependencies);

        return inputDependencies;
    }
}