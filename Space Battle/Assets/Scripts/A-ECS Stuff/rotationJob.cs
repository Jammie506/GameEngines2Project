using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

// This system updates all entities in the scene with both a RotationSpeed_ForEach and Rotation component.

// ReSharper disable once InconsistentNaming
public partial class rotationJob : SystemBase
{
    // OnUpdate runs on the main thread.
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        float mult = Random.Range(0, 5);
        float mult2 = Random.Range(0, 5);
        float mult3 = Random.Range(0, 5);

        // Schedule job to rotate around up vector
        Entities
            .WithName("entityRotation")
            .ForEach((ref Rotation rotation, in entityRotation rotationSpeed) =>
            {
                rotation.Value = math.mul(
                    math.normalize(rotation.Value),
                    quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecondUp * deltaTime * mult));
                
                rotation.Value = math.mul(
                    math.normalize(rotation.Value),
                    quaternion.AxisAngle(math.forward(), rotationSpeed.RadiansPerSecondForward * deltaTime * mult2));
                
                rotation.Value = math.mul(
                    math.normalize(rotation.Value),
                    quaternion.AxisAngle(math.right(), rotationSpeed.RadiansPerSecondRight * deltaTime * mult3));
            })
            
            .ScheduleParallel();
    }
}