using UnityEngine;
using System.Collections;

public class RenderQueueModifier : MonoBehaviour
{

    Renderer[] _renderers;
    public int m_RenderQueue = 2001;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in _renderers)
       {
          r.material.renderQueue = m_RenderQueue;
       }
    }
}
