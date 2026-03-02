using Unity.Entities;
using UnityEngine;

public class AgingAuthoring : MonoBehaviour
{
    public float MinSize = 0.5f;
    public float MaxSize = 1.0f;

    public class AgingBaker : Baker<AgingAuthoring>
    {
        public override void Bake(AgingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AgingComponent
            {
                MinSize = authoring.MinSize,
                MaxSize = authoring.MaxSize
            });
        }
    }
}

