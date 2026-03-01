using Unity.Entities;
using Unity.Mathematics;

public struct MovingDataComponent : IComponentData
{
    public float MaxSpeed;
    public float MaxRotationSpeed;
    public float RotateFailTime;
    public float MoveFailTime;
    public float CrawlingSpeedT;
    public float WalkingSpeedT;
    public float WalkingRotationSpeedT;
    public float IdleTime;

    public float4x4 ToFloat4x4()
    {
        return new float4x4(
            new float4(MaxSpeed, MaxRotationSpeed, RotateFailTime, MoveFailTime),
            new float4(CrawlingSpeedT, WalkingSpeedT, WalkingRotationSpeedT, IdleTime),
            float4.zero,
            float4.zero
        );
    }

    public static MovingDataComponent FromFloat4x4(float4x4 data)
    {
        return new MovingDataComponent
        {
            MaxSpeed = data.c0.x,
            MaxRotationSpeed = data.c0.y,
            RotateFailTime = data.c0.z,
            MoveFailTime = data.c0.w,
            CrawlingSpeedT = data.c1.x,
            WalkingSpeedT = data.c1.y,
            WalkingRotationSpeedT = data.c1.z,
            IdleTime = data.c1.w
        };
    }
}

