using System;

[Flags]
public enum ConditionFlags : uint
{
    None = 0,
    IsAnimal = 1 << 0,
    IsPlant = 1 << 1,
    IsPredator = 1 << 2,
    IsHerbivore = 1 << 3,
    IsWithMaleGenitalia = 1 << 4,
    IsWithFemaleGenitalia = 1 << 5,
}

