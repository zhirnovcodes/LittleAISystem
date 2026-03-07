using Unity.Entities;
using UnityEngine;

public class RunTransformExtensionsTestAuthoring : MonoBehaviour
{
    public bool RunTests = true;

    public class RunTransformExtensionsTestBaker : Baker<RunTransformExtensionsTestAuthoring>
    {
        public override void Bake(RunTransformExtensionsTestAuthoring authoring)
        {
            if (!authoring.RunTests)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunTransformExtensionsTestComponent());
        }
    }
}

