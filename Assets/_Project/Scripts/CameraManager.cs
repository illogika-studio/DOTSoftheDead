using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class CameraManager : MonoBehaviour
{
    public Camera Camera;
    public CameraEntityBridge entityBridge;
    public float cameraSharpness = 5f;
    public float cameraMinOrthoSize = 20f;
    public float cameraMaxOrthoSize = 80f;
    public float cameraPadding = 30f;

    void Start()
    {
        World.Active.GetOrCreateSystem<CharacterMoveSystem>().CameraTransform = Camera.transform;

        EntityManager manager = World.Active.EntityManager;
        Entity cameraEntity = manager.CreateEntity();
        manager.AddComponentData(cameraEntity, new Translation { Value = this.transform.position});
        manager.AddComponentData(cameraEntity, new Rotation { Value = Camera.transform.rotation });
        manager.AddComponentData(cameraEntity, new CameraData { cameraSize = Camera.orthographicSize, cameraSharpness = cameraSharpness, cameraMinSize = cameraMinOrthoSize, cameraMaxSize = cameraMaxOrthoSize, cameraPadding = cameraPadding});
        entityBridge.BridgedEntity = cameraEntity;
        entityBridge.cameraSharpness = cameraSharpness;
        entityBridge.cameraMinSize = cameraMinOrthoSize;
        entityBridge.cameraMaxSize = cameraMaxOrthoSize;
        entityBridge.cameraPadding = cameraPadding;

        Entity actualCamEntity = manager.CreateEntity();
        manager.AddComponentData(actualCamEntity, new Translation());
        entityBridge.actualCameraEntity = actualCamEntity;

        World.Active.GetOrCreateSystem<CameraSystem>().Camera = cameraEntity;
        World.Active.GetOrCreateSystem<BillBoardSystem>().Camera = actualCamEntity;
    }
}
