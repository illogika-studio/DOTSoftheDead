using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class FireRatePickUpAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public WeaponBonusPickUp fireRatePickUp;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, fireRatePickUp);
    }
}
