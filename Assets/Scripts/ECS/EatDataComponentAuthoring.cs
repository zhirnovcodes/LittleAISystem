using Unity.Entities;
using UnityEngine;

public class EatDataComponentAuthoring : MonoBehaviour
{
    public float Interval;
    public float FailTime;
    public float MaxDistance;
    public float BiteSize;

    class Baker : Baker<EatDataComponentAuthoring>
    {
        public override void Bake(EatDataComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EatDataComponent
            {
                Interval = authoring.Interval,
                FailTime = authoring.FailTime,
                MaxDistance = authoring.MaxDistance,
                BiteSize = authoring.BiteSize
            });
        }
    }
}

