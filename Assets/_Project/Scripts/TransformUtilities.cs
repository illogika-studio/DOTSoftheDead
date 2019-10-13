using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public static class TransformUtilities
{
    public static void UpdateTransformSystems(World world)
    {
        world.GetOrCreateSystem<EndFrameParentSystem>().Update();
        world.GetOrCreateSystem<EndFrameCompositeRotationSystem>().Update();
        world.GetOrCreateSystem<EndFrameCompositeScaleSystem>().Update();
        world.GetOrCreateSystem<EndFrameParentScaleInverseSystem>().Update();
        world.GetOrCreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
        world.GetOrCreateSystem<EndFrameTRSToLocalToParentSystem>().Update();
        world.GetOrCreateSystem<EndFrameLocalToParentSystem>().Update();
        world.GetOrCreateSystem<EndFrameWorldToLocalSystem>().Update();

        world.EntityManager.CompleteAllJobs();
    }

    public static void SetupBasicTransform(EntityManager dstManager, Entity entity)
    {
        if (!dstManager.HasComponent<Translation>(entity))
            dstManager.AddComponentData(entity, new Translation { });

        if (!dstManager.HasComponent<Rotation>(entity))
            dstManager.AddComponentData(entity, new Rotation { });

        if (!dstManager.HasComponent<LocalToWorld>(entity))
            dstManager.AddComponentData(entity, new LocalToWorld { Value = float4x4.identity });
    }

    public static void SetParent(EntityManager dstManager, Entity parent, Entity child, bool autoLinkInHierarchy)
    {
        if (!dstManager.HasComponent<LocalToWorld>(child))
            dstManager.AddComponentData(child, new LocalToWorld { Value = float4x4.identity });

        if (!dstManager.HasComponent<LocalToWorld>(parent))
            dstManager.AddComponentData(parent, new LocalToWorld { Value = float4x4.identity });

        if (!dstManager.HasComponent<LocalToParent>(child))
            dstManager.AddComponentData(child, new LocalToParent());

        if (!dstManager.HasComponent<Translation>(child))
            dstManager.AddComponentData(child, new Translation());

        if (!dstManager.HasComponent<Translation>(parent))
            dstManager.AddComponentData(parent, new Translation());

        if (!dstManager.HasComponent<Rotation>(child))
            dstManager.AddComponentData(child, new Rotation { Value = quaternion.identity });

        if (!dstManager.HasComponent<Rotation>(parent))
            dstManager.AddComponentData(parent, new Rotation { Value = quaternion.identity });

        if (!dstManager.HasComponent<Parent>(child))
            dstManager.AddComponentData(child, new Parent { Value = parent });
        else
            dstManager.SetComponentData(child, new Parent { Value = parent });


        // TODO: The following chunk of code is disgusting and there clearly has to be another way
        if (autoLinkInHierarchy)
        {
            UpdateTransformSystems(dstManager.World);
            MakeEntityLinkedInHierarchy(dstManager, parent);
        }
    }

    public static void MakeEntityLinkedInHierarchy(EntityManager dstManager, Entity entity)
    {
        NativeList<Entity> children = new NativeList<Entity>(Allocator.Temp);
        Entity rootEntity = FindRootEntity(dstManager, entity);
        DynamicBuffer<LinkedEntityGroup> linkedEntitiesBuffer = GetOrCreateLinkedEntitiesBufferOnEntity(dstManager, rootEntity);
        GetAllChildrenOfEntity(dstManager, rootEntity, ref children);
        foreach (var c in children)
        {
            linkedEntitiesBuffer.Add(c);
        }
        children.Dispose();
    }

    public static Entity FindRootEntity(EntityManager dstManager, Entity ofEntity)
    {
        while (ofEntity != Entity.Null && dstManager.HasComponent<Parent>(ofEntity))
        {
            Entity tmpParent = dstManager.GetComponentData<Parent>(ofEntity).Value;
            if (tmpParent != null)
            {
                ofEntity = tmpParent;
            }
        }

        return ofEntity;
    }

    public static void GetAllChildrenOfEntity(EntityManager dstManager, Entity ofEntity, ref NativeList<Entity> childrenList)
    {
        if (dstManager.HasComponent<Child>(ofEntity))
        {
            DynamicBuffer<Child> childBuffer = dstManager.GetBuffer<Child>(ofEntity);
            foreach (var c in childBuffer)
            {
                childrenList.Add(c.Value);
                GetAllChildrenOfEntity(dstManager, c.Value, ref childrenList);
            }
        }
    }

    public static DynamicBuffer<LinkedEntityGroup> GetOrCreateLinkedEntitiesBufferOnEntity(EntityManager dstManager, Entity onEntity)
    {
        DynamicBuffer<LinkedEntityGroup> linkedEntitiesBuffer = default;
        if (!dstManager.HasComponent<LinkedEntityGroup>(onEntity))
        {
            linkedEntitiesBuffer = dstManager.AddBuffer<LinkedEntityGroup>(onEntity);
        }
        else
        {
            linkedEntitiesBuffer = dstManager.GetBuffer<LinkedEntityGroup>(onEntity);
        }

        return linkedEntitiesBuffer;
    }
}