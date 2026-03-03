using Unity.Entities;
using UnityEngine;

public class RunConditionTestAuthoring : MonoBehaviour
{
    public bool RunTests = true;

    class Baker : Baker<RunConditionTestAuthoring>
    {
        public override void Bake(RunConditionTestAuthoring authoring)
        {
            if (!authoring.RunTests)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunConditionTestComponent());
        }
    }
}

