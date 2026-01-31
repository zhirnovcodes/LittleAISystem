using Unity.Mathematics;
using Unity.Transforms;

public static class LocalTransformExtensions
{
    public static bool IsTargetPositionReached(this LocalTransform transform, float3 position, float distance = 0.01f)
    {
        return transform.Position.IsTargetPositionReached(position, distance);
    }

    public static bool IsRotationTowardsTargetReached(this LocalTransform transform, float3 targetPosition, float delta = 0.01f)
    {
        var directionToTarget = targetPosition - transform.Position;
        return transform.Rotation.IsRotationTowardsTargetReached(directionToTarget, delta);
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, float3 position, float distance)
    {
        return transform.Position.IsDistanceGreaterThan(position, distance);
    }

    public static bool IsTargetReached(this LocalTransform transform, float3 position, float scale, float distance = 0.01f)
    {
        float reachThreshold = (transform.Scale + scale) * 0.5f + distance;
        return math.distancesq(transform.Position, position) <= reachThreshold * reachThreshold;
    }

    public static bool IsTargetReached(this LocalTransform transform, LocalTransform other, float distance = 0.01f)
    {
        return transform.IsTargetReached(other.Position, other.Scale, distance);
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, float3 position, float scale, float distance)
    {
        float distanceThreshold = (transform.Scale + scale) * 0.5f + distance;
        return math.distancesq(transform.Position, position) > distanceThreshold * distanceThreshold;
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, LocalTransform other, float distance)
    {
        return transform.IsDistanceGreaterThan(other.Position, other.Scale, distance);
    }

    public static LocalTransform MovePositionTowards(this LocalTransform transform, float3 targetPosition, float targetScale, float distance, float speed)
    {
        var directionToTarget = targetPosition - transform.Position;
        var distanceToTarget = math.length(directionToTarget);
        
        if (distanceToTarget <= 0.0001f)
        {
            return transform;
        }

        var normalizedDirection = directionToTarget / distanceToTarget;
        var moveDistance = speed * distance;

        // Clamp movement to not overshoot target
        moveDistance = math.min(moveDistance, distanceToTarget);

        var newPosition = transform.Position + normalizedDirection * moveDistance;

        return new LocalTransform
        {
            Position = newPosition,
            Rotation = transform.Rotation,
            Scale = transform.Scale
        };
    }

    public static LocalTransform MovePositionTowards(this LocalTransform transform, LocalTransform target, float distance, float speed)
    {
        return transform.MovePositionTowards(target.Position, target.Scale, distance, speed);
    }

    public static LocalTransform MovePositionAwayFrom(this LocalTransform transform, float3 targetPosition, float targetScale, float distance, float speed)
    {
        var directionFromTarget = transform.Position - targetPosition;
        var distanceFromTarget = math.length(directionFromTarget);

        if (distanceFromTarget <= 0.0001f)
        {
            // If at the same position, move in a default direction
            directionFromTarget = new float3(1, 0, 0);
        }
        else
        {
            directionFromTarget = directionFromTarget / distanceFromTarget;
        }

        var moveDistance = speed * distance;
        var newPosition = transform.Position + directionFromTarget * moveDistance;

        return new LocalTransform
        {
            Position = newPosition,
            Rotation = transform.Rotation,
            Scale = transform.Scale
        };
    }

    public static LocalTransform MovePositionAwayFrom(this LocalTransform transform, LocalTransform target, float distance, float speed)
    {
        return transform.MovePositionAwayFrom(target.Position, target.Scale, distance, speed);
    }

    public static LocalTransform RotateTowards(this LocalTransform transform, float3 targetPosition, float speed, float delta = 0.01f)
    {
        var directionToTarget = targetPosition - transform.Position;

        // Calculate target rotation to look at target position
        var targetRotation = quaternion.LookRotationSafe(directionToTarget, math.up());

        // Convert rotation speed from degrees to radians (speed should be degrees * deltaTime from outside)
        float rotationRadians = math.radians(speed);

        // Slerp towards target rotation
        float t = math.min(1.0f, rotationRadians);
        var newRotation = math.slerp(transform.Rotation, targetRotation, t);

        return new LocalTransform
        {
            Position = transform.Position,
            Rotation = newRotation,
            Scale = transform.Scale
        };
    }

    public static LocalTransform RotateTowards(this LocalTransform transform, LocalTransform target, float speed, float delta = 0.01f)
    {
        return transform.RotateTowards(target.Position, speed, delta);
    }
}

