using System;
using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace Tools{
    public delegate void VoidCall();
    public delegate void VoidCall<T>(T t);
    public delegate void VoidCall<T1, T2>(T1 t1, T2 t2);
    /// <summary>
    /// 界面相关的通用辅助类接口;
    /// </summary>
    public class UIHelper
    {
        /// <summary>
        /// 将毫秒转换为具体年月日时分;
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetActivityTime(long time)
        {
            time = time / 1000;
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtResult = dtStart.AddSeconds(time);
            return dtResult.ToString("yyyy年MM月dd日HH:mm", CultureInfo.CreateSpecificCulture("zh-CN"));
        }

        /// <summary>
        /// 获取分隔字符串中第index位的转换值;Format：item#name#value...
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index">0开始</param>
        /// <returns></returns>
        static public string GetStrValue(string str, int index)
        {
            string[] strArray = str.Split('#');
            if (index < strArray.Length && index >= 0)
            {
                return strArray[index];
            }

            return "";
        }


        /// <summary>
        /// 获取分隔符#的分割个数;
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public int GetStrLength(string str)
        {
            string[] strArray = str.Split('#');
            return strArray.Length;
        }

        /// <summary>
        /// 将s转换成具体的天时分;
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetTimeNamgChange(long time)
        {
            int day = (int)(time / 86400);    //day;
            int hour = (int)((time % 86400) / 3600);     //hour;
            int minue = (int)((time % 86400 % 3600) / 60);      //min;
            string nowTime = "";
            if (day > 0)
            {
                nowTime = day + "天";
            }
            else if (hour > 0)
            {
                nowTime = hour + "时";
            }
            else if (minue > 0)
            {
                nowTime = minue + "分";
            }
            return nowTime;
        }

        /// <summary>
        /// 将秒转换为格式化字符串
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static string ConvertTimeStr(int sec) {
            return string.Format("{0:00}:{1:00}:{2:00}", sec / 3600, (sec % 3600) / 60, sec % 60);
        }

        /// <summary>
        /// 将秒转换为具体分秒
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static string ConvertTimeStr2(int sec)
        {
            return string.Format("{0:00}:{1:00}",(sec % 3600) / 60, sec % 60);
        }

        /// <summary>
        /// 获取电池电量
        /// </summary>
        /// <returns></returns>
        public static int GetBatteryLevel()
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
#elif UNITY_ANDROID
        try
        {
            string capacityString = System.IO.File.ReadAllText("/sys/class/power_supply/battery/capacity");
            return int.Parse(capacityString);
        }
        catch (Exception e)
        {
            Debugerr.LogJL("Failed to read battery power :" + e.Message);
        }
#endif
            return -1;
        }
     
    }
}
