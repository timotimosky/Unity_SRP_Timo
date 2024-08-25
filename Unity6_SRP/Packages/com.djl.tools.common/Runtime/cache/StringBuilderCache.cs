using UnityEngine;
using System.Collections;
using System.Text;
using System;

/**   
* string缓冲池
* @author  djl  
* @date 2017/08/07
*/
public static class StringBuilderCache
{

    private const int MaxSize = 0x200;


    [ThreadStatic]
    private static StringBuilder CachedInstance;


    public static string Append(string a)
    {
        StringBuilder mStringBuilder = Acquire(a.Length);
        mStringBuilder.Append(a);
        return GetStringAndRelease(mStringBuilder);
    }

    public static string Append(string a, string b)
    {

       StringBuilder mStringBuilder = Acquire(a.Length + b.Length);
       mStringBuilder.Append(a);
       mStringBuilder.Append(b);
       return GetStringAndRelease(mStringBuilder);
    }
    public static string Append(string a, params object[] c)
    {

        StringBuilder mStringBuilder = Acquire();
        mStringBuilder.Append(a);
        mStringBuilder.Append(c);
        return GetStringAndRelease(mStringBuilder);
    }
    public static string Append(params object[] c)
    {
        StringBuilder mStringBuilder = Acquire();
        for (int i = 0; i < c.Length; i++)
        {
            mStringBuilder.Append(c[i]);
        }
        return GetStringAndRelease(mStringBuilder);
    }



    public static string Append(params string[] c)
    {
        int len = 0;
        for (int i = 0; i < c.Length; i++)
        {
            len += c[i].Length;
        }

        StringBuilder mStringBuilder = Acquire(len);
        for (int i = 0; i < c.Length; i++)
        {
            mStringBuilder.Append(c[i]);
        }     
        return GetStringAndRelease(mStringBuilder);
    }


    public static StringBuilder Acquire(int capacity = 0x100)
    {
        if (capacity <= MaxSize)
        {
            StringBuilder sb = StringBuilderCache.CachedInstance;
            if (sb != null && capacity <= sb.Capacity)
            {
                StringBuilderCache.CachedInstance = null;
                sb.Remove(0,sb.Length);
                return sb;
            }
        }
        return new StringBuilder(capacity);
    }

    private static void Release(StringBuilder sb)
    {
        if (sb == null)
            return;
        if (sb.Capacity <= MaxSize)
        {
            StringBuilderCache.CachedInstance = sb;
        }
    }

    public static string GetStringAndRelease(StringBuilder sb)
    {
        string result = sb.ToString();
        Release(sb);
        
        return result;
    }

    public static void Append(this StringBuilder sb, string str = "")
    {
        sb.Append(str + "\r\n");
    }

    public static void ClearSelf(this StringBuilder sb)
    {
        sb.Length = 0;
    }

}
