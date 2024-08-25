using UnityEngine;
using System.Collections;
using UnityEditor;
/********************************
 * Author：	    djl
 * Date：       2016//
 * Version：	V 0.1.0	
 * 
 *******************************/

[CustomEditor(typeof(UIAtals))]
public class UIAtlasInspector : Editor
{
    public override void OnInspectorGUI()
    {
        UIAtals atlas = target as UIAtals;
        atlas.mainText = EditorGUILayout.ObjectField("MainTextture", atlas.mainText, typeof(Texture2D)) as Texture2D;

        if (GUILayout.Button("刷新数据"))
        {
            if (atlas.mainText == null)
            {
                string path = EditorUtility.OpenFilePanel("选择一张图集", Application.dataPath, "png");
                if (!string.IsNullOrEmpty(path))
                {
                    atlas.mainText = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                }
            }
            if (atlas.mainText != null)
            {
                string path = AssetDatabase.GetAssetPath(atlas.mainText);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType == TextureImporterType.Sprite
                    && importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(atlas.mainText));
                    atlas.spriteLists.Clear();
                    foreach (Object o in objs)
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
                }
                else
                {
                    atlas.mainText = null;
                }
            }
        }
        if (atlas.spriteLists.Count > 0)
        {
            foreach (Sprite s in atlas.spriteLists)
            {
                EditorGUILayout.ObjectField(s.name, s, typeof(Sprite));
            }
        }
    }
}
