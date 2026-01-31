using Unity.Entities;
using UnityEngine;

public class AnimalStatsAuthoring : MonoBehaviour
{
    public AnimalStats InitialStats;
    public bool WithStatsChangeBuffer = true;

    public class AnimalStatsBaker : Baker<AnimalStatsAuthoring>
    {
        public override void Bake(AnimalStatsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AnimalStatsComponent
            {
                Stats = authoring.InitialStats
            });

            if (authoring.WithStatsChangeBuffer)
            {
                AddBuffer<StatsChangeItem>(entity);
            }
        }
    }
}

