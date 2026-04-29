using Unity.Entities;
using Unity.Mathematics;

public static class MoveComponentExtensions
{
    public static bool IsTargetReached(this in MoveInputComponent input, in MoveOutputComponent output)
    {
        float distance = math.distance(output.Position, output.TargetPosition);
        return distance <= input.MaxDistance;
    }

    public static bool IsLookingTowards(this in MoveInputComponent input, in MoveOutputComponent output)
    {
        float3 forward = math.forward(output.Rotation);
        float3 toTarget = output.TargetPosition - output.Position;
        float3 flatForward = math.normalizesafe(forward - math.dot(forward, input.Up) * input.Up);
        float3 flatToTarget = math.normalizesafe(toTarget - math.dot(toTarget, input.Up) * input.Up);
        float dot = math.clamp(math.dot(flatForward, flatToTarget), -1f, 1f);
        return math.acos(dot) <= input.RotationDelta;
    }

    public static void Reset(this ref MoveInputComponent input)
    {
        input.Speed = 0f;
        input.RotationSpeed = 0f;
        input.Target = Entity.Null;
    }

    public static void Reset(this ref MoveOutputComponent output)
    {
        output.TargetPosition = new float3(float.MaxValue);
        output.Position = new float3(float.MinValue);
        output.Rotation = quaternion.identity;
    }

    public static void Enable(this ref MoveInputComponent input, float speed, float rotationSpeed, float3 up)
    {
        input.Speed = speed;
        input.RotationSpeed = rotationSpeed;
        input.Up = up;
    }

    public static void SetTarget(this ref MoveInputComponent input, Entity target, float maxDistance)
    {
        input.Target = target;
        input.MaxDistance = maxDistance;
    }
}
