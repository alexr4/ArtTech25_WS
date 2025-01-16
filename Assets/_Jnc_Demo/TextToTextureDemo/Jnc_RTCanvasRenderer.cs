using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Jnc_RTCanvasRenderer : MonoBehaviour
{
    public Camera rtCamera;

    public RenderTexture renderTexture;

    public string title = "RTCanvasRenderer";

    public Texture RenderToTexture()
    {
        GetComponentInChildren<TMPro.TextMeshProUGUI>().text = title;

        rtCamera.targetTexture = renderTexture;
        rtCamera.Render();

        // Clone the render texture to a regular Texture2D
        var cloneTexture = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGBA32,
            mipChain: true);

        var oldActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        cloneTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        cloneTexture.Apply();
        RenderTexture.active = oldActive;

        return cloneTexture;
    }

    [CustomEditor(typeof(Jnc_RTCanvasRenderer))]
    public class Jnc_RTCanvasRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var rtCanvasRenderer = (Jnc_RTCanvasRenderer)target;

            if (GUILayout.Button("Render To Texture"))
            {
                rtCanvasRenderer.RenderToTexture();
            }
        }
    }
}
