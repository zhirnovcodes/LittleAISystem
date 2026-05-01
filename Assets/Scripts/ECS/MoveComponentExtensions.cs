using Unity.Entities;
using Unity.Mathematics;

public static class MoveComponentExtensions
{
    public static bool IsWaiting(this in MoveInputComponent input, in MoveOutputComponent output)
    {
        return (input.IsEnabled && output.IsEnabled) == false;
    }

    public static bool IsTargetReached(this in MoveInputComponent input, in MoveOutputComponent output)
    {
        float threshold = (output.Scale + output.TargetScale) * 0.5f + input.MaxDistance;
        return math.distancesq(output.Position, output.TargetPosition) <= threshold * threshold;
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
        input.IsEnabled = false;
    }

    public static void Reset(this ref MoveOutputComponent output)
    {
        output.IsTargetDisposed = false;
        output.IsEnabled = false;
    }

    public static void Enable(this ref MoveInputComponent input, float speed, float rotationSpeed, float3 up)
    {
        input.Speed = speed;
        input.RotationSpeed = rotationSpeed;
        input.Up = up;
        input.IsEnabled = true;
    }

    public static void SetTarget(this ref MoveInputComponent input, Entity target, float maxDistance)
    {
        input.Target = target;
        input.MaxDistance = maxDistance;
    }

    public static void SetTarget(this ref MoveInputComponent input, float3 targetPosition, float maxDistance)
    {
        input.Target = Entity.Null;
        input.TargetPosition = targetPosition;
        input.MaxDistance = maxDistance;
    }

    public static void Enable(this ref ComponentLookup<MoveInputComponent> lookup, Entity entity, float speed, float rotationSpeed, float3 up)
    {
        var component = lookup[entity];
        component.Enable(speed, rotationSpeed, up);
        lookup[entity] = component;
    }

    public static void SetTarget(this ref ComponentLookup<MoveInputComponent> lookup, Entity entity, Entity target, float maxDistance)
    {
        var component = lookup[entity];
        component.SetTarget(target, maxDistance);
        lookup[entity] = component;
    }

    public static void SetTarget(this ref ComponentLookup<MoveInputComponent> lookup, Entity entity, float3 targetPosition, float maxDistance)
    {
        var component = lookup[entity];
        component.SetTarget(targetPosition, maxDistance);
        lookup[entity] = component;
    }

    public static void Reset(this ref ComponentLookup<MoveInputComponent> lookup, Entity entity)
    {
        var component = lookup[entity];
        component.Reset();
        lookup[entity] = component;
    }

    public static void Reset(this ref ComponentLookup<MoveOutputComponent> lookup, Entity entity)
    {
        var component = lookup[entity];
        component.Reset();
        lookup[entity] = component;
    }
}
