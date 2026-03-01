using Unity.Entities;
using UnityEngine;

public class SleepDataComponentAuthoring : MonoBehaviour
{
    public float FailTime;
    public float MaxDistance;
    public float LayDownFailTime;
    public float Distance;

    class Baker : Baker<SleepDataComponentAuthoring>
    {
        public override void Bake(SleepDataComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SleepDataComponent
            {
                FailTime = authoring.FailTime,
                MaxDistance = authoring.MaxDistance,
                LayDownFailTime = authoring.LayDownFailTime,
                Distance = authoring.Distance
            });
        }
    }
}

