using UnityEngine;

public struct HermiteCurve4x2
{
    public HermiteCurve Value00;
    public HermiteCurve Value01;
    public HermiteCurve Value10;
    public HermiteCurve Value11;
    public HermiteCurve Value20;
    public HermiteCurve Value21;
    public HermiteCurve Value30;
    public HermiteCurve Value31;

    public HermiteCurve this[int x, int y]
    {
        get
        {
            switch (x)
            {
                case 0:
                    return y == 0 ? Value00 : Value01;
                case 1:
                    return y == 0 ? Value10 : Value11;
                case 2:
                    return y == 0 ? Value20 : Value21;
                case 3:
                    return y == 0 ? Value30 : Value31;
                default:
                    Debug.LogError($"Invalid index [{x},{y}] for HermiteCurve4x2");
                    return default;
            }
        }
        set
        {
            switch (x)
            {
                case 0:
                    if (y == 0) Value00 = value;
                    else Value01 = value;
                    break;
                case 1:
                    if (y == 0) Value10 = value;
                    else Value11 = value;
                    break;
                case 2:
                    if (y == 0) Value20 = value;
                    else Value21 = value;
                    break;
                case 3:
                    if (y == 0) Value30 = value;
                    else Value31 = value;
                    break;
                default:
                    Debug.LogError($"Invalid index [{x},{y}] for HermiteCurve4x2");
                    break;
            }
        }
    }
}

