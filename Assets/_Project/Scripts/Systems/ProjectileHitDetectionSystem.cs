using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(ProjectileMoveSystem))]
public class ProjectileHitDetectionSystem : JobComponentSystem
{
    private BuildPhysicsWorld _physicsWorld;
    private PreTransformGroupBarrier preTransformBarrier;
    private EntityQuery ProjectilesQuery;

    // Refs:
    // https://github.com/Unity-Technologies/DOTS-Shmup3D-sample/blob/master/Assets/Scripts/FighterManager.cs
    // https://github.com/Unity-Technologies/DOTS-Shmup3D-sample/blob/master/Assets/Scripts/MissileManager.cs
    // https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/UnityPhysicsSamples/Assets/Demos/3.%20Query/Scripts/QueryTester.cs
    // https://docs.unity3d.com/Packages/com.unity.physics@0.0/manual/collision_queries.html
    // https://docs.unity3d.com/Packages/com.unity.physics@0.0/manual/getting_started.html

    //[BurstCompile]
    unsafe struct HitDetectionJob : IJob
    {
        [ReadOnly]
        public PhysicsWorld PhysicsWorld;
        public EntityCommandBuffer entityCommandBuffer;

        [DeallocateOnJobCompletion]
        public NativeArray<RaycastHit> RaycastHits;

        [DeallocateOnJobCompletion]
        public NativeArray<DamageProjectile> Projectiles;
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> ProjectileEntities;
        [DeallocateOnJobCompletion]
        public NativeArray<Translation> ProjectileTranslations;

        public ComponentDataFromEntity<Health> HealthsFromEntity;

        public void Execute()
        {
            for (int i = 0; i < Projectiles.Length; i++)
            {
                CollisionFilter filter = CollisionFilter.Default;
                filter.CollidesWith = 4;

                RaycastInput raycastInput = new RaycastInput
                {
                    Start = Projectiles[i].PreviousPosition,
                    End = ProjectileTranslations[i].Value,
                    Filter = filter
                };
                MaxHitsCollector<RaycastHit> collector = new MaxHitsCollector<RaycastHit>(1.0f, ref RaycastHits);

                if (PhysicsWorld.CastRay(raycastInput, ref collector))
                {
                    if (collector.NumHits > 0)
                    {
                        RaycastHit closestHit = new RaycastHit();
                        closestHit.Fraction = 2f;

                        for (int j = 0; j < collector.NumHits; j++)
                        {
                            if(RaycastHits[j].Fraction < closestHit.Fraction)
                            {
                                closestHit = RaycastHits[j];
                            }
                        }

                        // Apply damage to hit rigidbody/collider
                        Entity hitEntity = PhysicsWorld.Bodies[closestHit.RigidBodyIndex].Entity;
                        if (HealthsFromEntity.Exists(hitEntity))
                        {
                            Health h = HealthsFromEntity[hitEntity];
                            h.Value -= Projectiles[i].Damage;
                            HealthsFromEntity[hitEntity] = h;
                        }

                        // Destroy projectile
                        entityCommandBuffer.DestroyEntity(ProjectileEntities[i]);
                    }
                }
            }
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        preTransformBarrier = World.GetOrCreateSystem<PreTransformGroupBarrier>();
        _physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();

        EntityQueryDesc queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<DamageProjectile>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Rotation>() }
        };
        ProjectilesQuery = GetEntityQuery(queryDesc);
    }

    protected unsafe override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // TODO: launch multiple separate IJobs for projectile collision detection? like one per thread

        HitDetectionJob hitDetectionJob = new HitDetectionJob();
        hitDetectionJob.PhysicsWorld = _physicsWorld.PhysicsWorld;
        hitDetectionJob.entityCommandBuffer = preTransformBarrier.CreateCommandBuffer();
        hitDetectionJob.RaycastHits = new NativeArray<RaycastHit>(64, Allocator.TempJob);
        hitDetectionJob.Projectiles = ProjectilesQuery.ToComponentDataArray<DamageProjectile>(Allocator.TempJob);
        hitDetectionJob.ProjectileEntities = ProjectilesQuery.ToEntityArray(Allocator.TempJob);
        hitDetectionJob.ProjectileTranslations = ProjectilesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        hitDetectionJob.HealthsFromEntity = GetComponentDataFromEntity<Health>();
        inputDependencies = hitDetectionJob.Schedule(inputDependencies);
        preTransformBarrier.AddJobHandleForProducer(inputDependencies);

        inputDependencies.Complete(); 

        return inputDependencies;
    }
}