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
                    Target = GetEntity(entry.Target, TransformUsageFlags.Dynamic),
                    Speed = entry.Speed,
                    RotationSpeed = entry.RotationSpeed,
                    MaxDistance = entry.MaxDistance,
                });
            }
        }
    }
}
