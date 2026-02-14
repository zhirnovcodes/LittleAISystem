using Unity.Entities;
using UnityEngine;

public class TriggerItemAuthoring : MonoBehaviour
{
    class Baker : Baker<TriggerItemAuthoring>
    {
        public override void Bake(TriggerItemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<TriggerItem>(entity);
        }
    }
}

