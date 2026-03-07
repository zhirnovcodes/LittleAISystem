using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TransformExtensionsTestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunTransformExtensionsTestComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("=== Starting Transform Extensions Tests ===");

        TestIsTargetReached();
        TestIsLookingTowards();
        TestMovePositionTowards();
        TestRotateTowards();
        TestIntegration();

        Debug.Log("=== All Transform Extensions Tests Complete ===");
    }

    #region IsTargetReached Tests

    private void TestIsTargetReached()
    {
        Debug.Log("\n--- Testing IsTargetReached() ---");

        // Test exact position match
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            bool result = transform.IsTargetDistanceReached(new float3(0, 0, 0), 1f, 0.01f);
            Debug.Log($"Exact position match: {result} (expected: true)");
            AssertTrue(result, "Exact position match");
        }

        // Test within threshold
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // reachThreshold = (1 + 1) * 0.5 + 0.5 = 1.5
            // Distance 1.4 is within threshold
            bool result = transform.IsTargetDistanceReached(new float3(1.4f, 0, 0), 1f, 0.5f);
            Debug.Log($"Within threshold (distance 1.4, threshold 1.5): {result} (expected: true)");
            AssertTrue(result, "Within threshold");
        }

        // Test just outside threshold
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // reachThreshold = (1 + 1) * 0.5 + 0.5 = 1.5
            // Distance 1.6 is outside threshold
            bool result = transform.IsTargetDistanceReached(new float3(1.6f, 0, 0), 1f, 0.5f);
            Debug.Log($"Just outside threshold (distance 1.6, threshold 1.5): {result} (expected: false)");
            AssertFalse(result, "Just outside threshold");
        }

        // Test far away
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            bool result = transform.IsTargetDistanceReached(new float3(100, 0, 0), 1f, 0.01f);
            Debug.Log($"Far away: {result} (expected: false)");
            AssertFalse(result, "Far away");
        }

        // Test zero distance parameter
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 2f
            };
            // reachThreshold = (2 + 2) * 0.5 + 0 = 2
            bool result = transform.IsTargetDistanceReached(new float3(1.9f, 0, 0), 2f, 0f);
            Debug.Log($"Zero distance parameter (distance 1.9, threshold 2): {result} (expected: true)");
            AssertTrue(result, "Zero distance parameter");
        }

        // Test zero scales
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 0f
            };
            // reachThreshold = (0 + 0) * 0.5 + 0.1 = 0.1
            bool result = transform.IsTargetDistanceReached(new float3(0.05f, 0, 0), 0f, 0.1f);
            Debug.Log($"Zero scales (distance 0.05, threshold 0.1): {result} (expected: true)");
            AssertTrue(result, "Zero scales");
        }

        // Test large distance parameter
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // reachThreshold = (1 + 1) * 0.5 + 100 = 101
            bool result = transform.IsTargetDistanceReached(new float3(50, 0, 0), 1f, 100f);
            Debug.Log($"Large distance parameter (distance 50, threshold 101): {result} (expected: true)");
            AssertTrue(result, "Large distance parameter");
        }

        // Test exactly at threshold boundary
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // reachThreshold = (1 + 1) * 0.5 + 1 = 2
            // Distance exactly 2.0
            bool result = transform.IsTargetDistanceReached(new float3(2.0f, 0, 0), 1f, 1f);
            Debug.Log($"Exactly at threshold boundary (distance 2.0, threshold 2.0): {result} (expected: true)");
            AssertTrue(result, "Exactly at threshold boundary");
        }

        // Test slightly inside threshold
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // reachThreshold = (1 + 1) * 0.5 + 1 = 2
            // Distance 1.99
            bool result = transform.IsTargetDistanceReached(new float3(1.99f, 0, 0), 1f, 1f);
            Debug.Log($"Slightly inside threshold (distance 1.99, threshold 2.0): {result} (expected: true)");
            AssertTrue(result, "Slightly inside threshold");
        }

        // Test slightly outside threshold
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // reachThreshold = (1 + 1) * 0.5 + 1 = 2
            // Distance 2.01
            bool result = transform.IsTargetDistanceReached(new float3(2.01f, 0, 0), 1f, 1f);
            Debug.Log($"Slightly outside threshold (distance 2.01, threshold 2.0): {result} (expected: false)");
            AssertFalse(result, "Slightly outside threshold");
        }
    }

    #endregion

    #region IsLookingTowards Tests

    private void TestIsLookingTowards()
    {
        Debug.Log("\n--- Testing IsLookingTowards() ---");

        // Test perfect alignment
        {
            quaternion rotation = quaternion.identity; // Looking forward (0, 0, 1)
            float3 direction = new float3(0, 0, 1);
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Perfect alignment: {result} (expected: true)");
            AssertTrue(result, "Perfect alignment");
        }

        // Test opposite direction
        {
            quaternion rotation = quaternion.identity; // Looking forward (0, 0, 1)
            float3 direction = new float3(0, 0, -1);
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Opposite direction: {result} (expected: false)");
            AssertFalse(result, "Opposite direction");
        }

        // Test perpendicular direction
        {
            quaternion rotation = quaternion.identity; // Looking forward (0, 0, 1)
            float3 direction = new float3(1, 0, 0);
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Perpendicular direction: {result} (expected: false)");
            AssertFalse(result, "Perpendicular direction");
        }

        // Test within delta threshold (small angle)
        {
            quaternion rotation = quaternion.identity; // Looking forward (0, 0, 1)
            // Direction slightly off (5 degrees)
            float angle = math.radians(5f);
            float3 direction = new float3(math.sin(angle), 0, math.cos(angle));
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Within delta threshold (5 degrees): {result} (expected: true)");
            AssertTrue(result, "Within delta threshold");
        }

        // Test zero vector direction
        {
            quaternion rotation = quaternion.identity;
            float3 direction = new float3(0, 0, 0);
            // This will be normalized to (0,0,0) / 0 = NaN, behavior may vary
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Zero vector direction: {result} (behavior may vary)");
        }

        // Test unnormalized direction (very long)
        {
            quaternion rotation = quaternion.identity;
            float3 direction = new float3(0, 0, 1000); // Long vector in Z direction
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Unnormalized long direction: {result} (expected: true after normalization)");
            AssertTrue(result, "Unnormalized long direction");
        }

        // Test unnormalized direction (very short)
        {
            quaternion rotation = quaternion.identity;
            float3 direction = new float3(0, 0, 0.001f); // Short vector in Z direction
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"Unnormalized short direction: {result} (expected: true after normalization)");
            AssertTrue(result, "Unnormalized short direction");
        }

        // Test delta = 0 (perfect alignment required)
        {
            quaternion rotation = quaternion.identity;
            float3 direction = new float3(0, 0, 1);
            bool result = rotation.IsLookingTowards(direction, 0f);
            Debug.Log($"Delta = 0 (perfect alignment): {result} (expected: true)");
            AssertTrue(result, "Delta = 0");
        }

        // Test delta = 2.0 (should always return true)
        {
            quaternion rotation = quaternion.identity;
            float3 direction = new float3(0, 0, -1); // Opposite direction
            bool result = rotation.IsLookingTowards(direction, 2.0f);
            Debug.Log($"Delta = 2.0 (always true): {result} (expected: true)");
            AssertTrue(result, "Delta = 2.0");
        }

        // Test 45-degree angle with small delta
        {
            quaternion rotation = quaternion.identity;
            float angle = math.radians(45f);
            float3 direction = new float3(math.sin(angle), 0, math.cos(angle));
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"45-degree angle with delta 0.1: {result} (expected: false)");
            AssertFalse(result, "45-degree angle with small delta");
        }

        // Test small angle (5 degrees) with various deltas
        {
            quaternion rotation = quaternion.identity;
            float angle = math.radians(5f);
            float3 direction = new float3(math.sin(angle), 0, math.cos(angle));
            
            bool result1 = rotation.IsLookingTowards(direction, 0.001f);
            Debug.Log($"5-degree angle with delta 0.001: {result1} (expected: false)");
            AssertFalse(result1, "5-degree angle with delta 0.001");

            bool result2 = rotation.IsLookingTowards(direction, 0.01f);
            Debug.Log($"5-degree angle with delta 0.01: {result2} (expected: true)");
            AssertTrue(result2, "5-degree angle with delta 0.01");
        }

        // Test 180-degree rotation
        {
            quaternion rotation = quaternion.RotateY(math.radians(180f));
            float3 direction = new float3(0, 0, 1);
            bool result = rotation.IsLookingTowards(direction, 0.1f);
            Debug.Log($"180-degree rotation: {result} (expected: false)");
            AssertFalse(result, "180-degree rotation");
        }
    }

    #endregion

    #region MovePositionTowards Tests

    private void TestMovePositionTowards()
    {
        Debug.Log("\n--- Testing MovePositionTowards() ---");

        // Test normal movement
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 1f, 1f, 2f, 1f);
            Debug.Log($"Normal movement from (0,0,0) towards (10,0,0): {result.Position} (expected: moved 2 units in X)");
            // Distance to target accounting for scales: 10 - (1+1)/2 = 9
            // Move distance: min(2*1, 9-1) = 2
            AssertApprox(result.Position.x, 2f, "Normal movement X");
            AssertApprox(result.Position.y, 0f, "Normal movement Y");
            AssertApprox(result.Position.z, 0f, "Normal movement Z");
        }

        // Test arrival at target
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // Target is at distance 1.5, with scales (1+1)/2 = 1 and distance threshold 1
            // Actual distance = 1.5 - 1 = 0.5, which is less than threshold 1
            var result = transform.MovePositionTowards(new float3(1.5f, 0, 0), 1f, 1f, 10f, 1f);
            Debug.Log($"Arrival at target: {result.Position} (expected: unchanged (0,0,0))");
            AssertApprox(result.Position.x, 0f, "Arrival - should not move");
        }

        // Test already at target
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(0, 0, 0), 1f, 0.01f, 5f, 1f);
            Debug.Log($"Already at target: {result.Position} (expected: unchanged (0,0,0))");
            AssertApprox(result.Position.x, 0f, "Already at target X");
            AssertApprox(result.Position.y, 0f, "Already at target Y");
            AssertApprox(result.Position.z, 0f, "Already at target Z");
        }

        // Test overshooting prevention
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // Distance to target: 5 - (1+1)/2 = 4
            // With distance threshold 1, we want to stop at distance 1 from adjusted target
            // So move distance = 4 - 1 = 3, but speed*deltaTime = 100
            // Should move min(100, 3) = 3
            var result = transform.MovePositionTowards(new float3(5, 0, 0), 1f, 1f, 100f, 1f);
            Debug.Log($"Overshooting prevention: {result.Position} (expected: (3,0,0))");
            AssertApprox(result.Position.x, 3f, "Overshooting prevention");
        }

        // Test after arrival, IsTargetReached returns true
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetPos = new float3(1.5f, 0, 0);
            float targetScale = 1f;
            float distance = 1f;
            
            var result = transform.MovePositionTowards(targetPos, targetScale, distance, 10f, 1f);
            bool reached = result.IsTargetDistanceReached(targetPos, targetScale, distance);
            Debug.Log($"After arrival, IsTargetReached: {reached} (expected: true)");
            AssertTrue(reached, "After arrival, IsTargetReached");
        }

        // Test with equal scales
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 2f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 2f, 0.5f, 3f, 1f);
            // Distance: 10 - (2+2)/2 = 8
            // Move: min(3, 8-0.5) = 3
            Debug.Log($"Equal scales: {result.Position} (expected: (3,0,0))");
            AssertApprox(result.Position.x, 3f, "Equal scales");
        }

        // Test with different scales
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 3f, 0.5f, 2f, 1f);
            // Distance: 10 - (1+3)/2 = 8
            // Move: min(2, 8-0.5) = 2
            Debug.Log($"Different scales: {result.Position} (expected: (2,0,0))");
            AssertApprox(result.Position.x, 2f, "Different scales");
        }

        // Test with zero scales
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 0f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 0f, 0.1f, 5f, 1f);
            // Distance: 10 - 0 = 10
            // Move: min(5, 10-0.1) = 5
            Debug.Log($"Zero scales: {result.Position} (expected: (5,0,0))");
            AssertApprox(result.Position.x, 5f, "Zero scales");
        }

        // Test with large scale difference
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 0.5f
            };
            var result = transform.MovePositionTowards(new float3(20, 0, 0), 10f, 1f, 3f, 1f);
            // Distance: 20 - (0.5+10)/2 = 14.75
            // Move: min(3, 14.75-1) = 3
            Debug.Log($"Large scale difference: {result.Position} (expected: (3,0,0))");
            AssertApprox(result.Position.x, 3f, "Large scale difference");
        }

        // Test zero speed
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 1f, 0.5f, 0f, 1f);
            Debug.Log($"Zero speed: {result.Position} (expected: (0,0,0))");
            AssertApprox(result.Position.x, 0f, "Zero speed");
        }

        // Test zero deltaTime
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 1f, 0.5f, 5f, 0f);
            Debug.Log($"Zero deltaTime: {result.Position} (expected: (0,0,0))");
            AssertApprox(result.Position.x, 0f, "Zero deltaTime");
        }

        // Test speed * deltaTime > remaining distance
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // Distance: 3 - (1+1)/2 = 2, threshold = 0.5
            // Remaining: 2 - 0.5 = 1.5
            // Speed * deltaTime = 100 * 1 = 100
            // Should move only 1.5
            var result = transform.MovePositionTowards(new float3(3, 0, 0), 1f, 0.5f, 100f, 1f);
            Debug.Log($"Speed * deltaTime > remaining: {result.Position} (expected: (1.5,0,0))");
            AssertApprox(result.Position.x, 1.5f, "Speed * deltaTime > remaining");
        }

        // Test zero distance threshold
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 1f, 0f, 2f, 1f);
            // Distance: 10 - (1+1)/2 = 9
            // Move: min(2, 9-0) = 2
            Debug.Log($"Zero distance threshold: {result.Position} (expected: (2,0,0))");
            AssertApprox(result.Position.x, 2f, "Zero distance threshold");
        }

        // Test large distance threshold
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // Distance: 10 - (1+1)/2 = 9
            // threshold = 50, so 9 - 50 = -41 < 0, should not move
            var result = transform.MovePositionTowards(new float3(10, 0, 0), 1f, 50f, 5f, 1f);
            Debug.Log($"Large distance threshold: {result.Position} (expected: (0,0,0))");
            AssertApprox(result.Position.x, 0f, "Large distance threshold");
        }

        // Test movement along Y axis
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(0, 10, 0), 1f, 0.5f, 3f, 1f);
            Debug.Log($"Movement along Y axis: {result.Position} (expected: (0,3,0))");
            AssertApprox(result.Position.y, 3f, "Movement along Y axis");
        }

        // Test movement along Z axis
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(0, 0, 10), 1f, 0.5f, 3f, 1f);
            Debug.Log($"Movement along Z axis: {result.Position} (expected: (0,0,3))");
            AssertApprox(result.Position.z, 3f, "Movement along Z axis");
        }

        // Test diagonal movement
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(10, 10, 0), 1f, 0.5f, 2f, 1f);
            // Direction: (10,10,0), length = sqrt(200) = 14.142
            // Adjusted distance: 14.142 - 1 = 13.142
            // Move: min(2, 13.142 - 0.5) = 2
            // Normalized direction: (10,10,0) / 14.142 = (0.707, 0.707, 0)
            // New position: (0,0,0) + (0.707, 0.707, 0) * 2 = (1.414, 1.414, 0)
            Debug.Log($"Diagonal movement: {result.Position} (expected: ~(1.414,1.414,0))");
            AssertApprox(result.Position.x, 1.414f, "Diagonal movement X", 0.01f);
            AssertApprox(result.Position.y, 1.414f, "Diagonal movement Y", 0.01f);
        }

        // Test very close positions
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            var result = transform.MovePositionTowards(new float3(0.001f, 0, 0), 1f, 0.0001f, 1f, 1f);
            // Distance: 0.001 - 1 = -0.999 (negative, but max(0, ...) makes it 0)
            // Should not move since distance is less than scales
            Debug.Log($"Very close positions: {result.Position}");
        }
    }

    #endregion

    #region RotateTowards Tests

    private void TestRotateTowards()
    {
        Debug.Log("\n--- Testing RotateTowards() ---");

        // Test no rotation needed
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(0, 0, 1);
            var result = transform.RotateTowards(targetDirection, 0f, 0.1f);
            Debug.Log($"No rotation needed: rotation unchanged (expected: true)");
            AssertTrue(math.all(result.Rotation.value == quaternion.identity.value), "No rotation needed");
        }

        // Test 90-degree rotation
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0); // 90 degrees to the right
            var result = transform.RotateTowards(targetDirection, 90f, 0.1f);
            Debug.Log($"90-degree rotation: {result.Rotation}");
            // Should rotate towards X axis
            float3 forward = math.mul(result.Rotation, new float3(0, 0, 1));
            Debug.Log($"Forward after rotation: {forward} (expected: close to (1,0,0))");
        }

        // Test 180-degree rotation
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(0, 0, -1); // 180 degrees
            var result = transform.RotateTowards(targetDirection, 180f, 0.1f);
            Debug.Log($"180-degree rotation: {result.Rotation}");
            float3 forward = math.mul(result.Rotation, new float3(0, 0, 1));
            Debug.Log($"Forward after rotation: {forward} (expected: close to (0,0,-1))");
        }

        // Test small angle rotation
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float angle = math.radians(10f);
            float3 targetDirection = new float3(math.sin(angle), 0, math.cos(angle));
            var result = transform.RotateTowards(targetDirection, 10f, 0.1f);
            Debug.Log($"Small angle (10 degrees) rotation: {result.Rotation}");
            float3 forward = math.mul(result.Rotation, new float3(0, 0, 1));
            Debug.Log($"Forward after rotation: {forward}");
        }

        // Test zero speed
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            var result = transform.RotateTowards(targetDirection, 0f, 0.1f);
            Debug.Log($"Zero speed: rotation unchanged");
            AssertTrue(math.all(result.Rotation.value == quaternion.identity.value), "Zero speed");
        }

        // Test very small speed
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            var result = transform.RotateTowards(targetDirection, 0.1f, 0.1f);
            Debug.Log($"Very small speed (0.1 degrees): {result.Rotation}");
            // Should rotate very slightly
        }

        // Test very large speed
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            var result = transform.RotateTowards(targetDirection, 1000f, 0.1f);
            Debug.Log($"Very large speed (1000 degrees): {result.Rotation}");
            // Should be clamped and rotate to target
            float3 forward = math.mul(result.Rotation, new float3(0, 0, 1));
            Debug.Log($"Forward after rotation: {forward} (expected: (1,0,0))");
            AssertApprox(forward.x, 1f, "Very large speed X", 0.1f);
        }

        // Test speed > 360
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            var result = transform.RotateTowards(targetDirection, 720f, 0.1f);
            Debug.Log($"Speed > 360 (720 degrees): {result.Rotation}");
            float3 forward = math.mul(result.Rotation, new float3(0, 0, 1));
            Debug.Log($"Forward after rotation: {forward}");
        }

        // Test zero direction vector
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(0, 0, 0);
            var result = transform.RotateTowards(targetDirection, 90f, 0.1f);
            Debug.Log($"Zero direction vector: {result.Rotation} (behavior may vary)");
        }

        // Test direction straight up
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(0, 1, 0);
            var result = transform.RotateTowards(targetDirection, 90f, 0.1f);
            Debug.Log($"Direction straight up: {result.Rotation}");
        }

        // Test direction straight down
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(0, -1, 0);
            var result = transform.RotateTowards(targetDirection, 90f, 0.1f);
            Debug.Log($"Direction straight down: {result.Rotation}");
        }

        // Test very small direction magnitude
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(0.0001f, 0, 0.0001f);
            var result = transform.RotateTowards(targetDirection, 45f, 0.1f);
            Debug.Log($"Very small direction magnitude: {result.Rotation}");
        }

        // Test interpolation accuracy (multiple small steps)
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            
            // Multiple small steps
            var result1 = transform.RotateTowards(targetDirection, 30f, 0.1f);
            var result2 = result1.RotateTowards(targetDirection, 30f, 0.1f);
            var result3 = result2.RotateTowards(targetDirection, 30f, 0.1f);
            
            float3 forwardMultiStep = math.mul(result3.Rotation, new float3(0, 0, 1));
            Debug.Log($"Multiple small steps (3x30deg): {forwardMultiStep}");
            
            // One large step
            var resultLarge = transform.RotateTowards(targetDirection, 90f, 0.1f);
            float3 forwardLargeStep = math.mul(resultLarge.Rotation, new float3(0, 0, 1));
            Debug.Log($"One large step (90deg): {forwardLargeStep}");
        }

        // Test rotation preservation (shortest path)
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            // Target slightly to the right
            float3 targetDirection = new float3(0.1f, 0, 1);
            var result = transform.RotateTowards(targetDirection, 1f, 0.1f);
            Debug.Log($"Rotation preservation (shortest path): {result.Rotation}");
        }

        // Test various delta values
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            
            var result1 = transform.RotateTowards(targetDirection, 90f, 0.001f);
            var result2 = transform.RotateTowards(targetDirection, 90f, 0.1f);
            var result3 = transform.RotateTowards(targetDirection, 90f, 1.0f);
            
            Debug.Log($"Various delta values - delta doesn't affect rotation in current implementation");
            Debug.Log($"Delta 0.001: {result1.Rotation}");
            Debug.Log($"Delta 0.1: {result2.Rotation}");
            Debug.Log($"Delta 1.0: {result3.Rotation}");
        }
    }

    #endregion

    #region Integration Tests

    private void TestIntegration()
    {
        Debug.Log("\n--- Testing Integration Scenarios ---");

        // Test MovePositionTowards until IsTargetReached returns true
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetPos = new float3(10, 0, 0);
            float targetScale = 1f;
            float distance = 0.5f;
            float speed = 2f;
            float deltaTime = 0.1f;

            int iterations = 0;
            int maxIterations = 100;
            
            while (!transform.IsTargetDistanceReached(targetPos, targetScale, distance) && iterations < maxIterations)
            {
                transform = transform.MovePositionTowards(targetPos, targetScale, distance, speed, deltaTime);
                iterations++;
            }

            Debug.Log($"MovePositionTowards iterations until target reached: {iterations}");
            Debug.Log($"Final position: {transform.Position}");
            bool reached = transform.IsTargetDistanceReached(targetPos, targetScale, distance);
            Debug.Log($"IsTargetReached after movement: {reached} (expected: true)");
            AssertTrue(reached, "Integration: MovePositionTowards until reached");
        }

        // Test RotateTowards until IsLookingTowards returns true
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetDirection = new float3(1, 0, 0);
            float rotationSpeed = 45f;
            float deltaTime = 0.1f;
            float delta = 0.1f;

            int iterations = 0;
            int maxIterations = 100;
            
            while (!transform.Rotation.IsLookingTowards(targetDirection, delta) && iterations < maxIterations)
            {
                transform = transform.RotateTowards(targetDirection, rotationSpeed * deltaTime, delta);
                iterations++;
            }

            Debug.Log($"RotateTowards iterations until looking at target: {iterations}");
            bool looking = transform.Rotation.IsLookingTowards(targetDirection, delta);
            Debug.Log($"IsLookingTowards after rotation: {looking} (expected: true)");
            AssertTrue(looking, "Integration: RotateTowards until looking");
        }

        // Test combined movement and rotation
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            float3 targetPos = new float3(10, 0, 10);
            float3 lookDirection = math.normalize(targetPos - transform.Position);
            
            float targetScale = 1f;
            float distance = 0.5f;
            float speed = 3f;
            float rotationSpeed = 90f;
            float deltaTime = 0.3f;
            float delta = 0.1f;

            int iterations = 0;
            int maxIterations = 10000;
            
            while ((!transform.IsTargetDistanceReached(targetPos, targetScale, distance) || 
                    !transform.Rotation.IsLookingTowards(lookDirection, delta)) && 
                   iterations < maxIterations)
            {
                transform = transform.MovePositionTowards(targetPos, targetScale, distance, speed, deltaTime);
                transform = transform.RotateTowards(lookDirection, rotationSpeed * deltaTime, delta);
                iterations++;
            }

            Debug.Log($"Combined movement and rotation iterations: {iterations}");
            Debug.Log($"Final position: {transform.Position}");
            bool posReached = transform.IsTargetDistanceReached(targetPos, targetScale, distance);
            bool rotReached = transform.Rotation.IsLookingTowards(lookDirection, delta);
            Debug.Log($"Position reached: {posReached}, Rotation reached: {rotReached} (expected: both true)");
            AssertTrue(posReached && rotReached, "Integration: Combined movement and rotation");
        }

        // Test the specific call patterns from MovingSystem.cs
        {
            var transform = new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            };
            
            // Simulating MoveControllerInputComponent
            float3 targetPosition = new float3(20, 0, 0);
            float targetScale = 1f;
            float inputDistance = 1f;
            float3 lookDirection = new float3(1, 0, 0);
            float inputSpeed = 5f;
            float inputRotationSpeed = 180f;
            float deltaTime = 0.016f; // ~60 FPS

            // Simulate the exact calls from MovingSystem
            bool hasArrived = transform.IsTargetDistanceReached(targetPosition, targetScale, inputDistance);
            Debug.Log($"HasArrived initially: {hasArrived} (expected: false)");
            
            bool isLookingAt = transform.Rotation.IsLookingTowards(lookDirection, 0.1f);
            Debug.Log($"IsLookingAt initially: {isLookingAt} (expected: true - same direction)");

            if (!hasArrived)
            {
                transform = transform.MovePositionTowards(targetPosition, targetScale, inputDistance * 0.9f, inputSpeed, deltaTime);
            }

            if (!isLookingAt)
            {
                transform = transform.RotateTowards(lookDirection, inputRotationSpeed * deltaTime, 0.1f);
            }

            Debug.Log($"Position after MovingSystem simulation: {transform.Position}");
            Debug.Log($"Rotation after MovingSystem simulation: {transform.Rotation}");
        }
    }

    #endregion

    #region Helper Methods

    private void AssertTrue(bool condition, string testName)
    {
        if (!condition)
        {
            Debug.LogError($"FAILED: {testName} - Expected: true, Got: false");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }

    private void AssertFalse(bool condition, string testName)
    {
        if (condition)
        {
            Debug.LogError($"FAILED: {testName} - Expected: false, Got: true");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }

    private void AssertApprox(float actual, float expected, string testName, float tolerance = 0.01f)
    {
        if (math.abs(actual - expected) > tolerance)
        {
            Debug.LogError($"FAILED: {testName} - Expected: {expected}, Got: {actual}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }

    #endregion
}

