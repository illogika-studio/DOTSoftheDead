using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics.Systems;
using Unity.Physics;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class PickupSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorldSystem;
    private StepPhysicsWorld stepPhysicsWorldSystem;
    private PreTransformGroupBarrier preTransformBarrier;

    [BurstCompile]
    struct HealthPickupSystemJob : ITriggerEventsJob
    {
        public EntityCommandBuffer entityCommandBuffer;

        public ComponentDataFromEntity<HealthPickup> healthPickupGroup;
        public ComponentDataFromEntity<Health> healthGroup;
        [ReadOnly]
        public ComponentDataFromEntity<PlayerCharacter> playerCharacterGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;
            bool entityAIsHealthPickup = healthPickupGroup.Exists(entityA);
            bool entityBIsPlayerCharacter = playerCharacterGroup.Exists(entityB);
            if(entityAIsHealthPickup && entityBIsPlayerCharacter)
            {
                HealthPickup healthPickUp = healthPickupGroup[entityA];
                Health health = healthGroup[entityB];

                health.Value += healthPickUp.restoredAmount;
                healthGroup[entityB] = health;

                entityCommandBuffer.DestroyEntity(entityA);
            }
            else
            {
                bool entityAIsPlayerCharacter = playerCharacterGroup.Exists(entityA);
                bool entityBIsHealthPickUp = healthPickupGroup.Exists(entityB);
                if(entityAIsPlayerCharacter && entityBIsHealthPickUp)
                {
                    HealthPickup healthPickUp = healthPickupGroup[entityB];
                    Health health = healthGroup[entityA];

                    health.Value = math.clamp(health.Value + healthPickUp.restoredAmount, 0, health.MaxValue);
                    healthGroup[entityA] = health;

                    entityCommandBuffer.DestroyEntity(entityB);
                }
            }
        }
    }

    [BurstCompile]
    struct WeaponBonusPickupSystemJob : ITriggerEventsJob
    {
        public EntityCommandBuffer entityCommandBuffer;

        public ComponentDataFromEntity<WeaponBonusPickUp> FireRatePickupGroup;
        public ComponentDataFromEntity<RangeWeapon> RangeWeaponGroup;
        [ReadOnly]
        public ComponentDataFromEntity<PlayerCharacter> playerCharacterGroup;

        [ReadOnly]
        public ComponentDataFromEntity<Character> CharacterGroup;



        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;
            bool entityAIsHealthPickup = FireRatePickupGroup.Exists(entityA);
            bool entityBIsPlayerCharacter = playerCharacterGroup.Exists(entityB);
            if (entityAIsHealthPickup && entityBIsPlayerCharacter)
            {
                WeaponBonusPickUp FireRatePickUp = FireRatePickupGroup[entityA];
                var characterEntity = CharacterGroup[entityB].ActiveRangeWeaponEntity;
                RangeWeapon weapon = RangeWeaponGroup[characterEntity];

                weapon.FiringRate += FireRatePickUp.additionnalFireRate;
                weapon.BulletAmountPerShot *= FireRatePickUp.bulletPerShotMultiplier;
                RangeWeaponGroup[characterEntity] = weapon;

                entityCommandBuffer.DestroyEntity(entityA);
            }
            else
            {
                bool entityAIsPlayerCharacter = playerCharacterGroup.Exists(entityA);
                bool entityBIsHealthPickUp = FireRatePickupGroup.Exists(entityB);
                if (entityAIsPlayerCharacter && entityBIsHealthPickUp)
                {
                    WeaponBonusPickUp FireRatePickUp = FireRatePickupGroup[entityB];
                    var characterEntity = CharacterGroup[entityA].ActiveRangeWeaponEntity;
                    RangeWeapon weapon = RangeWeaponGroup[characterEntity];

                    weapon.FiringRate += FireRatePickUp.additionnalFireRate;
                    weapon.BulletAmountPerShot *= FireRatePickUp.bulletPerShotMultiplier;
                    RangeWeaponGroup[characterEntity] = weapon;

                    entityCommandBuffer.DestroyEntity(entityB);
                }
            }
        }
    }
    protected override void OnCreate()
    {
        base.OnCreate();

        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        preTransformBarrier = World.GetOrCreateSystem<PreTransformGroupBarrier>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new HealthPickupSystemJob();
        job.entityCommandBuffer = preTransformBarrier.CreateCommandBuffer();
        job.healthPickupGroup = GetComponentDataFromEntity<HealthPickup>();
        job.healthGroup = GetComponentDataFromEntity<Health>();
        job.playerCharacterGroup = GetComponentDataFromEntity<PlayerCharacter>(true);
        inputDependencies = job.Schedule(stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, inputDependencies);
        preTransformBarrier.AddJobHandleForProducer(inputDependencies);

        var job2 = new WeaponBonusPickupSystemJob();
        job2.entityCommandBuffer = preTransformBarrier.CreateCommandBuffer();
        job2.FireRatePickupGroup = GetComponentDataFromEntity<WeaponBonusPickUp>();
        job2.RangeWeaponGroup = GetComponentDataFromEntity<RangeWeapon>();
        job2.playerCharacterGroup = GetComponentDataFromEntity<PlayerCharacter>(true);
        job2.CharacterGroup = GetComponentDataFromEntity<Character>();
        inputDependencies = job2.Schedule(stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, inputDependencies);
        preTransformBarrier.AddJobHandleForProducer(inputDependencies);

        return inputDependencies;
    }
}