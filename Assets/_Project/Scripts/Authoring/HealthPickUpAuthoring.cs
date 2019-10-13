using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class HealthPickUpAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float restoredAmount;
    public MoveToTarget MoveToTargetData;
    public FollowingPickUp followingPickup;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HealthPickup { restoredAmount = restoredAmount});
        dstManager.AddComponentData(entity, followingPickup);
        dstManager.AddComponentData(entity, MoveToTargetData);
    }
}
