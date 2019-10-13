using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class WeaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public ProjectileAuthoring ProjectilePrefab;
    public GameObject ShootPoint;
    public RangeWeapon WeaponData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity projectileEntity = conversionSystem.GetPrimaryEntity(ProjectilePrefab.gameObject);

        WeaponData.ProjectileEntity = projectileEntity;
        WeaponData.LastTimeShot = -1000f;
        WeaponData.ShootPointEntity = conversionSystem.GetPrimaryEntity(ShootPoint);

        dstManager.AddComponentData(entity, WeaponData);
        dstManager.AddComponentData(entity, new AttackInputs());
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(ProjectilePrefab.gameObject);
    }
}
