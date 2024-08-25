using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
/********************************
 * Author：	    djl
 * Date：       2016//
 * Version：	V 0.1.0	
 * 
 *******************************/

public class DrawNote
{
    public static Dictionary<string, UIAtals> atlasmanage = new Dictionary<string, UIAtals>();


    /// <summary>
    /// 设置滑动条宽度
    /// </summary>
    /// <param name="obj">滑动条</param>
    /// <param name="posx">x坐标</param>
    /// <param name="posy">y坐标</param>
    public static void SetContent(GameObject obj, int posx, int posy)
    {
        obj.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(posx, posy);
    }

    /// <summary>
    /// 动态生成图片
    /// </summary>
    /// <param name="spitename">图片名称</param>
    /// <returns></returns>
    public static Image LoadImage(Sprite sprite, Transform parenttrs, int posx, int posy, float offsetX)
    {
        GameObject go = new GameObject(sprite.name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.parent = parenttrs;
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.rectTransform.pivot = new Vector2(0, 0);
        image.rectTransform.localPosition = new Vector3(posx + offsetX, posy, 0);
        image.SetNativeSize();
        return image;
    }

    /// <summary>
    /// 加载图集
    /// </summary>
    /// <param name="spriteName">图集名称</param>
    public static UIAtals LoadAtals(string atlasName)
    {
        UIAtals atlas = Resources.Load<UIAtals>(atlasName);
        atlasmanage.Add(atlasName, atlas);
        return atlas;
    }

    /// <summary>
    /// 生成note图片
    /// </summary>
    /// <param name="atlasName">Altas名称</param>
    /// <param name="spriteName">精灵名称</param>
    /// <param name="parenttrs">父节点</param>
    /// <param name="posx">x坐标</param>
    /// <param name="posy">y坐标</param>
    public static Image CreateImage(string atlasName, string spriteName, Transform parenttrs, string posx, string posy, float offsetX)
    {
        int iposx = int.Parse(posx);
        int iposy = int.Parse(posy);
        UIAtals atlas;
        if (!atlasmanage.ContainsKey(atlasName))
        {
            atlas = LoadAtals(atlasName);
        }
        else
        {
            atlas = atlasmanage[atlasName];
        }
        Image img = LoadImage(atlas.GetSprite(spriteName), parenttrs, iposx, iposy, offsetX);
        atlas.SetSprite(ref img, spriteName);
        img.raycastTarget = false;
        return img;
    }

    /// <summary>
    /// 小节线图片生成
    /// </summary>
    /// <param name="atlasName">图集名称</param>
    /// <param name="spriteName">图片名称</param>
    /// <param name="parenttrs">父节点</param>
    /// <param name="posx">坐标</param>
    /// <param name="offsetX">偏移量</param>
    /// <param name="wight">图片宽度</param>
    /// <param name="height">图片高度</param>
    /// <returns></returns>
    public static Image CreateMeasure(string atlasName, string spriteName, Transform parenttrs, string posx, float offsetX, float wight, float height)
    {
        int iposx = int.Parse(posx);
        UIAtals atlas;
        if (!atlasmanage.ContainsKey(atlasName))
        {
            atlas = LoadAtals(atlasName);
        }
        else
        {
            atlas = atlasmanage[atlasName];
        }
        Sprite sprite = atlas.GetSprite(spriteName);

        GameObject go = new GameObject(sprite.name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.parent = parenttrs;
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.rectTransform.pivot = new Vector2(0, 0);
        image.rectTransform.localPosition = new Vector3(iposx + offsetX, 0, 0);
        image.SetNativeSize();
        image.rectTransform.sizeDelta = new Vector2(wight, height);
        return image;
    }
}
