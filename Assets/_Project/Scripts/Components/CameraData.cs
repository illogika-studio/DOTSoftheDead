using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CameraData : IComponentData
{
    public float cameraSharpness ;
    public float cameraSize;
    public float cameraMinSize;
    public float cameraMaxSize;
    public float cameraAspectRatio;
    public float cameraPadding;
}
