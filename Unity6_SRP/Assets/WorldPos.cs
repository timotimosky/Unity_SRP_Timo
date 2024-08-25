using UnityEngine;
//用一个long（int64)来储存，前32存x，后32位存y
public class WorldPos : MonoBehaviour
{
    public long GetID(int q, int r)
    {
        long rt = (long)q << 32;
        rt = rt | (r & 0xffffffff);
        return rt;
    }

    public Vector2Int GetCoordinate(long id)
    {
        int x = (int)(id >> 32);
        int y = (int)id;
        return new Vector2Int(x, y);
    }
}
