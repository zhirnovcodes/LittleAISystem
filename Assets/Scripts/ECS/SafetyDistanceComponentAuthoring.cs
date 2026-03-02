using Unity.Entities;
using UnityEngine;

public class SafetyDistanceComponentAuthoring : MonoBehaviour
{
    public float SafeDistance;
    public float CheckInterval;

    class Baker : Baker<SafetyDistanceComponentAuthoring>
    {
        public override void Bake(SafetyDistanceComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SafetyDataComponent
            {
                SafeDistance = authoring.SafeDistance,
                CheckInterval = authoring.CheckInterval
            });
        }
    }
}

