using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class CharacterAnimationSystem : JobComponentSystem
{
    [BurstCompile]
    struct CharacterAnimationSystemJob : IJobForEach<CharacterAnimator, Translation>
    {
        public float deltaTime;
        public float time;

        [ReadOnly]
        public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityFromEntity;
        [ReadOnly]
        public ComponentDataFromEntity<Character> CharacterFromEntity;

        public void Execute(ref CharacterAnimator characterAnimator, ref Translation translation)
        {
            PhysicsVelocity physicsVelocity = PhysicsVelocityFromEntity[characterAnimator.CharacterEntity];
            Character character = CharacterFromEntity[characterAnimator.CharacterEntity];

            float speedRatio = math.clamp(math.length(physicsVelocity.Linear) / character.MoveSpeed, 0, 1);

            characterAnimator.BobMoveValue = new float3(0, math.abs(math.sin(time * characterAnimator.BobFrequency)) * characterAnimator.BobAmount * speedRatio, 0);
            float3 previousPos = new float3(translation.Value.x, 0, translation.Value.z);
            translation.Value = math.lerp(previousPos, previousPos + characterAnimator.BobMoveValue, speedRatio);


            //float vBobValue = ((math.sin(time * anim.BobFrequency * 2f) * 0.5f) + 0.5f) * anim.BobAmount;
            //float rotBobValue = math.sin(time * anim.BobFrequency) * anim.BobAmount;
            //rotBobValue *= math.length(velocity.Linear);
            //rot.Value = math.mul(rot.Value, quaternion.RotateZ(rotBobValue));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new CharacterAnimationSystemJob();
        
        job.deltaTime = UnityEngine.Time.deltaTime;
        job.time = UnityEngine.Time.time;
        job.PhysicsVelocityFromEntity = GetComponentDataFromEntity<PhysicsVelocity>(true);
        job.CharacterFromEntity = GetComponentDataFromEntity<Character>(true);

        return job.Schedule(this, inputDependencies);
    }
}