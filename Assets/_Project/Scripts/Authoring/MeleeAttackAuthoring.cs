using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//[DisallowMultipleComponent]
//[RequiresEntityConversion]
public class MeleeAttackAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public DamageAuthoring DamagePrefab;
    public MeleeWeapon MeleeAttackData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity attackEntity = conversionSystem.GetPrimaryEntity(DamagePrefab);
        MeleeAttackData.AttackProjectileEntity = attackEntity;
        MeleeAttackData.LastTimeAttack = float.NegativeInfinity;

        dstManager.AddComponentData(entity, MeleeAttackData);
        dstManager.AddComponentData(entity, new AttackInputs());
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(DamagePrefab.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + (Vector3)MeleeAttackData.CenterPoint, math.sqrt(MeleeAttackData.AttackRangeSqr));
    }
}
