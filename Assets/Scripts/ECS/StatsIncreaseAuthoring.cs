using Unity.Entities;
using UnityEngine;

public class StatsIncreaseAuthoring : MonoBehaviour
{
    public AnimalStats StatIncreasePerSecond;

    public class StatsIncreaseBaker : Baker<StatsIncreaseAuthoring>
    {
        public override void Bake(StatsIncreaseAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new StatsIncreaseComponent
            {
                AnimalStats = authoring.StatIncreasePerSecond
            });
        }
    }
}

