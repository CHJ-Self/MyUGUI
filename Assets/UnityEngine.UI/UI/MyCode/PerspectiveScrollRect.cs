using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class PerspectiveScrollRect : MonoBehaviour
{
    private ScrollRect scrollRect;
    private PerspectiveImage[] perspectiveImages; 

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        perspectiveImages = GetComponentsInChildren<PerspectiveImage>();
        scrollRect.onValueChanged.AddListener(UpdataChilds);
        UpdataChilds(Vector2.zero);
    }

    /// <summary>
    /// 更新子节点
    /// </summary>
    void UpdataChilds(Vector2 vector2)
    {
        foreach(var i in perspectiveImages)
        {
            i.ModifyMesh(new VertexHelper());
        }
    }
}
