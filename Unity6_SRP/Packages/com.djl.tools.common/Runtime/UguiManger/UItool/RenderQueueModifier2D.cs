using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RenderQueueModifier2D : MonoBehaviour
{
    public string stortingTag = "GroundSurface";
    void Awake()
    {
        Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.sortingLayerName = stortingTag;
        }   
    }
}
