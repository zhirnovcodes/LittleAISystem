using Unity.Entities;

public struct MovingSpeedComponent : IComponentData
{
    public float MaxSpeed;
    public float MaxRotationSpeed;

    public float GetWalkingSpeed() => MaxSpeed * 0.5f;
    public float GetRunningSpeed() => MaxSpeed;
    public float GetCrawlingSpeed() => MaxSpeed * 0.25f;

    public float GetWalkingRotationSpeed() => MaxRotationSpeed * 0.5f;
    public float GetRunningRotationSpeed() => MaxRotationSpeed;
    public float GetCrawlingRotationSpeed() => MaxRotationSpeed * 0.25f;
}

