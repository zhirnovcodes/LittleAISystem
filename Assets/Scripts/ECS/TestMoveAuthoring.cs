using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TestMoveAuthoring : MonoBehaviour
{
    [Serializable]
    public class TargetEntry
    {
        public GameObject Target;
        public Vector3 TargetPosition;
        public float Speed;
        public float RotationSpeed;
        public float MaxDistance;
    }

    public List<TargetEntry> Targets = new List<TargetEntry>();

    class Baker : Baker<TestMoveAuthoring>
    {
        public override void Bake(TestMoveAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new TestMoveComponent { CurrentIndex = -1 });

            var buffer = AddBuffer<TestMoveTargetItem>(entity);

            foreach (var entry in authoring.Targets)
            {
                buffer.Add(new TestMoveTargetItem
                {
                    Target = entry.Target != null ? GetEntity(entry.Target, TransformUsageFlags.Dynamic) : Unity.Entities.Entity.Null,
                    TargetPosition = entry.TargetPosition,
                    Speed = entry.Speed,
                    RotationSpeed = entry.RotationSpeed,
                    MaxDistance = entry.MaxDistance,
                });
            }
        }
    }
}
