using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour
{
    public Vector2Int Size;
    public Grid Grid;
    public GameObject Prefab;

    private void Start()
    {
        var offset = new Vector3(0, 0, 1) * Grid.cellSize.z / 2f;

        for (int x = 0; x < Size.x; x++)
        {
            for (int z = 0; z < Size.y; z++)
            {
                var instance = GameObject.Instantiate(Prefab);
                var position = Grid.GetCellCenterWorld(new Vector3Int(x, 0, z));
                position += x % 2f == 0 ? Vector3.zero : offset;

                instance.transform.position = position;
            }
        }
    }
}
