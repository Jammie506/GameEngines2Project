using System;
using Unity.Entities;

// ReSharper disable once InconsistentNaming
[GenerateAuthoringComponent]
public struct entityRotation : IComponentData
{
    public float RadiansPerSecond;
}