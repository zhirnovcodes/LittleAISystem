using Unity.Entities;
using UnityEngine;

public class BaseConditionFlagsAuthoring : MonoBehaviour
{
    [SerializeField] private ConditionFlags BaseConditions = ConditionFlags.None;

    class Baker : Baker<BaseConditionFlagsAuthoring>
    {
        public override void Bake(BaseConditionFlagsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ConditionFlagsComponent
            {
                Conditions = authoring.BaseConditions
            });
        }
    }
}

