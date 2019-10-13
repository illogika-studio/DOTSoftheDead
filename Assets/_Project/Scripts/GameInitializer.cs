using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Collections;

public class GameInitializer : MonoBehaviour
{
    [Header("Misc")]
    public TextMeshProUGUI ZombieCountText;

    [Header("Players")]
    public bool SpawnPlayerForAllDevices = false;
    public bool SpawnOnePlayerPerGamepad = false;
    public bool SpawnPlayerForKeyboard = false;
    public GameObject PlayerCharacterPrefab;
    public Transform[] PlayerSpawnPoints;

    public const string KeyboardName = "Keyboard";
    public const string MouseName = "Mouse";
    public const string VirtualMultitouchDeviceName = "Virtual Multitouch Device";

    [Header("Zombie Spawning")]
    public float ZombieSpawnRate = 1;
    public float ZombieMaxSpawnRate = 1000;
    public float ZombieSpawnRateIncrease = 1;
    public int ZombieMaxCount = 10000;
    public float2 ZombieSpawnRadiusMinMax = 10;

    [Header("Zombie Batches")]
    public int BatchSpawnCount = 100;
    public GameObject ZombiePrefab;
    public Transform SpawnPointZombie;

    [Header("Gun")]
    public GameObject PlayerStartingGunPrefab;

    [Header("Melee")]
    public GameObject MeleePlayerAttackPrefab;
    public GameObject MeleeZombieAttackPrefab;

    [Header("PickUps")]
    // Two transforms to make a rectangle and spawn pickups inside it
    public UnityEngine.BoxCollider PickUpsBounds;
    public float HealthPickUpsNumber;
    public GameObject[] PickupPrefabs;

