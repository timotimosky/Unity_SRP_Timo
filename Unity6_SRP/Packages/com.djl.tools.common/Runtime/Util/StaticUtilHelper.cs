using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StaticUtilHelper
{
    /// <summary>
    /// 判断输入的字符串是否是合法的IPV6 地址
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsIPV6(string input)
    {
        string pattern = "";
        string temp = input;
        string[] strs = temp.Split(':');
        if (strs.Length > 8)
        {
            return false;
        }
        int count = GetStringCount(input, "::");
        if (count > 1)
        {
            return false;
        }
        else if (count == 0)
        {
            pattern = @"^([\da-f]{1,4}:){7}[\da-f]{1,4}$";
            return IsMatch(pattern, input);
        }
        else
        {
            pattern = @"^([\da-f]{1,4}:){0,5}::([\da-f]{1,4}:){0,5}[\da-f]{1,4}$";
            return IsMatch(pattern, input);
        }
    }

    /// <summary>
    /// 计算字符串的字符长度，一个汉字字符将被计算为两个字符
    /// </summary>
    /// <param name="input">需要计算的字符串</param>
    /// <returns>返回字符串的长度</returns>
    public static int GetByteLength(this string input)
    {
        return Regex.Replace(input, @"[\u4e00-\u9fa5/g]", "aa").Length;
    }

    /// <summary>
    /// 调用Regex中IsMatch函数实现一般的正则表达式匹配
    /// </summary>
    /// <param name="pattern">要匹配的正则表达式模式。</param>
    /// <param name="input">要搜索匹配项的字符串</param>
    /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
    public static bool IsMatch(string pattern, string input)
    {
        if (input == null || input == "") return false;
        Regex regex = new Regex(pattern);
        return regex.IsMatch(input);
    }

/* *******************************************************************
 * 1、通过“:”来分割字符串看得到的字符串数组长度是否小于等于8
 * 2、判断输入的IPV6字符串中是否有“::”。
 * 3、如果没有“::”采用 ^([\da-f]{1,4}:){7}[\da-f]{1,4}$ 来判断
 * 4、如果有“::” ，判断"::"是否止出现一次
 * 5、如果出现一次以上 返回false
 * 6、^([\da-f]{1,4}:){0,5}::([\da-f]{1,4}:){0,5}[\da-f]{1,4}$
 * ******************************************************************/
    /// <summary>
    /// 判断字符串compare 在 input字符串中出现的次数
    /// </summary>
    /// <param name="input">源字符串</param>
    /// <param name="compare">用于比较的字符串</param>
    /// <returns>字符串compare 在 input字符串中出现的次数</returns>
    private static int GetStringCount(string input, string compare)
    {
        int index = input.IndexOf(compare);
        if (index != -1)
        {
            return 1 + GetStringCount(input.Substring(index + compare.Length), compare);
        }
        else
        {
            return 0;
        }

    }

    /// <summary>
    /// 判断两个日期是否在同一周
    /// </summary>
    /// <returns><c>true</c> if is same week the specified dtmS dtmE; otherwise, <c>false</c>.</returns>
    /// <param name="dtmS">开始日期.</param>
    /// <param name="dtmE">结束日期.</param>
    public static bool IsSameWeek(DateTime dtmS, DateTime dtmE)
    {
        TimeSpan ts = dtmE - dtmS;

        double db1 = ts.TotalDays;

        int intDow = Convert.ToInt32(dtmE.DayOfWeek);

        if (intDow == 0)
        {

            intDow = 7;
        }

        if (db1 >= 7 || db1 >= intDow)
        {

            return false;
        }
        else
        {

            return true;
        }
    }

    /// <summary>
    /// 判断两个日期是否在同一月
    /// </summary>
    /// <returns><c>true</c> if is same week the specified dtmS dtmE; otherwise, <c>false</c>.</returns>
    /// <param name="dtmS">开始日期.</param>
    /// <param name="dtmE">结束日期.</param>
    public static bool IsSameMonth(DateTime dtmS, DateTime dtmE)
    {
        return dtmS.Year.Equals(dtmE.Year) && dtmS.Month.Equals(dtmE.Month);
    }

    /// <summary>
    /// 在由正则表达式模式定义的位置拆分输入字符串。
    /// </summary>
    /// <param name="pattern">模式字符串</param>
    /// <param name="input">输入字符串</param>
    /// <returns></returns>
    public static string[] Split(string pattern, string input)
    {
        Regex regex = new Regex(pattern);
        return regex.Split(input);
    }

    /// <summary>
    /// 判断输入的字符串是否是一个合法的手机号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsMobilePhone(string input)
    {
        return IsMatch(@"^13\\d{9}$", input);
    }

    /// <summary>
    /// 匹配3位或4位区号的电话号码，其中区号可以用小括号括起来，
    /// 也可以不用，区号与本地号间可以用连字号或空格间隔，
    /// 也可以没有间隔
    /// \(0\d{2}\)[- ]?\d{8}|0\d{2}[- ]?\d{8}|\(0\d{3}\)[- ]?\d{7}|0\d{3}[- ]?\d{7}
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsPhone(string input)
    {
        string pattern = "^\\(0\\d{2}\\)[- ]?\\d{8}$|^0\\d{2}[- ]?\\d{8}$|^\\(0\\d{3}\\)[- ]?\\d{7}$|^0\\d{3}[- ]?\\d{7}$";
        return IsMatch(pattern, input);
    }

    /// <summary>
    /// 判断输入的字符串只包含汉字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsChineseCh(string input)
    {
        return IsMatch(@"^[\u4e00-\u9fa5]+$", input);
    }

    /// <summary>
    /// 从输入字符串中的第一个字符开始，用替换字符串替换指定的正则表达式模式的所有匹配项。
    /// </summary>
    /// <param name="pattern">模式字符串</param>
    /// <param name="input">输入字符串</param>
    /// <param name="replacement">用于替换的字符串</param>
    /// <returns>返回被替换后的结果</returns>
    public static string Replace(string pattern, string input, string replacement)
    {
        Regex regex = new Regex(pattern);
        return regex.Replace(input, replacement);
    }

    /// <summary>
    /// 判断两个日期是否在同一天
    /// </summary>
    /// <returns><c>true</c> if is same week the specified dtmS dtmE; otherwise, <c>false</c>.</returns>
    /// <param name="dtmS">开始日期.</param>
    /// <param name="dtmE">结束日期.</param>
    public static bool IsSameDay(DateTime dtmS, DateTime dtmE)
    {
        return dtmS.Date.Equals(dtmE.Date);
    }

    /// <summary>
    /// 将字符串转换为数组.
    /// </summary>
    /// <returns>The array.</returns>
    /// <param name="str">String.</param>
    /// <param name="theSeparator">分隔符.</param>
    public static string[] ToArray(this string str, char theSeparator)
    {
        if (string.IsNullOrEmpty(str))
            return new string[] { };

        return str.Split(theSeparator);
    }
}
