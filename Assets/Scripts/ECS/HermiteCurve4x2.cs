using UnityEngine;

public struct HermiteCurve4x2
{
    public HermiteCurve4 c0;
    public HermiteCurve4 c1;

    public HermiteCurve this[int x, int y]
    {
        get
        {
            if (y == 0)
                return c0[x];
            else if (y == 1)
                return c1[x];
            else
            {
                Debug.LogError($"Invalid index [{x},{y}] for HermiteCurve4x2");
                return default;
            }
        }
        set
        {
            if (y == 0)
                c0[x] = value;
            else if (y == 1)
                c1[x] = value;
            else
                Debug.LogError($"Invalid index [{x},{y}] for HermiteCurve4x2");
        }
    }
}

