using Unity.Entities;
using UnityEngine;

public class RunNeedBasedAITestAuthoring : MonoBehaviour
{
    public class RunNeedBasedAITestBaker : Baker<RunNeedBasedAITestAuthoring>
    {
        public override void Bake(RunNeedBasedAITestAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunNeedBasedAITestComponent());
        }
    }
}

