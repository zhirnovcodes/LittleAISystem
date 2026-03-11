using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MoveLimitationAuthoring : MonoBehaviour
{
    public Transform Limitation;

    class Baker : Baker<MoveLimitationAuthoring>
    {
        public override void Bake(MoveLimitationAuthoring authoring)
        { 
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveLimitationComponent
            {
                Central = authoring.Limitation?.position ?? authoring.transform.position,
                Scale = authoring.Limitation?.localScale ?? new float3(10,10,10)
            });
        }
    }
}
