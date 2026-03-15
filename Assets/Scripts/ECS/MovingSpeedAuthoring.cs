using Unity.Entities;
using UnityEngine;

public class MovingSpeedAuthoring : MonoBehaviour
{
    public float MaxSpeed = 1.0f;
    public float MaxRotationSpeed = 30f;

    public class MovingSpeedBaker : Baker<MovingSpeedAuthoring>
    {
        public override void Bake(MovingSpeedAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new MovingSpeedComponent
            {
                MaxSpeed = authoring.MaxSpeed,
                MaxRotationSpeed = authoring.MaxRotationSpeed
            });

            AddComponent(entity, new MoveControllerInputComponent());
        }
    }
}

