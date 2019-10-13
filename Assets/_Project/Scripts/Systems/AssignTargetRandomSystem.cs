using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AssignTargetRandomSystem : JobComponentSystem
{
    [BurstCompile]
    struct AssignTargetSystemJob : IJobForEach<Translation, MoveToTarget, AITag>
    {
        public Random random;

        public void Execute([ReadOnly] ref Translation translation, ref MoveToTarget target, [ReadOnly] ref AITag aiTag)
        {
            if (target.TargetEntity == Entity.Null && 
                (math.distancesq(translation.Value, target.TargetPosition) < target.DistanceReassignTargetSqr)
                || target.TargetPosition.Equals(default))
            {
                float3 newRoamPos = new float3(random.NextFloat3(target.RandomMin, target.RandomMax));
                newRoamPos.y = 0;
                // the 0.8f is to keep the toward the center
                target.TargetPosition = translation.Value * 0.8f + newRoamPos;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        AssignTargetSystemJob assignTargetSystemJob = new AssignTargetSystemJob();
        assignTargetSystemJob.random = new Random((uint)UnityEngine.Random.Range(1, 100000));
        inputDependencies = assignTargetSystemJob.Schedule(this, inputDependencies);

        return inputDependencies;
    }
}