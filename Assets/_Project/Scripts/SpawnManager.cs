using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public static class SpawnManager
{
    public static Entity SpawnPlayerCharacter(EntityManager dstManager, Entity sourceEntity, Entity owningPlayerEntity)
    {
        Entity characterInstanceEntity = dstManager.Instantiate(sourceEntity);
        dstManager.AddComponentData(characterInstanceEntity, new OwningPlayer { PlayerEntity = owningPlayerEntity });
        dstManager.AddComponentData(characterInstanceEntity, new CameraFocus());

        PlayerCharacter pc = dstManager.GetComponentData<PlayerCharacter>(characterInstanceEntity);

        return characterInstanceEntity;
    }
}