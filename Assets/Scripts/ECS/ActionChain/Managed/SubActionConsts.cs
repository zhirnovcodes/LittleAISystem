public static class SubActionConsts
{
    public static class Idle
    {
        public const float IdleTime = 20f;
        public const float WanderRadius = 10f;
        public const float SpeedMultiplier = 0.6f;
    }

    public static class WalkTo
    {
        public const float MaxDistance = 0.4f;
        public const float FailTime = 30f;
    }

    public static class Eat
    {
        public const float FailTime = 20f;
        public const float MaxDistance = 0.4f;
    }

    public static class Communicate
    {
        public const float MaxDistance = 0.4f;
    }

    public static class StumbleUpon
    {
        public const float FailTime = 2f;
        public const float MaxDistance = 0.3f;
        public const float Delta = 1f;
    }

    public static class Sleeping
    {
        public const float FailTime = 100f;
        public const float MaxDistance = 0.1f;
    }

    public static class RunFrom
    {
        public const float SafeDistance = 10f;
    }

    public static class RotateTowards
    {
        public const float FailTime = 10f;
    }

    public static class LayDown
    {
        public const float FailTime = 5f;
        public const float Distance = 0.01f;
    }

    public static class TestMoveTo
    {
        public const float MoveSpeed = 5f;
        public const float RotationSpeed = 180f;
    }

    public static class TestEat
    {
        public const float EatDuration = 3f;
        public const float MoveSpeed = 2f;
        public const float RotationSpeed = 180f;
    }
}

