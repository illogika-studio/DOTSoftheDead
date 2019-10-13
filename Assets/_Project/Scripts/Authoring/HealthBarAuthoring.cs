using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class HealthBarAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject healthObject;
    public HealthBar HealthBar;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity healthEntity = conversionSystem.GetPrimaryEntity(healthObject);
        HealthBar.HealthEntity = healthEntity;
        dstManager.AddComponentData(entity, HealthBar);
    }
}
