using Unity.Entities;
using UnityEngine;

public class VisionDataAuthoring : MonoBehaviour
{
    public float MaxDistance;
    public float Interval;

    class Baker : Baker<VisionDataAuthoring>
    {
        public override void Bake(VisionDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new VisionData
            {
                MaxDistance = authoring.MaxDistance,
                Interval = authoring.Interval
            });
        }
    }
}

