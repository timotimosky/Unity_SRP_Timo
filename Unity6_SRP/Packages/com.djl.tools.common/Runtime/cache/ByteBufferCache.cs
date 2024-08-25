using System;

/**   
* byteBuffer缓冲池 
* @author  djl  
* @date 2017/08/07
*/
public static  class ByteBufferCache  {


    private const int MaxSize = 1024;


    [ThreadStatic]
    private static byte[] CachedInstance;

    public static void InitCache()
    {
        if (CachedInstance ==null)
            CachedInstance = new byte[1024];
    }

    public static byte[] Acquire(int capacity = 256)
    {
        InitCache();
        if (capacity <= MaxSize)
        {
            byte[] byteRoot = CachedInstance;
            if (byteRoot != null && capacity <= byteRoot.Length)
            {
                DebugTool.LogError("复用buffer");
                CachedInstance = null;
                Array.Clear(byteRoot, 0, byteRoot.Length);
                return byteRoot;
            }
        }
        return new byte[capacity];
    }

    public static void Release(byte[] sb)
    {
        if (sb.Length <= MaxSize)
        {
            CachedInstance = sb;
        }
    }

    public static void Delete()
    {

        CachedInstance = null;
    }


}
