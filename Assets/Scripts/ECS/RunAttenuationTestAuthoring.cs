using Unity.Entities;
using UnityEngine;

public class RunAttenuationTestAuthoring : MonoBehaviour
{
    public bool RunTests = true;

    public class RunAttenuationTestBaker : Baker<RunAttenuationTestAuthoring>
    {
        public override void Bake(RunAttenuationTestAuthoring authoring)
        {
            if (!authoring.RunTests)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunAttenuationTestComponent());
        }
    }
}

