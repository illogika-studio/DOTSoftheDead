using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public static class MathUtilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ProjectOnPlane(float3 vector, float3 onPlane)
    {
        float3 orthogonalComponent = onPlane * math.dot(vector, onPlane);
        return vector - orthogonalComponent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ProjectOnNormal(float3 vector, float3 onNormal)
    {
        return onNormal * math.dot(vector, onNormal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ClampMagnitude(float3 vector, float magnitude)
    {
        float lengthScale = math.length(vector) / magnitude;
        if (lengthScale > 1f)
        {
            vector = vector * (1f / lengthScale);
        }
        return vector;
    }
}
