using System.IO;
using UnityEngine;

namespace FileIO
{
	public static class VersionControlUtility
	{
		// Perforce and other VCS have a lock mechanism that usually
		// only makes the file writable once checked out. However, this
		// is inconvenient for game development projects and Unity 
		// disregards this lock for all its generated files, so we do the same.
		// https://support.ludiq.io/communities/5/topics/1645-x
		private static bool UnlockFile(string path)
		{
			if (!File.Exists(path))
			{
				return false;
			}
			var info = new FileInfo(path);
			if (info.IsReadOnly)
			{
				//Debug.LogError("要保存的配置文件 Check out "+ path);
				info.IsReadOnly = false;
			}
            return !info.IsReadOnly;

        }

        public static bool UnlockAll(string path)
        {
            UnlockByP4V(path);
            UnlockFile(path);
            UnlockDir(path);
            return true;
        }


        private static bool UnlockByP4V(string path)
        {
#if UNITY_EDITOR
            if (!UnityEditor.VersionControl.Provider.enabled)
            {
                return false;
            }
            UnityEditor.VersionControl.Asset file = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
            if (file==null ||!file.isInCurrentProject)
            {
                return false;
            }
            if (UnityEditor.VersionControl.Provider.onlineState != UnityEditor.VersionControl.OnlineState.Online)
            {
                return false;
            }
            UnityEditor.VersionControl.Task mTask = UnityEditor.VersionControl.Provider.Checkout(file, UnityEditor.VersionControl.CheckoutMode.Asset);
            mTask.Wait();
            if (!mTask.success)
            {
                return false;
            }
#endif
            return true;
        }


        public static bool CheckIsReadOnly(string sourcePath)
        {
            System.IO.DirectoryInfo DirInfo = new DirectoryInfo(sourcePath);

            DirInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
            if (DirInfo == null || (!DirInfo.Exists))
            {
                return false;
            }
            if (System.IO.File.GetAttributes(sourcePath).ToString().IndexOf("ReadOnly") != -1)
            {
                Debug.Log(sourcePath + "是只读----并尝试自动check out");
                File.SetAttributes(sourcePath, FileAttributes.Normal);

            }

            if (System.IO.File.GetAttributes(sourcePath).ToString().IndexOf("ReadOnly") != -1)
            {
                Debug.Log(sourcePath + "是只读,自动check out失败，请手动check out ");
                return true;
            }
            else
            {
                Debug.Log(sourcePath + "不是只读");
                return false;
            }
        }


        private static void UnlockDir(string sourcePath)
        {
            System.IO.DirectoryInfo DirInfo = new DirectoryInfo(sourcePath);
            if (DirInfo == null || (!DirInfo.Exists))
            {
                return;
            }
            DirInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
		}

        public static void DeleteDirs(DirectoryInfo dirs)
        {
            if (dirs == null || (!dirs.Exists))
            {
                return;
            }

            DirectoryInfo[] subDir = dirs.GetDirectories();
            if (subDir != null)
            {
                for (int i = 0; i < subDir.Length; i++)
                {
                    if (subDir[i] != null)
                    {
                        DeleteDirs(subDir[i]);
                    }
                }
                subDir = null;
            }

            FileInfo[] files = dirs.GetFiles();
            if (files != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i] != null)
                    {
                        //Debug.Log("删除文件:" + files[i].FullName + "__over");
                        UnlockFile(files[i].FullName);
                        files[i].Delete();
                        files[i] = null;
                    }
                }
                files = null;
            }

            // Debug.Log("删除文件夹:" + dirs.FullName + "__over");
            dirs.Delete();
        }

    }
}