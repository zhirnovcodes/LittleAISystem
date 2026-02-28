using Unity.Entities;
using UnityEngine;

public class FemaleGenetaliaComponentAuthoring : MonoBehaviour
{
    class Baker : Baker<FemaleGenetaliaComponentAuthoring>
    {
        public override void Bake(FemaleGenetaliaComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new FemaleGenetaliaComponent
            {
            });

            AddBuffer<FemaleTubeItem>(entity);
        }
    }
}

