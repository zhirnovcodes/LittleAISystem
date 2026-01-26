using Unity.Entities;
using UnityEngine;

public class RunAttenuationTestAuthoring : MonoBehaviour
{
    public class RunAttenuationTestBaker : Baker<RunAttenuationTestAuthoring>
    {
        public override void Bake(RunAttenuationTestAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunAttenuationTestComponent());
        }
    }
}

