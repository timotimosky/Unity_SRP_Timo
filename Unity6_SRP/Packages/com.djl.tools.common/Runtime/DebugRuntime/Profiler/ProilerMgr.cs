using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProilerMgr : MonoBehaviour {

    public int UsedAssetCount = 0;
    public int NotUsedAssetCount = 0;
    public string CurrentMemory = string.Empty;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Application.isEditor)
        {
         //   UsedAssetCount = AssetCacheManager.Instance.;
         //  NotUsedAssetCount = AssetCacheManager.Instance.NotUsedCacheList.Count;
            float mem = (float)GetCheckMemorySize() / (1024 * 1024);
            CurrentMemory = string.Format("{0}M", mem.ToString());
        }
    }

    public uint GetCheckMemorySize()
    {
        if (Application.isEditor)
            return (uint)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 2;
        else
            return (uint)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        // return Profiler.usedHeapSize;
    }
}
