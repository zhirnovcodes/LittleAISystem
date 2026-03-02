using Unity.Entities;
using UnityEngine;

public class TransformDataAuthoring : MonoBehaviour
{
    public float MinSize;
    public float MaxSize;

    class Baker : Baker<TransformDataAuthoring>
    {
        public override void Bake(TransformDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TransformData
            {
                MinSize = authoring.MinSize,
                MaxSize = authoring.MaxSize
            });
        }
    }
}

