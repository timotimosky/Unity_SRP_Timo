using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
/// <summary>
/// 资源下载的路径
/// </summary>
namespace UnityBook
{
    public class AssetsPathUtil
    {

        //读取本地流文件夹下的文件
        public static string FileReadPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return "file:///" + Application.streamingAssetsPath + "/";
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return "jar:file://" + Application.dataPath + "!/assets/";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return "file://" + Application.persistentDataPath + "/";
                    // Application.dataPath + "/Raw/";
                }
                else
                {
                    return "file:///" + Application.streamingAssetsPath + "/";
                }
            }
        }

        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                if (Application.isMobilePlatform || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return Application.persistentDataPath + "/lua/";
                }
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    return Application.dataPath + "/StreamingAssets/lua";
                }
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    int i = Application.dataPath.LastIndexOf('/');
                    return Application.dataPath.Substring(0, i + 1) + "/lua/";
                }
                return Application.persistentDataPath + "/lua/";
            }
        }


        //判断本地流文件夹下的文件是否存在
        public static string FileExixtPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return Application.streamingAssetsPath + "/";
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return Application.dataPath + "!/assets/";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return Application.persistentDataPath + "/";
                    //Application.dataPath + "/Raw/";
                }
                else
                {
                    return Application.dataPath + "/StreamingAssets/";
                }
            }
        }


        //读取沙盒(非ab)
        public static string ConfigPathForRead
        {
            get
            {
                return "file:///" + ConfigPathForWrite;
            }
        }


        //写入文件入沙盒 和删除沙盒文件，不需要file开头
        public static string ConfigPathForWrite
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return Application.persistentDataPath + "/";
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return Application.persistentDataPath + "/";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    //return Application.dataPath + "/Raw/";
                    return Application.persistentDataPath + "/";
                }
                else
                {
                    return Application.persistentDataPath + "/";
                }
            }
        }
        //读取沙盒
        public static string WwwPath
        {
            get
            {
                return ConfigPathForRead + PlatformUtil.GetRunTimePlatformFolder() + "/";
            }
        }

        public static string WwwPathNofile_compress
        {
            get
            {
                return loadABPath + "Compressed/";
            }
        }

        public static string WwwPathNofile_UnCompressed
        {
            get
            {
                return loadABPath + "UnCompressed/";
            }
        }


        //streamingAssets 这个目录在IOS下是可以同步读取的，但是在Android下必须用www来异步读取。
        //File.ReadAllText 或者 File.Exists Directory.Exists 这类操作的路径要jar:file://开头。 AssetBundle.LoadFromFile 这类路径不需要
        //写入文件入沙盒 和删除沙盒文件，不需要file开头
        //判断是否存在
        public static string loadABPath
        {
            get
            {
                return ConfigPathForWrite + PlatformUtil.GetRunTimePlatformFolder() + "/";
            }

        }

        public static string LuaPath
        {
            get
            {
                return "file:///" + LuaPathNofile;
            }
        }

        public static string LuaPathNofile
        {
            get
            {
                return ConfigPathForWrite + "/lua/";
            }
        }

    }
}
