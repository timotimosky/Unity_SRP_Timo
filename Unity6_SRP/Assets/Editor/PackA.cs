using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public class PackA : Editor
{
    [MenuItem("AssetBundle/Package (Default)")]
    private static void PackageBuddle()
    {
        Debug.Log("Packaging AssetBundle...");
        string packagePath = UnityEditor.EditorUtility.OpenFolderPanel("Select Package Path", "F:/", "");
        if (packagePath.Length <= 0 || !Directory.Exists(packagePath))
            return;
        Debug.Log("Output Path: " + packagePath);
        BuildPipeline.BuildAssetBundles(packagePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
       // AssetDatabase.Refresh();
    }

    static string AssetBundlesOutputPath = @"Assets/AssetBundles2";
    [MenuItem("AssetBundle/Package (Default)2222")]
    public static void FragmentBuild()
    {
        if (!Directory.Exists(AssetBundlesOutputPath))
        {
            Directory.CreateDirectory(AssetBundlesOutputPath);
        }

        BuildPipeline.BuildAssetBundles(AssetBundlesOutputPath,
            GetAssetBundleBuild(Selection.gameObjects), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

    }

    public static AssetBundleBuild[] GetAssetBundleBuild(GameObject[] objs)
    {
        List<string> all = GetAllBuildGamobj(objs);

        AssetBundleBuild[] builds = new AssetBundleBuild[all.Count];
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].EndsWith(".mat"))
                continue;
            AssetBundleBuild build = new AssetBundleBuild();
            string[] str = all[i].Split('/');
            string name = str[str.Length - 1];
            string[] str1 = name.Split('.');
            string result = str1[0];
            build.assetBundleName = result;
            build.assetNames = new string[] { all[i] };
            build.assetBundleVariant = "ab";
            builds[i] = build;
        }
        return builds;
    }
    static List<string> GetAllBuildGamobj(GameObject[] objs)
    {
        List<string> all = new List<string>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (!Check(all, AssetDatabase.GetAssetPath(objs[i])))
                all.Add(AssetDatabase.GetAssetPath(objs[i]));
            string[] depend = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(objs[i]));
            for (int j = 0; j < depend.Length; j++)
            {
                if (!Check(all, depend[j]))
                    all.Add(depend[j]);
            }
        }
        return all;
    }
    private static bool Check(List<string> list, string target)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == target)
            {
                return true;
            }
        }
        return false;
    }
}
