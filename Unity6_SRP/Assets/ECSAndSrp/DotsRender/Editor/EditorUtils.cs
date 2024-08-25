using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ECSFrame.ECSFrameBinary
{

    /// <summary>
    /// 编辑器下的通用方法
    /// </summary>
    public static class EditorUtils
    {

        #region 编辑器下使用

        public static bool DisplayDialog(string title, string message, string ok = "确定")
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialog(title, message, ok);
#else
            return false;
#endif
        }

        public static bool DisplayDialog(string title, string message, string ok, string cancel = "\"\"")
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
#else
            return false;
#endif
        }

        public static void DisplayProgressBar(string title, string info, float progress)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar($"{title}:{progress * 100:f2}%", info, progress);
#endif
        }

        public static void DisplayProgress(string title, string info, int index, int total)
        {
#if UNITY_EDITOR
            DisplayProgressBar(title, info, (float)index / total);
#endif
        }

        public static bool DisplayCancelableProgressBar(string title, string info, float progress)
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayCancelableProgressBar($"{title}:{progress * 100:f2}%", info, progress);
#else
            return false;
#endif
        }

        public static bool DisplayCancelableProgressBar(string title, string info, int index, int total)
        {
#if UNITY_EDITOR
            return DisplayCancelableProgressBar(title, info, (float)index / total);
#else
            return false;
#endif
        }

        public static void ClearProgressBar()
        {
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// 重命名；
        /// </summary>
        public static void RenameAsset(UnityEngine.Object target, string newName)
        {
            if (target == null)
                return;

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(target);
            AssetDatabase.RenameAsset(path, newName);
#endif
        }

        /// <summary>
        /// 获取某个资源的大小（KB）
        /// </summary>
        public static int GetAssetSize(UnityEngine.Object target)
        {
            if (target == null)
                return 0;

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(target);
            FileInfo file = new FileInfo(path);
            return Mathf.CeilToInt(file.Length / 1024.0f);//获取文件大小；
#else
            return 0;
#endif
        }

        /// <summary>
        /// 获取GUID； 
        /// </summary>
        public static string GetGuid(this Object target)
        {
            if (target == null)
                return null;
            string ret = "";
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(target);
            ret = AssetDatabase.AssetPathToGUID(path);
#endif
            return ret;
        }

        /// <summary>
        /// 获取目标文件夹下所有的预制；
        /// 会自行搜索子文件夹。
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static List<GameObject> GetPrefabListAtPath(string Path) { return GetObjectListAtPath<GameObject>(Path, "prefab"); }

        /// <summary>
        /// 获取目标文件夹下的所有特定类型的资源；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Path">路径</param>
        /// <param name="typeStr">资源类型名</param>
        /// <returns></returns>
        public static List<T> GetObjectListAtPath<T>(string Path, string typeStr = "asset") where T : Object
        {
            List<T> L = new List<T>();
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"t:{typeStr}", new string[] { Path });
            int length = guids.Length;
            for (int i = 0; i < length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                L.Add(asset);
            }
#endif
            return L;
        }


        public static T FindResource<T>(string tName) where T : Object
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(tName);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in objs)
                {
                    var res = obj as T;
                    if (res != null && res.name == tName)
                    {
                        return res;
                    }
                }
            }
