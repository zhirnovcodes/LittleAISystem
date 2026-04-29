using Unity.Entities;
using UnityEngine;

public class MoveAuthoring : MonoBehaviour
{
    class Baker : Baker<MoveAuthoring>
    {
        public override void Bake(MoveAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveInputComponent());
            AddComponent(entity, new MoveOutputComponent());
        }
    }
}
