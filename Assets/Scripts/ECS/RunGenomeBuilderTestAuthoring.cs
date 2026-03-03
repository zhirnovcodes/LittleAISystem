using Unity.Entities;
using UnityEngine;

public class RunGenomeBuilderTestAuthoring : MonoBehaviour
{
    public bool RunTests = true;

    class Baker : Baker<RunGenomeBuilderTestAuthoring>
    {
        public override void Bake(RunGenomeBuilderTestAuthoring authoring)
        {
            if (!authoring.RunTests)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunGenomeBuilderTestComponent());
        }
    }
}

