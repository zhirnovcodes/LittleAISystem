using LittleAI.Enums;

public struct SubActionResult
{
    public SubActionStatus Status;
    public int FailCode;

    public SubActionResult(SubActionStatus status, int failCode = 0)
    {
        Status = status;
        FailCode = failCode;
    }

    public static SubActionResult Running() => new SubActionResult(SubActionStatus.Running);
    public static SubActionResult Success() => new SubActionResult(SubActionStatus.Success);
    public static SubActionResult Fail(int failCode = 0) => new SubActionResult(SubActionStatus.Fail, failCode);
    public static SubActionResult Cancel(int failCode = 0) => new SubActionResult(SubActionStatus.Cancel, failCode);
}