#endif
            return null;
        }

        public static T CreateAsset<T>(T obj, string newPath) where T : Object
        {
#if UNITY_EDITOR
            if (obj == null)
                return null;
            var newObj = Object.Instantiate(obj);
            AssetDatabase.CreateAsset(newObj, newPath);
            return newObj;
#else
            return null;
#endif
        }

        /// <summary>
        /// 同时创建大批量文件；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public static T[] CreateAssets<T>(T[] objs, string[] newPath) where T : Object
        {
#if UNITY_EDITOR
            if (objs == null)
                return null;
            int length = objs.Length;
            T[] result = new T[length];

            AssetDatabase.DisallowAutoRefresh();
            AssetDatabase.StartAssetEditing();

            try
            {
                for (int i = 0; i < length; i++)
                {
                    var obj = objs[i];
                    var path = newPath[i];
                    result[i] = CreateAsset(obj, path);
                }
            }
            catch (System.Exception ex)
            {
                DebugTool.LogError(ex);
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.AllowAutoRefresh();

            return result;
#else
            return null;
#endif
        }

        public static void SaveAndRefresh()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public static void FindAllMainResource(string tName, List<Object> result)
        {
            result?.Clear();
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(tName);

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadMainAssetAtPath(path);

                if (obj is DefaultAsset)//这个一般是文件夹
                    continue;

                if (obj is MonoScript)//脚本重名会编译错误，这里不需要管；
                    continue;

                if (obj.GetFileName() != tName)
                    continue;//这个搜索是包含逻辑，而不是全字匹配；

                result?.Add(obj);
            }
#endif
        }

        public static string GetFileName(this Object obj)
        {
            if (obj == null)
                return null;

            //材质球的名字和文件名可能会有不同；
            bool isShader = obj is Shader;
            if (!isShader)
                return obj.name;

#if UNITY_EDITOR
            string filePath = AssetDatabase.GetAssetPath(obj);
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                DebugTool.LogError($"路径获取失败[{obj.GetType()}]：{filePath}");
                return obj.name;
            }

            string fileName = file.Name;
            int dot = fileName.IndexOf('.');
            string InFolderName = fileName.Substring(0, dot);
            if (InFolderName != obj.name && !isShader)
                DebugTool.LogWarning($"[{obj}]{obj.GetType()} 名字不相同！");
            return InFolderName;
#else
            return obj.name;
#endif
        }

        /// <summary>
        /// 查找在某个文件夹下的所有类型资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder">工程中文件夹相对路径</param>
        /// <param name="result">返回搜索的结果</param>
        /// <param name="fliterName">筛选名字</param>
        public static void FindAssetInFolder<T>(string folder, List<T> result, string fliterName = null, bool showProgress = false) where T : Object
        {
            if (result == null)
                result = new List<T>();
            result.Clear();

#if UNITY_EDITOR

            //先找到所有资源路径
            List<string> tPathes = new List<string>();
            GetAllAssetPath(folder, tPathes, fliterName, showProgress);

            int length = tPathes.Count;
            for (int i = 0; i < length; i++)
            {
                var path = tPathes[i];
                if (showProgress)
                    EditorUtility.DisplayProgressBar("提取主资源..", $"[{i}/{length}]：{path}", (float)i / length);

                //加载主资源
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    result.Add(asset);
            }

            if (showProgress)
                EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// 查找在某个文件夹下的所有主资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder">工程中文件夹相对路径</param>
        /// <param name="result">返回搜索的结果</param>
        /// <param name="fliterName">筛选名字</param>
        public static void FindMainAssetInFolder(string folder, List<Object> result, string fliterName = null, bool showProgress = false)
        {
            if (result == null)
                result = new List<Object>();
            result.Clear();

#if UNITY_EDITOR

            //先找到所有资源路径
            List<string> tPathes = new List<string>();
            GetAllAssetPath(folder, tPathes, fliterName, showProgress);

            int length = tPathes.Count;
            for (int i = 0; i < length; i++)
            {
                var path = tPathes[i];
                if (showProgress)
                    EditorUtility.DisplayProgressBar("提取主资源..", $"[{i}/{length}]：{path}", (float)i / length);

                //加载主资源
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset != null)
                    result.Add(asset);
            }

            if (showProgress)
                EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// 获取此文件夹下所有Unity资源的相对路径；
        /// </summary>
        public static void GetAllAssetPath(string folder, List<string> filePaths, string fliterName = null, bool showProgress = false)
        {
            if (filePaths == null)
                filePaths = new List<string>();
            filePaths.Clear();

#if UNITY_EDITOR
            //定位到指定文件夹
            if (!Directory.Exists(folder))
                return;
            var directory = new DirectoryInfo(folder);
            var curDirectoryPath = Directory.GetCurrentDirectory();
            int subLength = curDirectoryPath.Length + 1;//去除头和反斜杠

            //查询该文件夹下的所有文件；
            var files = directory.GetFiles("*", SearchOption.AllDirectories);
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                var file = files[i];

                //跳过Unity的meta文件（后缀名为.meta）
                if (file.Extension.Contains("meta"))
                    continue;

                if (fliterName != null)
                {
                    //是否开启名字筛选
                    if (!file.Name.Contains(fliterName))
                        continue;
                }
                //根据路径直接拼出对应的文件的相对路径
                string path = file.FullName.Substring(subLength);
                if (showProgress)
                    DisplayProgress("搜索资源..", $"[{i}/{length}]：{path}", i, length);
                filePaths.Add(path);
            }

            if (showProgress)
                ClearProgressBar();
#endif
        }

        /// <summary>
        /// 查找有相同名字的资源；
        /// </summary>
        /// <param name="res"></param>
        /// <param name="sameNameList">返回相同名字的资源列表（排除自己）</param>
        public static void FindSameNameAsset(Object res, List<Object> sameNameList)
        {
            sameNameList?.Clear();
            if (res == null)
                return;

#if UNITY_EDITOR

            //查找所有同名文件
            string resName = res.GetFileName();
            FindAllMainResource(resName, sameNameList);
            sameNameList.Remove(res);

            //去除自身的资源，只能通过路径比对，否则GameObject类型不能正确去除；
            var selfPath = AssetDatabase.GetAssetPath(res);
            int length = sameNameList.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                var temp = sameNameList[i];
                var path = AssetDatabase.GetAssetPath(temp);
                if (selfPath != path)
                    continue;

                sameNameList.RemoveAt(i);
            }
