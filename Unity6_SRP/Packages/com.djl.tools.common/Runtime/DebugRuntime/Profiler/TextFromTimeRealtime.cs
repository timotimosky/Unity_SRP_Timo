using UnityEngine;
using UnityEngine.UI;

public enum TimeType
{
    RealTime,
    UpdateTime,
}

public class TextFromTimeRealtime : MonoBehaviour
{
    public TimeType curTimeType = TimeType.RealTime;

    private float m_LastTime = 0;
    Text mText;
    private void Start()
    {
        mText = GetComponent<Text>();
    }

    void Update()
    {
        if (curTimeType == TimeType.RealTime)
        {
            SetRealTime();
        }
        else
            SetTime();
    }

    void SetTime()
    {
        float t = Time.time;
        mText.text =
            "Time.time=" + t.ToString("0.000s") + " (delta=" + ((t - m_LastTime) * 1000).ToString("0.00ms") + ")";
        m_LastTime = t;
    }

    void SetRealTime()
    {
        float t = Time.realtimeSinceStartup;
        mText.text =
            "Time.realtimeSinceStartup=" + t.ToString("0.000s") + " (delta=" + ((t - m_LastTime) * 1000).ToString("0.00ms") + ")";
        m_LastTime = t;
    }
}
