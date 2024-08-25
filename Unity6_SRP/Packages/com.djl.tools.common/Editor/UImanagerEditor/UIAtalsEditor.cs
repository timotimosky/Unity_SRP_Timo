using UnityEngine;
using System.Collections;
using UnityEditor;
/********************************
 * Author：	    djl
 * Date：       2016//
 * Version：	V 0.1.0	
 * 
 *******************************/

public class UIAtalsEditor
{
    [MenuItem("Assets/创建当前选中图集")]
    public static void CreateAtlasPrefab()
    {
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                UIAtals atlas = ScriptableObject.CreateInstance<UIAtals>();
                object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
                atlas.spriteLists.Clear();
                foreach (object o in objs)
                {
                    if (o.GetType() == typeof(Texture2D))
                    {
                        atlas.mainText = o as Texture2D;
                    }
                    else if (o.GetType() == typeof(Sprite))
                    {
                        atlas.spriteLists.Add(o as Sprite);
                    }
                }
                AssetDatabase.CreateAsset(atlas, path.Replace(".png", "_Atlas.prefab"));
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log("当前选中的不是图集图片");
            }
        }
    }
}