    private Entity startingGunPrefabEntity;
    private Entity playerStartingMeleePrefabEntity;
    private Entity zombieStartingMeleePrefabEntity;
    private Entity characterPrefabEntity;
    private Entity prefabEntityZombie;
    private List<Entity> prefabEntityPickUps = new List<Entity>();
    private EntityManager entityManager;
    private ZombieSpawningSystem zombieSpawnSystem;
    private EntityQuery playersQuery;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            zombieSpawnSystem.SpawnZombieBatch(BatchSpawnCount);
            UpdateZombieCount();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            NativeArray<Entity> playerEntities = playersQuery.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < playerEntities.Length; i++)
            {
                string linkedEntities = "";
                DynamicBuffer<LinkedEntityGroup> linkedEntitiesBuffer = World.Active.EntityManager.GetBuffer<LinkedEntityGroup>(playerEntities[i]);

                foreach (var item in linkedEntitiesBuffer)
                {
                    linkedEntities += item.Value.Index.ToString() + ",";
                }

                Debug.Log("Player " + playerEntities[i].Index + " has links : " + linkedEntities);
            }
            playerEntities.Dispose();
        }

        UpdateZombieCount();
    }

    void UpdateZombieCount()
    {
        ZombieCountText.text = zombieSpawnSystem.CurrentZombieCount.ToString();
    }

    void Start()
    {
        entityManager = World.Active.EntityManager;
        zombieSpawnSystem = World.Active.GetOrCreateSystem<ZombieSpawningSystem>();

        EntityQueryDesc queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Character>(), ComponentType.ReadOnly<PlayerCharacter>() }
        };
        playersQuery = World.Active.EntityManager.CreateEntityQuery(queryDesc);

        //foreach (var device in InputDevice.all.ToList())
        //{
        //    Debug.Log("NEW DEVICE: " + device.displayName);
        //}

        startingGunPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(PlayerStartingGunPrefab, World.Active);
        playerStartingMeleePrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(MeleePlayerAttackPrefab, World.Active);
        zombieStartingMeleePrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(MeleeZombieAttackPrefab, World.Active);
        characterPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(PlayerCharacterPrefab, World.Active);
        prefabEntityZombie = GameObjectConversionUtility.ConvertGameObjectHierarchy(ZombiePrefab, World.Active);

        foreach(GameObject prefab in PickupPrefabs)
        {
            prefabEntityPickUps.Add(GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active));
        }

        int playersSpawned = 0;

        if (SpawnPlayerForAllDevices)
        {
            List<int> deviceIds = new List<int>();
            foreach (var device in InputDevice.all.ToList())
            {
                deviceIds.Add(device.deviceId);
            }
            Entity newPlayeEntity = World.Active.GetOrCreateSystem<PlayerInputSystem>().CreatePlayer(deviceIds);
            SpawnCharacterForPlayer(entityManager, characterPrefabEntity, startingGunPrefabEntity, playerStartingMeleePrefabEntity, PlayerSpawnPoints[playersSpawned].position, Quaternion.identity, newPlayeEntity);

            playersSpawned++;
        }

        if (SpawnOnePlayerPerGamepad)
        {
            foreach (var device in InputDevice.all.ToList())
            {
                if (PlayerSpawnPoints.Length > playersSpawned)
                {
                    if (device.displayName != KeyboardName &&
                        device.displayName != MouseName &&
                        device.displayName != VirtualMultitouchDeviceName)
                    {
                        Entity newPlayeEntity = World.Active.GetOrCreateSystem<PlayerInputSystem>().CreatePlayer(new List<int>() { device.deviceId });
                        SpawnCharacterForPlayer(entityManager, characterPrefabEntity, startingGunPrefabEntity, playerStartingMeleePrefabEntity, PlayerSpawnPoints[playersSpawned].position, Quaternion.identity, newPlayeEntity);

                        playersSpawned++;
                    }
                }
            }
        }

        if (SpawnPlayerForKeyboard)
        {
            foreach (var device in InputDevice.all.ToList())
            {
                if (PlayerSpawnPoints.Length > playersSpawned)
                {
                    if (device.displayName == KeyboardName)
                    {
                        Entity newPlayerEntity = World.Active.GetOrCreateSystem<PlayerInputSystem>().CreatePlayer(new List<int>() { device.deviceId });
                        SpawnCharacterForPlayer(entityManager, characterPrefabEntity, startingGunPrefabEntity, playerStartingMeleePrefabEntity, PlayerSpawnPoints[playersSpawned].position, Quaternion.identity, newPlayerEntity);

                        playersSpawned++;
                    }
                }
            }
        }

        Bounds bounds = PickUpsBounds.bounds;
        for(int i = 0; i < HealthPickUpsNumber; i++)
        {
            float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
            float z = UnityEngine.Random.Range(bounds.min.z, bounds.max.z);
            Vector3 spawnPointhealthPickUp = new Vector3(x, 2.0f, z);
            SpawnHealthPickUp(entityManager, prefabEntityPickUps[0], spawnPointhealthPickUp);
        }

        zombieSpawnSystem = World.Active.GetOrCreateSystem<ZombieSpawningSystem>();
        zombieSpawnSystem.ZombiePrefab = prefabEntityZombie;
        zombieSpawnSystem.MeleePrefabEntity = zombieStartingMeleePrefabEntity;
        zombieSpawnSystem.DropOnDeathEntities = prefabEntityPickUps.ToArray();
        zombieSpawnSystem.SpawnRadiusMinMax = ZombieSpawnRadiusMinMax;
        zombieSpawnSystem.StartSpawning(ZombieSpawnRate, ZombieSpawnRateIncrease, ZombieMaxCount, ZombieMaxSpawnRate);
    }

    public void SpawnCharacterForPlayer(EntityManager entityManager, Entity characterPrefabEntity, Entity startingGunPrefabEntity, Entity startingMeleePrefabEntity, Vector3 atPoint, Quaternion atRotation, Entity owningEntity)
    {
        // Spawn a character and assign to player
        Entity charInstanceEntity = entityManager.Instantiate(characterPrefabEntity);
        entityManager.SetComponentData(charInstanceEntity, new Translation { Value = atPoint });
        entityManager.SetComponentData(charInstanceEntity, new Rotation { Value = atRotation });
        entityManager.AddComponentData(charInstanceEntity, new OwningPlayer { PlayerEntity = owningEntity });
        entityManager.AddComponentData(charInstanceEntity, new CameraFocus());
        
        Character spawnedCharacter = entityManager.GetComponentData<Character>(charInstanceEntity);

        // Give players the starting range weapon
        Entity gunEntityInstance = entityManager.Instantiate(startingGunPrefabEntity);
        TransformUtilities.SetParent(entityManager, spawnedCharacter.WeaponHoldPointEntity, gunEntityInstance, true);
        entityManager.AddComponentData(gunEntityInstance, new OwningPlayer() { PlayerEntity = owningEntity });

        // Give players the starting melee attack
        Entity meleeEntityInstance = entityManager.Instantiate(startingMeleePrefabEntity);
        TransformUtilities.SetParent(entityManager, spawnedCharacter.WeaponHoldPointEntity, meleeEntityInstance, true);
        entityManager.AddComponentData(meleeEntityInstance, new OwningPlayer() { PlayerEntity = owningEntity });

        // set active weapons on Character
        Character charData = entityManager.GetComponentData<Character>(charInstanceEntity);
        charData.ActiveRangeWeaponEntity = gunEntityInstance;
        charData.ActiveMeleeWeaponEntity = meleeEntityInstance;
        entityManager.SetComponentData(charInstanceEntity, charData);
    }

    public void SpawnHealthPickUp(EntityManager entityManager, Entity healthPickupPrefabEntity, Vector3 atPoint)
    {
        Entity HealPickUpInstance = entityManager.Instantiate(healthPickupPrefabEntity);
        entityManager.SetComponentData(HealPickUpInstance, new Translation { Value = atPoint });
    }
}
