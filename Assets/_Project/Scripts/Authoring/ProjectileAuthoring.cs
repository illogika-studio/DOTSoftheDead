using UnityEngine;
using Unity.Entities;

public class ProjectileAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public DamageProjectile ProjectileData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, ProjectileData);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ProjectileData.Radius);
    }
}
