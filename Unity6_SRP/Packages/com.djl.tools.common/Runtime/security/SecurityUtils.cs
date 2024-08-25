using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#if  UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using GameDjlDLL;
#endif

/// <summary>
/// 加密工具集合
/// </summary>
/// 
namespace GameDjlDLL
{

    public class SecurityUtils
    {

#if UNITY_EDITOR
        [MenuItem("打包/Export Config MD5")]
        static void ExportCondigMD5()
        {
            string savePath = EditorUtility.SaveFolderPanel("请选择导出文件保存位置", "", "");
            if (savePath.Length != 0)
            {
                string dirpath = EditorUtility.SaveFolderPanel("请选择配置文件文件夹", "", "");
                if (dirpath.Length != 0)
                {
                    var dirpaths = Directory.GetFiles(dirpath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".dat") || s.EndsWith(".json"));
                    Dictionary<string, string> configversionDic = new Dictionary<string, string>();
                    foreach (string item in dirpaths)
                    {
                        string md5 = SecurityUtils.GetMD5HashFromFile(@item);
                        int startIndex = item.IndexOf("GameConfig");
                        int lastDotIdx = item.LastIndexOf('.');
                        string path;
                        path = item.Substring(startIndex + 11, lastDotIdx - startIndex - 11);
                        path = path.Replace("//", "/").Replace("\\", "/");
                        if (/*path.Contains("describe") ||*/ path.Contains("ConfigVersion") || path.Contains("Version"))
                            continue;
                        else
                            configversionDic.Add(path + ".json", md5);
                    }
                    //string info = JsonConvert.SerializeObject(configversionDic);
                   // FileHelp.WriteText(savePath, "ConfigVersion.json", info);
                }
            }
        }
#endif

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string GetMD5HashFromFile(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                //new FileStream(file, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }


        /// <summary>
        /// 生成MD5加密信息
        /// </summary>
        /// <param name="inputs">需要加密的字符串</param>
        /// <returns>十六进制格式的MD5字符串</returns>
        public static string MD5(params string[] inputs)
        {
            //合并字符串
            string input;
            if (inputs.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string s in inputs)
                {
                    sb.Append(s);
                }
                input = sb.ToString();
            }
            else
            {
                input = inputs[0];
            }
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return MD5(buffer);
        }

        public static string MD5(byte[] bytes)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] md5 = provider.ComputeHash(bytes);
            return BitConverter.ToString(md5).Replace("-", "");
        }

        /// <summary>
        /// 生成BPHash加密信息
        /// </summary>
        /// <param name="bytes">需要加密的信息</param>
        /// <param name="start">开始索引</param>
        /// <param name="end">结束索引</param>
        /// <returns>哈希值</returns>
        public static long BPHash(byte[] bytes, int start = 0, int end = 0)
        {
            if (end <= 0)
            {
                end = bytes.Length;
            }
            long hash = 0;
            for (int i = start; i < end; ++i)
            {
                hash = hash << 7 ^ (sbyte)bytes[i];
            }
            return hash;
        }

    }
}