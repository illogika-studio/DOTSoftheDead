using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public class AttackSystem : JobComponentSystem
{
    private PreTransformGroupBarrier preTransformBarrier;

    [BurstCompile]
    struct PlayerInputsToRangeWeaponInputsJob : IJobForEach<RangeWeapon, AttackInputs, OwningPlayer>
    {
        [ReadOnly]
        public ComponentDataFromEntity<GameplayInputs> GameplayInputsFromEntity;

        public void Execute([ReadOnly] ref RangeWeapon weapon, ref AttackInputs attackInputs, [ReadOnly] ref OwningPlayer owningPlayer)
        {
            GameplayInputs i = GameplayInputsFromEntity[owningPlayer.PlayerEntity];

            attackInputs.AttackDown = i.ShootPressed;
            attackInputs.AttackUp = i.ShootReleased;
            attackInputs.AttackHeld = i.Shoot > 0.5f;
        }
    }

    //[BurstCompile]
    struct PlayerInputsToMeleeWeaponInputsJob : IJobForEach<MeleeWeapon, AttackInputs, OwningPlayer>
    {
        [ReadOnly]
        public ComponentDataFromEntity<GameplayInputs> GameplayInputsFromEntity;

        public void Execute([ReadOnly] ref MeleeWeapon weapon, ref AttackInputs attackInputs, [ReadOnly] ref OwningPlayer owningPlayer)
        {
            GameplayInputs i = GameplayInputsFromEntity[owningPlayer.PlayerEntity];

            attackInputs.AttackHeld = i.Melee > 0.5f;
        }
    }

    // TODO: getting errors after a while when bursted (because of instantiate?)
    //[BurstCompile]
    struct WeaponShootJob : IJobForEachWithEntity<RangeWeapon, AttackInputs>
    {
        [ReadOnly]
        public float Time;

        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;

        public void Execute(Entity entity, int index, ref RangeWeapon weapon, [ReadOnly] ref AttackInputs attackInputs)
        {
            if (attackInputs.AttackHeld)
            {
                if(Time >= weapon.LastTimeShot + (1f / weapon.FiringRate))
                {
                    LocalToWorld localToWorld = LocalToWorldFromEntity[weapon.ShootPointEntity];
                    // store gun forward direction
                    quaternion dir = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up);
                    // convert angle to radians
                    float spreadAngle = math.radians(30);
                    // change angle over time
                    spreadAngle = spreadAngle / 2 + math.abs(math.sin(Time * 2)) * spreadAngle / 2;
                    // store angle slice per bullet
                    quaternion diffraction = quaternion.EulerXYZ(0, spreadAngle / (math.ceil(weapon.BulletAmountPerShot) + 1), 0);
                    // prepare offset 50% spreadAngle
                    dir = math.mul(dir, quaternion.EulerXYZ(0, -spreadAngle / 2, 0));

                    for (int i = 0; i < math.ceil(weapon.BulletAmountPerShot); i++)
                    {
                        dir = math.mul(dir, diffraction);

                        Entity spawnedProjectile = entityCommandBuffer.Instantiate(index, weapon.ProjectileEntity);
                        entityCommandBuffer.SetComponent(index, spawnedProjectile, new Translation { Value = localToWorld.Position } );
                        entityCommandBuffer.SetComponent(index, spawnedProjectile, new Rotation { Value = dir });
                    }

                    weapon.LastTimeShot = Time;
                }
            }
        }
    }

    //[BurstCompile]
    struct MeleeAttackJob : IJobForEachWithEntity<MeleeWeapon, AttackInputs>
    {
        [ReadOnly] public float Time;

        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;
        public void Execute(Entity entity, int index, 
            ref MeleeWeapon meleeWeapon,
            [ReadOnly] ref AttackInputs attackInputs)
        {
            if (attackInputs.AttackHeld)
            {
                if (Time >= meleeWeapon.LastTimeAttack + meleeWeapon.AttackCooldown)
                {
                    LocalToWorld localToWorld = LocalToWorldFromEntity[entity];

                    // spawn the sprite
                    Entity spawnedSlashEntity = entityCommandBuffer.Instantiate(index, meleeWeapon.AttackProjectileEntity);
                    entityCommandBuffer.SetComponent(index, spawnedSlashEntity, new Translation { Value = localToWorld.Position });
                    entityCommandBuffer.SetComponent(index, spawnedSlashEntity, new Rotation { Value = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up) });

                    // reset timer
                    meleeWeapon.LastTimeAttack = Time;
                }
            }
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        preTransformBarrier = World.GetOrCreateSystem<PreTransformGroupBarrier>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // range weapon inputs
        PlayerInputsToRangeWeaponInputsJob playerInputsToWeaponInputsJob = new PlayerInputsToRangeWeaponInputsJob();
        playerInputsToWeaponInputsJob.GameplayInputsFromEntity = GetComponentDataFromEntity<GameplayInputs>();
        inputDependencies = playerInputsToWeaponInputsJob.Schedule(this, inputDependencies);

        // melee weapon inputs
        PlayerInputsToMeleeWeaponInputsJob playerInputsToMeleeWeaponInputsJob = new PlayerInputsToMeleeWeaponInputsJob();
        playerInputsToMeleeWeaponInputsJob.GameplayInputsFromEntity = GetComponentDataFromEntity<GameplayInputs>();
        inputDependencies = playerInputsToMeleeWeaponInputsJob.Schedule(this, inputDependencies);

        // range weapon shoot
        WeaponShootJob weaponShootJob = new WeaponShootJob();
        weaponShootJob.Time = UnityEngine.Time.time;
        weaponShootJob.entityCommandBuffer = preTransformBarrier.CreateCommandBuffer().ToConcurrent();
        weaponShootJob.LocalToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
        inputDependencies = weaponShootJob.Schedule(this, inputDependencies);

        // melee weapon shoot
        MeleeAttackJob meleeAttackJob = new MeleeAttackJob();
        meleeAttackJob.Time = UnityEngine.Time.time;
        meleeAttackJob.entityCommandBuffer = preTransformBarrier.CreateCommandBuffer().ToConcurrent();
        meleeAttackJob.LocalToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
        inputDependencies = meleeAttackJob.Schedule(this, inputDependencies);

        preTransformBarrier.AddJobHandleForProducer(inputDependencies);

        //Debug.DrawLine(TranslationFromEntity[weapon.ShootPointEntity].Value, TranslationFromEntity[weapon.ShootPointEntity].Value + new float3(0, 10, 0));

        return inputDependencies;
    }
}