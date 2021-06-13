using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(MeshCollider))]
public class PerspectiveButton : MonoBehaviour
{
    private MeshCollider meshCollider;
    private Graphic graphic;
    private Button button;

    private void Start()
    {
        graphic = GetComponent<Graphic>();
        button = GetComponent<Button>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void SetColliderMesh()
    {
        Mesh mesh = new Mesh();
        VertexHelper vertexHelper = new VertexHelper();
        graphic.OnPopulateMesh_Public(vertexHelper);
        vertexHelper.FillMesh(mesh);
        meshCollider.sharedMesh = mesh;
    }

    private void OnMouseDown()
    {
        button.onClick.Invoke();
    }

    public void OnClick()
    {
        print(1);
    }
}
