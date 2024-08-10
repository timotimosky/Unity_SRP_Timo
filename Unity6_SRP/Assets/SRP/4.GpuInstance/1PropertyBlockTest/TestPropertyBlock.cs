using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
//材质块 可以提高刷新缓存区的速度
//使用材质块更新，效率是10倍以上
//同材质下，可以不破坏动态批处理

//单纯使用材质块 不可以用来合并drawcall。还必须结合draw函数
public class TestPropertyBlock : MonoBehaviour {
    GameObject[] listObj = null;
    GameObject[] listProp = null;

    public int objCount = 100;
    MaterialPropertyBlock prop = null;

    public GameObject obj;
    int colorID;

    void Start()
    {
        colorID = Shader.PropertyToID("_Color");
        prop = new MaterialPropertyBlock();
        //var obj = Resources.Load("Perfabs/Sphere") as GameObject;
        listObj = new GameObject[objCount];
        listProp = new GameObject[objCount];
        for (int i = 0; i < objCount; ++i)
        {
            int x = Random.Range(-6, -2);
            int y = Random.Range(-4, 4);
            int z = Random.Range(-4, 4);
            GameObject o = Instantiate(obj);
            o.name = i.ToString();
            o.transform.localPosition = new Vector3(x, y, z);
            listObj[i] = o;
        }
        for (int i = 0; i < objCount; ++i)
        {
            int x = Random.Range(2, 6);
            int y = Random.Range(-4, 4);
            int z = Random.Range(-4, 4);
            GameObject o = Instantiate(obj);
            o.name = (objCount + i).ToString();
            o.transform.localPosition = new Vector3(x, y, z);
            listProp[i] = o;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < objCount; ++i)
            {
                float r = Random.Range(0, 1f);
                float g = Random.Range(0, 1f);
                float b = Random.Range(0, 1f);
                listObj[i].GetComponent<Renderer>().material.SetColor(colorID, new Color(r, g, b, 1));
            }
            sw.Stop();
            UnityEngine.Debug.Log(string.Format("material total: {0:F4} ms", (float)sw.ElapsedTicks * 1000 / Stopwatch.Frequency));
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < objCount; ++i)
            {
                float r = Random.Range(0, 1f);
                float g = Random.Range(0, 1f);
                float b = Random.Range(0, 1f);

                // 用 MaterialPropertyBlock 的目的是 修改单体材质球属性时不生成新的 材质球实例。 因为材质球实例 无法跟 
                //其他的原始材质球 一起批处理， 所以如果因为需要修改单个材质球属性造成的 无法 batch， 用MaterialPropertyBlock 可以解决。 
                //如果本身就没法batch的， 你用MaterialPropertyBlock 也不会有效果。
                //这个时候我们可以配合Gpu Instance来进一步的提高性能，使用Gpu Instance一是可以省去实体对象本身的开销，
                //二是能够起到减少Drawcall的作用，
                //同时还能减少动态合批的cpu开销，静态合批的内存开销



                //prop = new MaterialPropertyBlock();
                listProp[i].GetComponent<Renderer>().GetPropertyBlock(prop);
                prop.SetColor(colorID, new Color(r, g, b, 1));
                listProp[i].GetComponent<Renderer>().SetPropertyBlock(prop);
            }
            sw.Stop();
            UnityEngine.Debug.Log(string.Format("MaterialPropertyBlock total: {0:F4} ms", (float)sw.ElapsedTicks * 1000 / Stopwatch.Frequency));
        }
    }
}
