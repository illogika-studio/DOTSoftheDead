using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CharacterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject WeaponHoldPoint;
    public GameObject CharacterMesh;
    public Character CharacterData;
    public CharacterAnimator CharacterAnimator;
    public MeleeWeapon MeleeAttack;
    public Health HealthData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        CharacterData.WeaponHoldPointEntity = conversionSystem.GetPrimaryEntity(WeaponHoldPoint);
        CharacterData.ActiveRangeWeaponEntity = Entity.Null;
        CharacterData.ActiveMeleeWeaponEntity = Entity.Null;
        HealthData.Value = HealthData.MaxValue;

        dstManager.AddComponentData(entity, CharacterData);
        dstManager.AddComponentData(entity, MeleeAttack);
        dstManager.AddComponentData(entity, HealthData);
        dstManager.AddComponentData(entity, new CharacterInputs());

        CharacterAnimator.CharacterEntity = entity;
        dstManager.AddComponentData(conversionSystem.GetPrimaryEntity(CharacterMesh), CharacterAnimator);
    }
}
