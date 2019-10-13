using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class BillBoardSystem : JobComponentSystem
{
    public Entity Camera;

    //[BurstCompile]
    struct BillBoardSystemJob : IJobForEach<Billboard, LocalToWorld, Rotation, Translation, Parent>
    {
        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;

        public Translation CamPosition;

        public void Execute([ReadOnly] ref Billboard billboard, [ReadOnly] ref LocalToWorld ltw, ref Rotation rotation, [ReadOnly] ref Translation translation, [ReadOnly] ref Parent parent)
        {
            //UnityEngine.Debug.Log("CameraPosition from BillBoardSystem : " + CamPosition.Value);
            float3 billboardToCamera = CamPosition.Value - math.mul(ltw.Value, new float4(translation.Value, 1)).xyz;
            quaternion desiredWorlRotation = quaternion.LookRotationSafe(math.normalizesafe(billboardToCamera), new float3(0, 1, 0));
            if (LocalToWorldFromEntity.Exists(parent.Value))
            {
                quaternion currentWorldRotation = new quaternion(LocalToWorldFromEntity[parent.Value].Value);
                rotation.Value = math.mul(math.inverse(currentWorldRotation), desiredWorlRotation);
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new BillBoardSystemJob();
        job.CamPosition = GetComponentDataFromEntity<Translation>()[Camera];
        job.LocalToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
        return job.Schedule(this, inputDependencies);
    }
}