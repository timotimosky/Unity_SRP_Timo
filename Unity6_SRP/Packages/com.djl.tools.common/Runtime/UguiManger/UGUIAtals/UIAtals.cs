using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
/********************************
 * Author：	    djl
 * Date：       2016//
 * Version：	V 0.1.0	
 * 
 *******************************/

public class UIAtals : ScriptableObject
{

    public Texture2D mainText;

    public List<Sprite> spriteLists = new List<Sprite>();

    /// <summary>
    /// 根据图片名称获取sprite
    /// </summary>
    /// <param name="spritename">图片名称</param>
    /// <returns></returns>
    public Sprite GetSprite(string spritename)
    {
        return spriteLists.Find((Sprite s) => { return s.name == spritename; });
    }

    /// <summary>
    /// 设置Image的Sprite
    /// </summary>
    /// <param name="im">Image</param>
    /// <param name="spritename">图片名称</param>
    public void SetSprite(ref Image im, string spritename)
    {

        if (im == null)
        {
            return;
        }
        Sprite sp = GetSprite(spritename);
        if (sp != null)
        {
            im.overrideSprite = sp;
        }
        else
        {
            Debug.Log("图集没有对应的图片:" + spritename);
        }
    }
}
