using Unity.Entities;
using UnityEngine;

public class RunConditionTestAuthoring : MonoBehaviour
{
    class Baker : Baker<RunConditionTestAuthoring>
    {
        public override void Bake(RunConditionTestAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunConditionTestComponent());
        }
    }
}

