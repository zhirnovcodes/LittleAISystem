using Unity.Entities;
using UnityEngine;

public class SleepingPlaceComponentAuthoring : MonoBehaviour
{
    [SerializeField] private float EnergyReplanish = 10f;

    class Baker : Baker<SleepingPlaceComponentAuthoring>
    {
        public override void Bake(SleepingPlaceComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new SleepingPlaceComponent
            {
                EnergyReplanish = authoring.EnergyReplanish
            });
        }
    }
}

