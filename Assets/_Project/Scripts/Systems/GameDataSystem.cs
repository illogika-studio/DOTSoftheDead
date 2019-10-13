using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class GameDataSystem : ComponentSystem
{
    public GameData GameData;

    protected override void OnCreate()
    {
        base.OnCreate();

        GameData = Resources.Load<GameData>("GameData");

    }

    protected override void OnUpdate()
    {
    }
}