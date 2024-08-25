using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DJLUtil
{
    public static class ChactText
    {
        /// <summary>
        /// 查找字符串中是否有需要的字符
        /// </summary>
        /// <param name="numberStr"> 原文 </param>
        /// <param name="list"> 需要查找的字符 </param>
        /// <returns>false 未找到，true 已找到</returns>
        public static bool FindChar(this string str)
        {
            char[] c = str.ToCharArray();
            for (int i = 0; i < c.Length; ++i)
            {
                //英文字母
                if (c[i] >= 0x00 && c[i] <= 0xff)
                {
                    continue;
                }

                //CJK标准文字    CJK= Chinese Japanese Korean
                else if (c[i] >= 0x4e00 && c[i] <= 0x9fa5)
                {
                    continue;
                }

                //全角ASCII、全角中英文标点、半宽片假名、半宽平假名、半宽韩文字母：FF00-FFEF
                else if (c[i] >= 0xff00 && c[i] <= 0xffef)
                {
                    continue;
                }
                //CJK部首补充
                else if (c[i] >= 0x2e80 && c[i] <= 0x2eff)
                {
                    continue;
                }

                //CJK标点符号
                else if (c[i] >= 0x3000 && c[i] <= 0x303f)
                {
                    continue;
                }
                //CJK笔画
                else if (c[i] >= 0x31c0 && c[i] <= 0x31ef)
                {
                    continue;
                }

                //康熙部首
                else if (c[i] >= 0x2f00 && c[i] <= 0x2fdf)
                {
                    continue;
                }

                //汉字结构描述字符
                else if (c[i] >= 0x2ff0 && c[i] <= 0x2fff)
                {
                    continue;
                }

                //注音符号
                else if (c[i] >= 0x3100 && c[i] <= 0x312f)
                {
                    continue;
                }

               //注音符号（闽南语、客家语扩展）
                else if (c[i] >= 0x31a0 && c[i] <= 0x31bf)
                {
                    continue;
                }

                //日文平假名
                else if (c[i] >= 0x3040 && c[i] <= 0x309f)
                {
                    continue;
                }

                //日文片假名
                else if (c[i] >= 0x30a0 && c[i] <= 0x30ff)
                {
                    continue;
                }

                //日文片假名拼音扩展
                else if (c[i] >= 0x31f0 && c[i] <= 0x31ff)
                {
                    continue;
                }

                //韩文拼音
                else if (c[i] >= 0xac00 && c[i] <= 0xd7af)
                {
                    continue;
                }

                //韩文字母
                else if (c[i] >= 0x1100 && c[i] <= 0x11ff)
                {
                    continue;
                }

                //韩文兼容字母
                else if (c[i] >= 0x3130 && c[i] <= 0x318f)
                {
                    continue;
                }

                //太玄经符号
                /*if(c>= 0x1d300 && c <= 0x1d35f){
                    return true;
                }*/

                //易经六十四卦象
                /*	if(c>= 0x4dc0 && c <= 0x4dff){
                        return true;
                    }*/

                //彝文音节
                else if (c[i] >= 0xa000 && c[i] <= 0xa48f)
                {
                    continue;
                }

                //彝文部首
                else if (c[i] >= 0xa490 && c[i] <= 0xa4cf)
                {
                    continue;
                }

                //盲文符号
                else if (c[i] >= 0x2800 && c[i] <= 0x28ff)
                {
                    continue;
                }

                //）CJK字母及月份
                else if (c[i] >= 0x3200 && c[i] <= 0x32ff)
                {
                    return true;
                }

                //CJK特殊符号（日期合并）
                else if (c[i] >= 0x3300 && c[i] <= 0x33ff)
                {
                    continue;
                }

               //装饰符号（非CJK专用）
                /*	if(c>= 0x2700 && c <= 0x27bf){
                        return true;
                    }*/

               //杂项符号（非CJK专用）
                /*if(c>= 0x2600 && c <= 0x26ff){
                    return true;
                }*/

               //中文竖排标点
                else if (c[i] >= 0xfe10 && c[i] <= 0xfe1f)
                {
                    continue;
                }

                //CJK兼容符号（竖排变体、下划线、顿号）
                else if (c[i] >= 0xfe30 && c[i] <= 0xfe4f)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

    }
}
