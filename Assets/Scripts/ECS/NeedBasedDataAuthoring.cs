using Unity.Entities;
using UnityEngine;

public class NeedBasedDataAuthoring : MonoBehaviour
{
    public float CancelThreshold;
    public float AddThreshold;

    class Baker : Baker<NeedBasedDataAuthoring>
    {
        public override void Bake(NeedBasedDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new NeedBasedData
            {
                CancelThreshold = authoring.CancelThreshold,
                AddThreshold = authoring.AddThreshold
            });
        }
    }
}

