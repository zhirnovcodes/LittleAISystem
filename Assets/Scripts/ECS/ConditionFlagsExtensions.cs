public static class ConditionFlagsExtensions
{
    /// <summary>
    /// Checks if the actor's conditions meet the expected conditions.
    /// Returns true if ALL expected conditions are present in the actor's conditions.
    /// </summary>
    /// <param name="actorConditions">The conditions the actor has</param>
    /// <param name="expectedConditions">The conditions required</param>
    /// <returns>True if all expected conditions are met</returns>
    public static bool IsConditionMet(this ConditionFlags actorConditions, ConditionFlags expectedConditions)
    {
        // If no conditions are expected, always return true
        if (expectedConditions == ConditionFlags.None)
            return true;

        // Check if all expected flags are present in actor conditions
        return (actorConditions & expectedConditions) == expectedConditions;
    }
}

