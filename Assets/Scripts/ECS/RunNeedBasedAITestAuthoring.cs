using Unity.Entities;
using UnityEngine;

public class RunNeedBasedAITestAuthoring : MonoBehaviour
{
    public bool RunTests = true;

    public class RunNeedBasedAITestBaker : Baker<RunNeedBasedAITestAuthoring>
    {
        public override void Bake(RunNeedBasedAITestAuthoring authoring)
        {
            if (!authoring.RunTests)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunNeedBasedAITestComponent());
        }
    }
}

