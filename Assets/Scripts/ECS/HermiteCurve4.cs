using UnityEngine;

[System.Serializable]
public struct HermiteCurve4
{
    public HermiteCurve x;
    public HermiteCurve y;
    public HermiteCurve z;
    public HermiteCurve w;

    public HermiteCurve this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
                case 3: return w;
                default:
                    Debug.LogError($"Invalid index [{index}] for HermiteCurve4");
                    return default;
            }
        }
        set
        {
            switch (index)
            {
                case 0: x = value; break;
                case 1: y = value; break;
                case 2: z = value; break;
                case 3: w = value; break;
                default:
                    Debug.LogError($"Invalid index [{index}] for HermiteCurve4");
                    break;
            }
        }
    }
}

