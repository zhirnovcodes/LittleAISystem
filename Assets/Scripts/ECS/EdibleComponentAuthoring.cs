using Unity.Entities;
using UnityEngine;

public class EdibleComponentAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject EdibleBody;
    [SerializeField] private float Nutrition = 10f;

    class Baker : Baker<EdibleComponentAuthoring>
    {
        public override void Bake(EdibleComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            var edibleBodyEntity = GetEntity(authoring.EdibleBody, TransformUsageFlags.Dynamic);

            
            AddComponent(entity, new EdibleComponent
            {
                EdibleBody = edibleBodyEntity,
                Nutrition = authoring.Nutrition
            });
        }
    }
}

