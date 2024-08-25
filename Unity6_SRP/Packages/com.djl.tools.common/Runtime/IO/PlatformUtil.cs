using UnityEngine;

namespace UnityBook
{
    public class PlatformUtil
    {
        //运行时获取平台
        public static string GetRunTimePlatformFolder()
        {

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.WebGLPlayer:
                    return "webgl";
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "osx";
                default:
                    return "windows";
            }
        }



    }
}