#endif
        }

        /// <summary>
        /// 获取资源依赖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="L"></param>
        /// <param name="result"></param>
        public static void GetDependencies<T>(List<T> L, List<Object> result) where T : Object
        {
            result.Clear();
#if UNITY_EDITOR
            int length = L.Count;
            for (int i = 0; i < length; i++)
            {
                var obj = L[i];
                var path = AssetDatabase.GetAssetPath(obj);
                var arr = AssetDatabase.GetDependencies(path);//返回路径
                for (int j = 0; j < arr.Length; j++)
                {
                    var dPath = arr[j];
                    //在Packages包里的不管；
                    if (dPath.StartsWith("Packages/"))
                        continue;

                    var dAsset = AssetDatabase.LoadMainAssetAtPath(dPath);
                    //排除自己；
                    if (dAsset == obj)
                        continue;

                    if (!result.Contains(dAsset))
                        result.Add(dAsset);
                }
            }
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// 获取当前的打包平台
        /// </summary>
        /// <returns></returns>
        public static BuildTargetGroup GetCurrentBuildTargetGroup()
        {
            var curTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(curTarget);
            return buildTargetGroup;
        }

        public static NamedBuildTarget GetCurrentNamedBuildTarget()
        {
            BuildTargetGroup buildTargetGroup = GetCurrentBuildTargetGroup();
            return NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        }

        public static void AddScriptingDefineSymbol(NamedBuildTarget buildTarget, string tSymbol)
        {
            string[] symbols;
            if (ContainsScriptingDefineSymbol(buildTarget, tSymbol, out symbols))
                return;

            List<string> L = new List<string>(symbols);
            L.Add(tSymbol);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, L.ToArray());
        }

        public static bool ContainsScriptingDefineSymbol(NamedBuildTarget buildTarget, string tSymbol, out string[] symbols)
        {
            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out symbols);

            foreach (var item in symbols)
            {
                if (item == tSymbol)
                    return true;
            }
            return false;
        }

        public static void RemoveScriptingDefineSymbol(NamedBuildTarget buildTarget, string tSymbol)
        {
            string[] symbols;
            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out symbols);
            List<string> L = new List<string>(symbols);
            L.RemoveAll(x => x == tSymbol);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, L.ToArray());
        }
