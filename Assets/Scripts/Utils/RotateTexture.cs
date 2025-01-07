using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTexture : MonoBehaviour
{
    public bool isAutoUpdate = true;
    public RenderTexture input;
    public RenderTexture output;
    private Material material;

    private void Awake(){
        CreateMaterial();
    }
    
    private void Update(){
        if(isAutoUpdate) ApplyFilter();
    }

    public void ApplyFilter(){
        // CreateMaterial();

        Graphics.Blit(input, output, material);

    }

    private void CreateMaterial(){
        if(material == null){
            material = new Material(Shader.Find("Hidden/RotateTexture"));
        }
    }
}
