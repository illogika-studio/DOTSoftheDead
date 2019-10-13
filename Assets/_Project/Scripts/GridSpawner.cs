using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.Transforms;

public class GridSpawner : MonoBehaviour
{
    public int spawnResolution = 1;
    public float spawnSpacing = 1;
    public GameObject Prefab;
    public Transform SpawnCenter;

    void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;
        Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, World.Active);

        for (int x = 0; x < spawnResolution; x++)
        {
            for (int z = 0; z < spawnResolution; z++)
            {
                Entity characterInstance = entityManager.Instantiate(prefabEntity);
                entityManager.SetComponentData(characterInstance, new Translation { Value = SpawnCenter.position + (Vector3.right * x * spawnSpacing) + (Vector3.forward * z * spawnSpacing) });
                entityManager.SetComponentData(characterInstance, new Rotation { Value = Quaternion.identity });
            }
        }
    }
}
