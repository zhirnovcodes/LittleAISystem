using Unity.Entities;
using UnityEngine;

public class TalkingDataComponentAuthoring : MonoBehaviour
{
    public float StumbleFailTime;
    public float MaxDistance;
    public float SocialIncrease;
    public bool HasMaleGenitalia;

    class Baker : Baker<TalkingDataComponentAuthoring>
    {
        public override void Bake(TalkingDataComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TalkingDataComponent
            {
                StumbleFailTime = authoring.StumbleFailTime,
                MaxDistance = authoring.MaxDistance,
                SocialIncrease = authoring.SocialIncrease,
                HasMaleGenitalia = authoring.HasMaleGenitalia
            });
        }
    }
}

