using Unity.Entities;
using UnityEngine;

public class AnimalStatsDataAuthoring : MonoBehaviour
{
    public AnimalStats Stats;

    class Baker : Baker<AnimalStatsDataAuthoring>
    {
        public override void Bake(AnimalStatsDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new AnimalStatsData
            {
                Stats = authoring.Stats
            });
        }
    }
}

