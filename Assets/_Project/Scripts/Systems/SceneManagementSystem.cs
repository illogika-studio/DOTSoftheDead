using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine.SceneManagement;

public class SceneManagementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
    }

    public void ResetGame()
    {
        foreach (var e in EntityManager.GetAllEntities())
        {
            if (EntityManager.Exists(e))
            {
                EntityManager.DestroyEntity(e);
            }
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}