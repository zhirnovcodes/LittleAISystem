using Unity.Entities;
using UnityEngine;

public class ReproductionAuthoring : MonoBehaviour
{
    public bool IsMale;
    public float GestationTime = 10f;

    class Baker : Baker<ReproductionAuthoring>
    {
        public override void Bake(ReproductionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new GenetaliaComponent 
            {
                IsEnabled = false,
                IsMale = authoring.IsMale
            });

            if (authoring.IsMale == false)
            {
                AddComponent(entity, new ReproductionComponent
                {
                    IsMale = authoring.IsMale,
                    GestationTime = authoring.GestationTime,
                    TimeElapsed = 0f
                });

                // Start disabled
                SetComponentEnabled<ReproductionComponent>(entity, false);

                AddBuffer<DNAStorageItem>(entity);
            }

        }
    }
}

