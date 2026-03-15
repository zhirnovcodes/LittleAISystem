using Unity.Mathematics;
using Unity.Transforms;

public static class LocalTransformExtensions
{
    /// <summary>
    /// Generates a random position around the current position with the given offset
    /// </summary>
    public static float3 GenerateRandomPosition(float3 currentPosition, float offset, ref Random random)
    {
        var randomDirection = random.NextFloat3Direction();
        //randomDirection.y = 0; // Keep on same height
        return currentPosition + randomDirection * offset;// * random.NextFloat(0.5f, offset);
    }
    /// <summary>
    /// Generates a random position around the current position with the given offset
    /// </summary>
    public static float3 GenerateRandomPosition(float3 position, float3 scale, ref Random random)
    {
        var randomPosition = position;
        var minPos = scale / 2 * -1;
        var maxPos = scale / 2;
        var randomScaleRange = random.NextFloat3(minPos, maxPos);
        randomPosition += randomScaleRange;
        return randomPosition;
    }

    public static float3 GenerateRandomEscapePosition(float3 currentPosition, float3 targetPosition, float2 safeDistance, ref Random random)
    {
        var randomDirection = math.normalize(currentPosition - targetPosition);
        var randomDistance = random.NextFloat(safeDistance.x, safeDistance.y);
        var direction = randomDirection * randomDistance;
        //randomDirection.y = 0; // Keep on same height
        return currentPosition + direction;// * random.NextFloat(0.5f, offset);
    }
    public static bool IsTargetPositionReached(this LocalTransform transform, float3 position, float distance = 0.01f)
    {
        return transform.Position.IsTargetPositionReached(position, distance);
    }

    public static bool IsLookingTowards(this LocalTransform transform, float3 targetPosition, float delta = 0.01f)
    {
        var directionToTarget = targetPosition - transform.Position;
        return transform.Rotation.IsLookingTowards(directionToTarget, delta);
    }

    public static bool IsLookingTowards(this LocalTransform transform, LocalTransform target, float delta = 0.01f)
    {
        var directionToTarget = target.Position - transform.Position;
        return transform.Rotation.IsLookingTowards(directionToTarget, delta);
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, float3 position, float distance)
    {
        return transform.Position.IsDistanceGreaterThan(position, distance);
    }

    public static bool IsTargetDistanceReached(this LocalTransform transform, float3 position, float scale, float distance, float delta = 0.01f)
    {
        float reachThreshold = (transform.Scale + scale) / 2f + distance + delta;
        return math.distancesq(transform.Position, position) <= reachThreshold * reachThreshold;
    }

    public static bool IsTargetDistanceReached(this LocalTransform transform, LocalTransform other, float distance, float delta = 0.01f)
    {
        return transform.IsTargetDistanceReached(other.Position, other.Scale, distance, delta);
    }

    public static bool IsArrivedAndLooking(this LocalTransform transform, LocalTransform other, float distance, float delta = 0.01f)
    {
        return transform.IsTargetDistanceReached(other, distance, delta) &&
            transform.IsLookingTowards(other, delta);
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

    public static LocalTransform MovePositionTowards(this LocalTransform transform, float3 targetPosition, float targetScale, float distance, float speed, float deltaTime)
    {
        var directionToTarget = targetPosition - transform.Position;
        var distanceToTarget = math.length(directionToTarget);
        var distanceToTargetAbs = math.max(0, distanceToTarget - (targetScale + transform.Scale) / 2);

        //if (distanceToTargetAbs < distance)
        if (transform.IsTargetDistanceReached(targetPosition, targetScale, distance))
        {
            return transform;
        }

        var normalizedDirection = directionToTarget / distanceToTarget;
        var moveDistance = math.min(speed * deltaTime, distanceToTargetAbs - distance);

        var newPosition = transform.Position + normalizedDirection * moveDistance;

        return new LocalTransform
        {
            Position = newPosition,
            Rotation = transform.Rotation,
            Scale = transform.Scale
        };
    }

    public static LocalTransform MovePositionTowards(this LocalTransform transform, LocalTransform target, float distance, float speed, float deltaTime)
    {
        return transform.MovePositionTowards(target.Position, target.Scale, distance, speed, deltaTime);
    }

    public static LocalTransform MovePositionAwayFrom(this LocalTransform transform, float3 targetPosition, float targetScale, float speed)
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

        var moveDistance = speed;
        var newPosition = transform.Position + directionFromTarget * moveDistance;

        return new LocalTransform
        {
            Position = newPosition,
            Rotation = transform.Rotation,
            Scale = transform.Scale
        };
    }

    public static LocalTransform MovePositionAwayFrom(this LocalTransform transform, LocalTransform target, float speed)
    {
        return transform.MovePositionAwayFrom(target.Position, target.Scale, speed);
    }

    public static LocalTransform RotateTowards(this LocalTransform transform, float3 targetDirection, float speed, float delta = 0.01f)
    {
        var targetRotation = quaternion.LookRotationSafe(targetDirection, math.up());

        float rotationRadians = math.radians(speed);

        // Get the total angle between current and target rotation
        float angleBetween = AngleBetween(transform.Rotation, targetRotation);

        // Avoid division by zero if already facing target
        if (math.abs(angleBetween) < delta)
            return transform;/*new LocalTransform
            {
                Position = transform.Position,
                Rotation = targetRotation,
                Scale = transform.Scale
            };*/

        // t = how much of the remaining angle to cover this frame
        float t = math.min(1.0f, rotationRadians / angleBetween);

        var newRotation = math.slerp(transform.Rotation, targetRotation, t);
        return new LocalTransform
        {
            Position = transform.Position,
            Rotation = newRotation,
            Scale = transform.Scale
        };
    }

    public static float AngleBetween(quaternion a, quaternion b)
    {
        // Dot product of two quaternions gives cos(halfAngle)
        float dot = math.abs(math.dot(a, b));
        dot = math.clamp(dot, 0f, 1f);
        return 2f * math.acos(dot);
    }

    public static LocalTransform RotateTowards(this LocalTransform transform, LocalTransform target, float speed, float delta = 0.01f)
    {
        var directionToTarget = target.Position - transform.Position;
        return transform.RotateTowards(directionToTarget, speed, delta);
    }

    public static float CalculateDistance(this LocalTransform self, float3 position, float scale)
    {
        float3 deltaPos = position - self.Position;
        float rawDistance = math.length(deltaPos);
        float distance = math.max(0, rawDistance - (self.Scale / 2.0f + scale / 2.0f));
        return distance;
    }

    public static float CalculateDistance(this LocalTransform self, LocalTransform other)
    {
        return self.CalculateDistance(other.Position, other.Scale);
    }
}

