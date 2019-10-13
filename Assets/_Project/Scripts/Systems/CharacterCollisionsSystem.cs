using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class CharacterCollisionsSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorldSystem;
    private StepPhysicsWorld stepPhysicsWorldSystem;

    [BurstCompile]
    public struct CharacterRepulsionJob : ITriggerEventsJob
    {
        [ReadOnly]
        public ComponentDataFromEntity<Translation> TranslationGroup;
        public ComponentDataFromEntity<Character> CharactersGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;
            bool entityAIsCharacter = CharactersGroup.Exists(entityA);
            bool entityBIsCharacter = CharactersGroup.Exists(entityB);

            if(entityAIsCharacter && entityBIsCharacter)
            {
                float3 offsetBToA = TranslationGroup[entityA].Value - TranslationGroup[entityB].Value;

                Character characterA = CharactersGroup[entityA];
                characterA.StoredImpulse += offsetBToA;
                CharactersGroup[entityA] = characterA;

                Character characterB = CharactersGroup[entityB];
                characterB.StoredImpulse -= offsetBToA;
                CharactersGroup[entityB] = characterB;

                // TODO: project velocity as well?

                // TODO: can use CalculateDistance + SimplexSolver.Solve 
            }
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        CharacterRepulsionJob characterRepulsionJob = new CharacterRepulsionJob();
        characterRepulsionJob.TranslationGroup = GetComponentDataFromEntity<Translation>(true);
        characterRepulsionJob.CharactersGroup = GetComponentDataFromEntity<Character>();
        inputDependencies = characterRepulsionJob.Schedule(stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, inputDependencies);

        return inputDependencies;
    }
}