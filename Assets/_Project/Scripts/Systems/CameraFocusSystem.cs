using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class CameraSystem : JobComponentSystem
{
    public Entity Camera;

    private EntityQuery FocusablesQuery;

    [BurstCompile]
    struct CameraSystemJob : IJob
    {
        public Entity camEntity;
        public ComponentDataFromEntity<Translation> TranslationFromEntity;
        public ComponentDataFromEntity<Rotation> RotationFromEntity;
        public ComponentDataFromEntity<CameraData> CameraDataFromEntity;

        public float deltaTime;

        [DeallocateOnJobCompletion]
        public NativeArray<Translation> focusables;

        public void Execute()
        {
            CameraData camData = CameraDataFromEntity[camEntity];
            Translation camTranslation = TranslationFromEntity[camEntity];
            Rotation camRotation = RotationFromEntity[camEntity];

            float3 up = math.mul(camRotation.Value, new float3(0, 1, 0));
            float3 right = math.mul(camRotation.Value, new float3(1, 0, 0));

            float minX = default;
            float maxX = default;
            float minY = default;
            float maxY = default;

            for (int i = 0; i < focusables.Length; i++)
            {

                float3 xValue = MathUtilities.ProjectOnNormal(focusables[i].Value, right);
                float3 yValue = MathUtilities.ProjectOnNormal(focusables[i].Value, up);

                if (i == 0)
                {
                    minX = maxX = xValue.x;
                    minY = maxY = yValue.y;
                }
                else
                {
                    minX = math.min(minX, xValue.x);
                    maxX = math.max(maxX, xValue.x);
                    minY = math.min(minY, yValue.y);
                    maxY = math.max(maxY, yValue.y);
                }
            }


            float width = maxX - minX;
            float height = maxY - minY;

            float cameraHeight = math.clamp((height + camData.cameraPadding), camData.cameraMinSize, camData.cameraMaxSize);
            float cameraWidth = cameraHeight * camData.cameraAspectRatio;

            if (cameraWidth < width)
            {
                cameraHeight = math.clamp(((width + camData.cameraPadding) / (camData.cameraAspectRatio)), camData.cameraMinSize, camData.cameraMaxSize);
            }


            float3 min = focusables[0].Value;
            float3 max = focusables[0].Value;

            for (int i = 0; i < focusables.Length; i++)
            {
                var t = focusables[i].Value;
                min.x = math.min(t.x, min.x);
                min.y = math.min(t.y, min.y);
                min.z = math.min(t.z, min.z);
                max.x = math.max(t.x, max.x);
                max.y = math.max(t.y, max.y);
                max.z = math.max(t.z, max.z);
            }

           float3 resultingPosition = (min + max) / 2f;
           resultingPosition.y = 0;


           camTranslation.Value = math.lerp(camTranslation.Value, resultingPosition, camData.cameraSharpness * deltaTime);
           camData.cameraSize = math.lerp(camData.cameraSize, cameraHeight, camData.cameraSharpness * deltaTime);

           TranslationFromEntity[camEntity] = camTranslation;
           RotationFromEntity[camEntity] = camRotation;
           CameraDataFromEntity[camEntity] = camData;
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        FocusablesQuery = GetEntityQuery(ComponentType.ReadOnly<CameraFocus>(), ComponentType.ReadOnly<Translation>());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        if (Camera == default)
            return inputDependencies;

        CameraSystemJob job = new CameraSystemJob();
        job.focusables = FocusablesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        job.camEntity = Camera;
        job.TranslationFromEntity = GetComponentDataFromEntity<Translation>();
        job.RotationFromEntity = GetComponentDataFromEntity<Rotation>();
        job.CameraDataFromEntity = GetComponentDataFromEntity<CameraData>();
        job.deltaTime = Time.deltaTime;

        inputDependencies = job.Schedule(inputDependencies);


        // TODO: make it complete before some barrier system

        return inputDependencies;
    }

}