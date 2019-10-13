using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

public class ZombieAutoAttackSystem : JobComponentSystem
{
    [BurstCompile]
    struct ZombieMeleeAttackSystemJob : IJobForEachWithEntity<MoveToTarget, Character, Translation>
    {
        [ReadOnly]
        public ComponentDataFromEntity<MeleeWeapon> MeleeWeaponGroup;
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<AttackInputs> AttackInputsGroup;

        public void Execute(Entity entity, 
            int index,
            [ReadOnly] ref MoveToTarget target, 
            [ReadOnly] ref Character character, 
            [ReadOnly] ref Translation pos)
        {
            Entity meleeWeaponEntity = character.ActiveMeleeWeaponEntity;

            AttackInputs myAttackInput = AttackInputsGroup[meleeWeaponEntity];

            if (target.TargetEntity != Entity.Null
                && meleeWeaponEntity != Entity.Null
                && math.distancesq(target.TargetPosition, pos.Value) < MeleeWeaponGroup[meleeWeaponEntity].AttackRangeSqr)
            {
                // set inputs
                myAttackInput.AttackHeld = true;
            }
            else
            {
                myAttackInput.AttackHeld = false;
            }
            AttackInputsGroup[meleeWeaponEntity] = myAttackInput;
                
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        ZombieMeleeAttackSystemJob zombieAttackJob = new ZombieMeleeAttackSystemJob();
        zombieAttackJob.MeleeWeaponGroup = GetComponentDataFromEntity<MeleeWeapon>(true);
        zombieAttackJob.AttackInputsGroup = GetComponentDataFromEntity<AttackInputs>();
        inputDependencies = zombieAttackJob.Schedule(this, inputDependencies);

        return inputDependencies;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }
}