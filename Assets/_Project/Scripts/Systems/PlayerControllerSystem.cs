using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class PlayerControllerSystem : JobComponentSystem
{
    public EntityQuery PlayersWithGameplayInputsQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        EntityQueryDesc queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<GameplayInputs>() }
        };
        PlayersWithGameplayInputsQuery = GetEntityQuery(queryDesc);
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        NativeArray<GameplayInputs> playerGameplayInputs = PlayersWithGameplayInputsQuery.ToComponentDataArray<GameplayInputs>(Allocator.TempJob);

        foreach (var pInputs in playerGameplayInputs)
        {
            //if(pInputs.ReturnPressed)
            if (pInputs.Return >= 1f)
            {
                World.Active.GetOrCreateSystem<SceneManagementSystem>().ResetGame();
                break;
            }
        }

        playerGameplayInputs.Dispose();
        return inputDependencies;
    }
}