#endif

        #endregion;

        #region 文件 IO 相关，非编辑器下可能不好使

        /// <summary>
        /// 删除文件夹下下的所有内容
        /// </summary>
        /// <param name="directory"></param>
        public static void ClearAll(this DirectoryInfo directory)
        {
            var childs = directory.GetDirectories();
            foreach (var child in childs)
            {
                child.ClearAll();
                child.Delete();
            }

            //所有文件
            var allFiles = directory.GetFiles();
            foreach (var file in allFiles)
                file.Delete();
        }

        /// <summary>
        /// 删除目标文件夹下的所有文件
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="containsName">筛选文件</param>
        public static void ClearFolderAllAsset(string folderPath, string containsName = null)
        {
            if (!Directory.Exists(folderPath))
                return;
            var files = Directory.GetFiles(folderPath);
            for (int i = 0; i < files.Length; i++)
            {
                if (containsName == null || files[i].Contains(containsName))
                    File.Delete(files[i]);
            }
        }

        public static void WritePNGFile(string path, Texture2D texture)
        {
            byte[] datas = texture.EncodeToPNG();
            File.WriteAllBytes(path, datas);
        }

        /// <summary>
        /// Unity 路径转成绝对路径
        /// </summary>
        /// <param name="path"></param>
        public static string GetFullName(string path)
        {
            string str = Application.dataPath;
            str = str.Substring(0, str.Length - 7);
            return $"{str}/{path}";
        }

        public static DirectoryInfo CreateFolder(string realPath)
        {
            if (Directory.Exists(realPath))
                return new DirectoryInfo(realPath);
            return Directory.CreateDirectory(realPath);
        }

        /// <summary>
        /// 删除文件夹下的所有文件，但是不删除文件夹
        /// </summary>
        public static void ClearAllFiles(string targetFolder, string searchPattern = "*")
        {
            DirectoryInfo root = new DirectoryInfo(targetFolder);
            if (!root.Exists)
            {
                Debug.LogError($"文件夹：{targetFolder} 不存在！");
                return;
            }

            //所有文件夹；
            var allDirectory = new List<DirectoryInfo>(root.GetDirectories(searchPattern, SearchOption.AllDirectories));
            //所有文件
            var allFiles = root.GetFiles(searchPattern, SearchOption.AllDirectories);

            int length = allFiles.Length;
            for (int i = 0; i < length; i++)
            {
                var file = allFiles[i];
                DisplayProgress($"删除文件 ...", file.Name, i, length);
                string fullName = file.FullName;
                //unity 里（新版 > 2022），meta 标记文件夹是否存在，因此不能删除；
                if (file.Extension.Contains("meta"))
                {
                    //跳过文件夹的meta文件；
                    var folder = allDirectory.Find(x => fullName.Contains(x.FullName));
                    if (folder != null)
                        continue;
                }

                //删除文件；
                File.Delete(fullName);
            }

            ClearProgressBar();
        }

        /// <summary>
        /// 拷贝、或者替换文件；
        /// </summary>
        public static void CopyAndReplace(string sourceFileName, string destFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                Debug.LogError($"原始文件不存在：{sourceFileName}");
                return;
            }

            //已经有此文件，则删除
            if (File.Exists(destFileName))
                File.Delete(destFileName);

            //拷贝文件
            File.Copy(sourceFileName, destFileName);
        }

        /// <summary>
        /// 拷贝，或者跳过文件
        /// </summary>
        public static void CopyOrIngore(string sourceFileName, string destFileName)
        {
            //已经有此文件，跳过
            if (File.Exists(destFileName))
                return;

            if (!File.Exists(sourceFileName))
            {
                Debug.LogError($"原始文件不存在：{sourceFileName}");
                return;
            }

            //拷贝文件
            File.Copy(sourceFileName, destFileName);
        }

        /// <summary>
        /// 同步文件夹目录结构；
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void SyncFolderTo(this DirectoryInfo from, DirectoryInfo to)
        {
            if (from == null || to == null)
                return;

            string targetRoot = to.FullName;
            //获取自身所有的子目录(根节点)；
            var subs = from.GetDirectories();
            foreach (var item in subs)
            {
                string folderName = item.Name;
                //在目标文件夹下面同步创建；
                string targetPath = $"{targetRoot}/{folderName}";
                var tFolder = CreateFolder(targetPath);
                //递归，子目录都创建；
                item.SyncFolderTo(tFolder);
            }
        }


        public static void CopyAllFilesTo(this DirectoryInfo from, DirectoryInfo to, System.Action<string> OnMoveOneFile = null)
        {
            if (!to.Exists)
            {
                Debug.LogError($"目标文件夹：{to} 不存在！");
                return;
            }
            string folderPath = to.FullName;

            var files = from.GetFiles();
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                var file = files[i];
                DisplayProgress("拷贝文件...", file.Name, i, length);

                //复制此文件夹下的文件；
                string source = file.FullName;
                string name = file.Name;
                string tPath = $"{folderPath}/{name}";
                ForceCopy(source, tPath);
                OnMoveOneFile?.Invoke(name);
            }

            //之后递归拷贝文件夹；
            var subs = from.GetDirectories();
            length = subs.Length;
            for (int i = 0; i < length; i++)
            {
                var sub = subs[i];
                string folderName = sub.Name;
                string targetPath = $"{folderPath}/{folderName}";
                sub.CopyAllFilesTo(new DirectoryInfo(targetPath), OnMoveOneFile);
            }

            ClearProgressBar();
        }

        /// <summary>
        /// 强制拷贝，如果已经有此文件，就会替换
        /// </summary>
        public static void ForceCopy(string sourceFileName, string destFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                Debug.LogError($"原始文件不存在：{sourceFileName}");
                return;
            }

            //已经有此文件，则删除
            if (File.Exists(destFileName))
                File.Delete(destFileName);

            //拷贝文件
            File.Copy(sourceFileName, destFileName);
        }

        /// <summary>
        /// 拷贝Unity资源，会连带meta文件一并导出；
        /// </summary>
        /// <param name="o"></param>
        /// <param name="dir">目标文件夹</param>
        /// <param name="fileName">为空表示保留原文件名</param>
        public static void CopyTo(this Object o, DirectoryInfo dir, string fileName = null)
        {
            if (o == null || dir == null || !dir.Exists)
                return;

#if UNITY_EDITOR
            //获取文件路径；
            string path = AssetDatabase.GetAssetPath(o);
            FileInfo file = new FileInfo(path);
            string folder = file.Directory.FullName.Replace('\\', '/');
            fileName = string.IsNullOrEmpty(fileName) ? file.Name : fileName;
            //拷贝文件
            ForceCopy(file.FullName, $"{dir.FullName}/{fileName}");
            //拷贝meta；
            ForceCopy($"{folder}/{file.Name}.meta", $"{dir.FullName}/{fileName}.meta");
#endif
        }

        /// <summary>
        /// 拷贝Unity资源，会连带meta文件一并导出；
        /// </summary>
        /// <param name="o"></param>
        /// <param name="dir">目标文件夹</param>
        /// <param name="fileName">为空表示保留原文件名</param>
        public static void CopyTo(this Object o, string dirPath, string fileName = null)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            o.CopyTo(dir, fileName);
        }

        public static void RunBat(string filePath, bool showWindow = false)
        {
            if (!File.Exists(filePath))
            {
                DebugTool.LogError($"目标路径不存在：{filePath}");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = filePath; // .bat文件的完整路径
            startInfo.Arguments = ""; // 如果.bat文件需要参数，可以在这里添加
            startInfo.UseShellExecute = false; // 必须为false以便能够重定向I/O流
            startInfo.CreateNoWindow = !showWindow; // 是否显示命令提示符窗口（可选）

            // 如果希望看到.bat文件的输出
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            startInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            startInfo.WindowStyle = ProcessWindowStyle.Normal; // 或Hidden来隐藏窗口

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, args) => DebugTool.Log(args.Data);
                process.ErrorDataReceived += (sender, args) => DebugTool.LogError(args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 如果需要等待.bat文件执行完成再继续
                process.WaitForExit();
            }
        }

        #endregion
    }
}