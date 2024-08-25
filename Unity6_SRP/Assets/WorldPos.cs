using UnityEngine;
//��һ��long��int64)�����棬ǰ32��x����32λ��y
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
