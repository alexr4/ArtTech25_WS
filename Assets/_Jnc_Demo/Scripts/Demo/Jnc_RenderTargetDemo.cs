using UnityEngine;
using UnityEngine.UI;

public class Jnc_RenderTargetDemo : MonoBehaviour
{
    [Range(0.001f, 0.1f)]
    public float brushRadius = 0.05f;

    [Range(0, 1)]
    public float brushSoftness = 0.05f;

    public Color brushColor = Color.red;

    [Range(0, 0.05f)]
    public float decayFrameFactor = 1 / 256f;

    Material brushMaterial;
    Material fadeToMaterial;

    bool rt1IsDst = true;
    RenderTexture rt1;
    RenderTexture rt2;

    (RenderTexture src, RenderTexture dst) GetRenderTexture()
    {
        return rt1IsDst ? (rt2, rt1) : (rt1, rt2);
    }

    (RenderTexture src, RenderTexture dst) GetRenderTextureAndSwap()
    {
        rt1IsDst = !rt1IsDst;
        return GetRenderTexture();
    }

    void Start()
    {
        brushMaterial = new Material(Shader.Find("Custom/Jnc_CircleBrush"));
        fadeToMaterial = new Material(Shader.Find("Custom/Jnc_FadeTo"));

        rt1 = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32);
        rt1.Create();

        rt2 = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32);
        rt2.Create();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // Get the mouse position in screen space
            var mouse = Input.mousePosition;

            var uv = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);

            brushMaterial.SetVector("_BrushPosition", new Vector4(uv.x, uv.y, 0, 0));
            brushMaterial.SetFloat("_BrushRadius", brushRadius);
            brushMaterial.SetColor("_BrushColor", brushColor);
            brushMaterial.SetFloat("_BrushSoftness", brushSoftness);

            // Swap the render textures and render the brush
            var (src, dst) = GetRenderTextureAndSwap();
            Graphics.Blit(src, dst, brushMaterial);
        }

        {
            fadeToMaterial.SetFloat("_DecayFrameFactor", decayFrameFactor);

            var (src, dst) = GetRenderTextureAndSwap();
            Graphics.Blit(src, dst, fadeToMaterial);

            // Display the render target
            GetComponent<Image>().material.mainTexture = dst;
        }
    }
}
