using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class DestroyAfterTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Time;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new DestroyAfterTime { Time = Time });
    }
}
