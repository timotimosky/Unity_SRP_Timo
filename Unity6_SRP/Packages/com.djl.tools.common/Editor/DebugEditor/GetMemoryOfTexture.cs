using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class GetMemoryOfTexture :  EditorWindow
{
    //内存
    //使用Profiler可以查看某个资源的内存占用情况，但是必须启动游戏，并且待查看的资源已经载入游戏中。我希望的是不启动游戏，也能看到它的内存好做统计。

    //硬盘
    //由于unity中的资源压缩格式记录在meta中所以，在文件夹中看到的资源大小是不正确的。打开unity需要选择一个资源，
    //比如Texture、然后在右侧Inspector面板最下面可以看见它真实的硬盘占用。这个数据我也希望那个可以脚本取到，这样我好做统计工具。
    //在Project视图中先选择一个Texture 然后点击menuitem(“1/1”)即可


    public static float Texture_Memory(Texture texture)
    {
        if (texture == null)
        {
            return 0;
        }

       // System.Type type = System.Reflection.Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
        Type type = typeof(Editor).Assembly.GetType("UnityEditor.TextureUtil");

        long size= Profiler.GetRuntimeMemorySizeLong(Selection.activeObject);
        UnityEngine.Debug.Log("Texture 内存占用：" + EditorUtility.FormatBytes(size));

        MethodInfo methodInfo = type.GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
        long  fileSize = (long)methodInfo.Invoke(null, new object[] { texture });

        UnityEngine.Debug.Log("Texture 硬盘占用：" + EditorUtility.FormatBytes(fileSize));

        return size/1024.0f;
    }

    [MenuItem("自动检测工具/GetMemory")]
    static float GetVerctorNum()
    {
        if (Selection.activeObject == null)
        {
            Debug.Log("没选择资源，无法获取硬盘和内存大小");
            return 0;
        }

        UnityEngine.Debug.Log("整个预制体 内存占用：" + EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(Selection.activeObject)));

        UnityEngine.Object[] selectedAsset = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        float memory = 0;
        for (int i = 0; i < selectedAsset.Length; i++)
        {
            UnityEngine.Object obj = selectedAsset[i];
            if (obj is Mesh objmesh)
            {
                memory += Mesh_Memory(objmesh);
            }
            else if (obj is GameObject gameobj)
            {
                memory += GameObject_Mesh_Memory(gameobj);
            }
            else if (obj is Texture texture)
                memory += Texture_Memory(texture);
            else if (obj is Sprite sprite)
            {
                memory += Texture_Memory(sprite.texture);
            }
        }
        return memory;
    }


    static float Mesh_Memory(Mesh objmesh)
    {
        if (objmesh != null)
        {
            int vert2 = objmesh.vertexCount;
            float rom2 = (vert2) / 9f;
            Debug.Log(objmesh.name + "的Mesh内存占用为" + rom2 + "kb");
            return rom2; ;
        }
        return 0;
    }
    static float GameObject_Mesh_Memory(GameObject gameobj)
    {
        if (gameobj == null)
        {
            return 0;
        }

        MeshFilter[] filters = gameobj.GetComponentsInChildren<MeshFilter>();
        int vert = 0; //48byte
        uint index = 0;//2byte;
        for (int j = 0; j < filters.Length; j++)
        {
            MeshFilter f = filters[j];
            vert += f.sharedMesh.vertexCount;
            for (int k = 0; k < f.sharedMesh.subMeshCount; k++)
                index += f.sharedMesh.GetIndexCount(k);
        }

      //  float rom = (vert * 48 + index * 2) / 1024 / 1024;
        float rom = (vert) / 9.0f;
        Debug.Log(gameobj.name + "的Mesh内存占用为" + rom + "kb");
        return rom;
    }
}
