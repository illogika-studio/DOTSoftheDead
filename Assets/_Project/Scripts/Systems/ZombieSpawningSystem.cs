using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(PreTransformGroupBarrier))]
public class ZombieSpawningSystem : JobComponentSystem
{
    public float2 SpawnRadiusMinMax;
    public Entity ZombiePrefab;
    public Entity MeleePrefabEntity;
    public Entity[] DropOnDeathEntities;

    public int CurrentZombieCount;

    private float SpawnRate;
    private float SpawnRateIncrease;
    private int MaxZombies;
    private float MaxSpawnRate;
    private bool Spawn;
    private Unity.Mathematics.Random random;
    private NativeArray<float> LastSpawnTime;
    private GameDataSystem gameDataSystem;
    private PreTransformGroupBarrier preTransformBarrier;
    private EntityQuery zombiesQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        EntityQueryDesc queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Character>() },
            None = new ComponentType[] { ComponentType.ReadOnly<PlayerCharacter>() }
        };
        zombiesQuery = EntityManager.CreateEntityQuery(queryDesc);

        gameDataSystem = World.GetOrCreateSystem<GameDataSystem>();
        preTransformBarrier = World.GetOrCreateSystem<PreTransformGroupBarrier>();

        LastSpawnTime = new NativeArray<float>(1, Allocator.Persistent);
        LastSpawnTime[0] = -999f;

        random = new Random();
        random.InitState(10);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        LastSpawnTime.Dispose();
    }

    public void StartSpawning(float initialRate, float rateIncreaseSpeed, int maxZombies, float maxSpawnRate)
    {
        Spawn = true;
        SpawnRate = initialRate;
        MaxSpawnRate = maxSpawnRate;
        MaxZombies = maxZombies;
        SpawnRateIncrease = rateIncreaseSpeed;
        LastSpawnTime[0] = UnityEngine.Time.time;
    }

    public void SpawnZombieBatch(int count)
    {
        NativeList<Entity> spawnedZombies = new NativeList<Entity>(Allocator.Persistent);
        for (int i = 0; i < count; i++)
        {
            spawnedZombies.Add(SpawnCharacterZombie(ZombiePrefab, MeleePrefabEntity, DropOnDeathEntities[random.NextInt(0, DropOnDeathEntities.Length) % DropOnDeathEntities.Length]));
        }
        
        TransformUtilities.UpdateTransformSystems(World);

        foreach (var z in spawnedZombies)
        {
            TransformUtilities.MakeEntityLinkedInHierarchy(EntityManager, z);
        }

        spawnedZombies.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        if (!Spawn)
            return inputDependencies;

        NativeArray<Character> allZombies = zombiesQuery.ToComponentDataArray<Character>(Unity.Collections.Allocator.TempJob);
        CurrentZombieCount = allZombies.Length;
        allZombies.Dispose();

        // TODO: jobify this
        //SpawnRate += SpawnRateIncrease * UnityEngine.Time.deltaTime;
        //SpawnRate = math.clamp(SpawnRate, 0f, MaxSpawnRate);

        //float deltaTime = UnityEngine.Time.time - LastSpawnTime[0];
        //int zombiesToSpawn = (int)math.floor(SpawnRate * deltaTime);

        //if (CurrentZombieCount > MaxZombies)
        //{
        //    LastSpawnTime[0] = UnityEngine.Time.time;
        //}
        //else
        //{
        //    for (int i = 0; i < zombiesToSpawn; i++)
        //    {
        //        SpawnCharacterZombie(ZombiePrefab, MeleePrefabEntity, DropOnDeathEntities[random.NextInt(0, DropOnDeathEntities.Length) % DropOnDeathEntities.Length]);
        //    }
        //}

        return inputDependencies;
    }

    public Entity SpawnCharacterZombie(Entity zombiePrefabEntity, Entity startingMeleePrefabEntity, Entity dropOnDeathEntity)
    {
        float randomAngle = random.NextFloat(0f, 360f);
        float randomDistance = random.NextFloat(SpawnRadiusMinMax.x, SpawnRadiusMinMax.y);
        float3 dir = math.mul(quaternion.RotateY(randomAngle), new float3(0, 0, 1));
        float3 spawnPos = dir * randomDistance;
        quaternion spawnRot = quaternion.LookRotationSafe(math.normalizesafe(-spawnPos), new float3(0f, 1f, 0f));

        Entity charInstanceEntity = EntityManager.Instantiate(zombiePrefabEntity);
        EntityManager.SetComponentData(charInstanceEntity, new Translation { Value = spawnPos });
        EntityManager.SetComponentData(charInstanceEntity, new Rotation { Value = spawnRot });
        EntityManager.AddComponentData(charInstanceEntity, new AITag());
        EntityManager.AddComponentData(charInstanceEntity, new OwningAI { AIEntity = charInstanceEntity });

        // ~10% chance of drop
        if (random.NextInt(0, 10) == 1)
        {
            EntityManager.AddComponentData(charInstanceEntity, new DropOnDeath { toDrop = dropOnDeathEntity });
        }

        Character spawnedCharacter = EntityManager.GetComponentData<Character>(charInstanceEntity);

        // Give zombie the starting melee attack
        Entity meleeEntityInstance = EntityManager.Instantiate(startingMeleePrefabEntity);
        TransformUtilities.SetParent(EntityManager, spawnedCharacter.WeaponHoldPointEntity, meleeEntityInstance, false);
        EntityManager.AddComponentData(meleeEntityInstance, new OwningAI() { AIEntity = charInstanceEntity });

        // set active weapon on Character
        Character charData = EntityManager.GetComponentData<Character>(charInstanceEntity);
        charData.ActiveMeleeWeaponEntity = meleeEntityInstance;
        EntityManager.SetComponentData(charInstanceEntity, charData);

        LastSpawnTime[0] = LastSpawnTime[0] + 1f / SpawnRate;

        return charInstanceEntity;
    }
}
