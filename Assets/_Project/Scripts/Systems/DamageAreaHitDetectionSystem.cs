using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Unity.Physics.Extensions 
{
    public unsafe class DamageAreaHitDetectionSystem : JobComponentSystem
    {
        private BuildPhysicsWorld _physicsWorld;
        private EntityQuery DamageAreaQuery;

        protected PointDistanceInput PointDistanceInput;


        [BurstCompile]
        unsafe struct DamageAreaHitDetectionSystemJob : IJob
        {
            [DeallocateOnJobCompletion] public NativeArray<Entity> DamageEntities;
            [DeallocateOnJobCompletion] public NativeArray<Translation> Translations;
            [DeallocateOnJobCompletion] public NativeArray<DamageArea> DamageAreas;

            [DeallocateOnJobCompletion] public NativeArray<DistanceHit> DistanceHits;

            public ComponentDataFromEntity<DamageArea> DamageAreasFromEntity;
            public ComponentDataFromEntity<Health> HealthsFromEntity;

            public PointDistanceInput pointDistanceInput;

            [ReadOnly]
            public PhysicsWorld PhysicsWorld;

            public void Execute()
            {
                for (int i = 0; i < DamageEntities.Length; i++)
                {
                    MaxHitsCollector<DistanceHit> collector = new MaxHitsCollector<DistanceHit>(10.0f, ref DistanceHits);

                    if (!DamageAreas[i].WasUsed)
                    {
                        CollisionFilter filter = CollisionFilter.Default;
                        filter.CollidesWith = DamageAreas[i].CollisionFilter;

                        pointDistanceInput.Position = Translations[i].Value;
                        pointDistanceInput.Filter = filter;
                        

                        PhysicsWorld.CalculateDistance(pointDistanceInput, ref collector);

                        for (int j = 0; j < collector.NumHits; j++)
                        {

                            Entity hitEntity = PhysicsWorld.Bodies[DistanceHits[j].RigidBodyIndex].Entity;
                            
                            if (DistanceHits[j].Fraction <= DamageAreas[i].Radius)
                            {
                                // deal damage
                                if (HealthsFromEntity.Exists(hitEntity))
                                {
                                    Health h = HealthsFromEntity[hitEntity];
                                    h.Value -= DamageAreas[i].Damage;
                                    HealthsFromEntity[hitEntity] = h;
                                }
                            }
                        }
                        
                        Entity damageEntity = DamageEntities[i];
                        if (DamageAreasFromEntity.Exists(damageEntity))
                        {
                            DamageArea d = DamageAreasFromEntity[damageEntity];
                            if (d.SingleUse)
                            {
                                d.WasUsed = true;
                                DamageAreasFromEntity[damageEntity] = d;
                            }
                        }
                    }
                }
            }
        }

        protected unsafe override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new DamageAreaHitDetectionSystemJob();
            job.DamageEntities = DamageAreaQuery.ToEntityArray(Allocator.TempJob);
            job.Translations = DamageAreaQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            job.DamageAreas = DamageAreaQuery.ToComponentDataArray<DamageArea>(Allocator.TempJob);

            job.PhysicsWorld = _physicsWorld.PhysicsWorld;
            job.DamageAreasFromEntity = GetComponentDataFromEntity<DamageArea>();
            job.HealthsFromEntity = GetComponentDataFromEntity<Health>();
            job.DistanceHits = new NativeArray<DistanceHit>(128, Allocator.TempJob);

            PointDistanceInput = new PointDistanceInput
            {
                Position = float3.zero,
                MaxDistance = 50
            };

            job.pointDistanceInput = PointDistanceInput;

            inputDependencies = job.Schedule(inputDependencies);
            inputDependencies.Complete();

            return inputDependencies;
        }

        protected /*unsafe*/ override void OnCreate()
        {
            base.OnCreate();

            _physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();

            EntityQueryDesc queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<DamageArea>() }
            };
            DamageAreaQuery = GetEntityQuery(queryDesc);
        }
    }
}