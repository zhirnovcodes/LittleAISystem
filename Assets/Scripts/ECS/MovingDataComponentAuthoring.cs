using Unity.Entities;
using UnityEngine;

public class MovingDataComponentAuthoring : MonoBehaviour
{
    public float MaxSpeed;
    public float MaxRotationSpeed;
    public float RotateFailTime;
    public float MoveFailTime;
    public float CrawlingSpeedT;
    public float WalkingSpeedT;
    public float WalkingRotationSpeedT;
    public float IdleTime;

    class Baker : Baker<MovingDataComponentAuthoring>
    {
        public override void Bake(MovingDataComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MovingDataComponent
            {
                MaxSpeed = authoring.MaxSpeed,
                MaxRotationSpeed = authoring.MaxRotationSpeed,
                RotateFailTime = authoring.RotateFailTime,
                MoveFailTime = authoring.MoveFailTime,
                CrawlingSpeedT = authoring.CrawlingSpeedT,
                WalkingSpeedT = authoring.WalkingSpeedT,
                WalkingRotationSpeedT = authoring.WalkingRotationSpeedT,
                IdleTime = authoring.IdleTime
            });
        }
    }
}

