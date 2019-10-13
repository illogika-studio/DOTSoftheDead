using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class TransformEntityBridge : MonoBehaviour
{
    public bool Write = false;
    public Entity BridgedEntity;
    public EntityManager EntityManager;

    private Transform _transform;

    private void Start()
    {
        _transform = this.transform;
        EntityManager = World.Active.EntityManager;
    }

    void Update()
    {
        if (Write)
        {
            EntityManager.SetComponentData<Translation>(BridgedEntity, new Translation() { Value = _transform.position });
            EntityManager.SetComponentData<Rotation>(BridgedEntity, new Rotation() { Value = _transform.rotation });
        }
        else
        {
            _transform.position = EntityManager.GetComponentData<Translation>(BridgedEntity).Value;
            _transform.rotation = EntityManager.GetComponentData<Rotation>(BridgedEntity).Value;
        }
    }
}