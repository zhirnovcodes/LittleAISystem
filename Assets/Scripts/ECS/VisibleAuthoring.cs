using Unity.Entities;
using UnityEngine;

public class VisibleAuthoring : MonoBehaviour
{
    public class VisibleBaker : Baker<VisibleAuthoring>
    {
        public override void Bake(VisibleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new VisibleComponent());
            AddBuffer<VisibleItem>(entity);
        }
    }
}

