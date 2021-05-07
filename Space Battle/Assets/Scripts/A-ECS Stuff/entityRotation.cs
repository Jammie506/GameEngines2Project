using System;
using Unity.Entities;

// ReSharper disable once InconsistentNaming
[GenerateAuthoringComponent]
public struct entityRotation : IComponentData
{
    public float RadiansPerSecondUp;
    public float RadiansPerSecondForward;
    public float RadiansPerSecondRight;
}