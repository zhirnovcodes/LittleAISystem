using Unity.Entities;
using UnityEngine;

public class RotationHandlerAuthoring : MonoBehaviour 
{
    public class Baker : Baker<RotationHandlerAuthoring>
    {
        public override void Bake(RotationHandlerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var parent = GetEntity(authoring.transform.parent, TransformUsageFlags.Dynamic);
            AddComponent(entity, new RotationHandlerComponent
            {
                Parent = parent
            });
        }
    }
}