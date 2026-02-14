using Unity.Entities;
using UnityEngine;

public class VisionComponentAuthoring : MonoBehaviour
{
    [SerializeField] private float MaxDistance = 10f;
    [SerializeField] private float Interval = 0.5f;

    class Baker : Baker<VisionComponentAuthoring>
    {
        public override void Bake(VisionComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            
            AddComponent(entity, new VisionComponent
            {
                MaxDistance = authoring.MaxDistance,
                Interval = authoring.Interval,
                TimeElapsed = 0f
            });
            
            AddBuffer<VisibleItem>(entity);
        }
    }
}

