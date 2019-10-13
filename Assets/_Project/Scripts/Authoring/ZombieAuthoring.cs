using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ZombieAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public MoveToTarget moveToTargetData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, moveToTargetData);
    }
}
