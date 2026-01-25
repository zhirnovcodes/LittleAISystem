namespace LittleAI.Enums
{
    public enum ActionTypes : byte
    {
        Idle,
        Search,
        Eat,
        Sleep,
        FallAsleep,
        Escape,
        Communicate
    }

    public enum SubActionTypes
    {
        Idle,
        Search,
        MoveTo,
        MoveInto,
        MoveToTalk,
        RunFrom,
        RotateTowards,
        Eat,
        Sleep,
        StumbleUpon, 
        Communicate
    }
}