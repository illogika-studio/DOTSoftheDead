using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CameraEntityBridge : MonoBehaviour
{
    public Camera Camera;
    public Entity BridgedEntity;
    public EntityManager EntityManager;
    public float cameraSharpness;
    public float cameraMinSize;
    public float cameraMaxSize;
    public float cameraPadding;
    public Entity actualCameraEntity;

    private GameObject actualCameraGameObject;
    private Transform _transform;

    private void Start()
    {
        actualCameraGameObject = Camera.gameObject;
        _transform = this.transform;
        EntityManager = World.Active.EntityManager;
    }

    void Update()
    {
        _transform.position = EntityManager.GetComponentData<Translation>(BridgedEntity).Value;

        CameraData cData = EntityManager.GetComponentData<CameraData>(BridgedEntity);

        Camera.orthographicSize = cData.cameraSize;
        cData.cameraSharpness = cameraSharpness;
        cData.cameraMinSize = cameraMinSize;
        cData.cameraMaxSize = cameraMaxSize;
        cData.cameraPadding = cameraPadding;
        cData.cameraAspectRatio = Camera.aspect;

        EntityManager.SetComponentData<CameraData>(BridgedEntity, cData);

        EntityManager.SetComponentData(actualCameraEntity, new Translation { Value = actualCameraGameObject.transform.position});
    }
}
