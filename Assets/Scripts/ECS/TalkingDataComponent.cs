using Unity.Entities;
using Unity.Mathematics;

public struct TalkingDataComponent : IComponentData
{
    public float StumbleFailTime;
    public float MaxDistance;
    public float SocialIncrease;
    public bool HasMaleGenitalia;

    public float4 ToFloat4()
    {
        return new float4(StumbleFailTime, MaxDistance, SocialIncrease, HasMaleGenitalia ? 1f : 0f);
    }

    public static TalkingDataComponent FromFloat4(float4 data)
    {
        return new TalkingDataComponent
        {
            StumbleFailTime = data.x,
            MaxDistance = data.y,
            SocialIncrease = data.z,
            HasMaleGenitalia = data.w >= 0.5f
        };
    }
}

