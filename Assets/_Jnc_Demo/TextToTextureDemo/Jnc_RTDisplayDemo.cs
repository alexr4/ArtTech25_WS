using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Jnc_RTDisplayDemo : MonoBehaviour
{
    public string rtSceneName;

    Jnc_RTCanvasRenderer rtCanvasRenderer;

    void Start()
    {
        SceneManager.LoadSceneAsync(rtSceneName, LoadSceneMode.Additive).completed += _ =>
        {
            var secondaryScene = SceneManager.GetSceneByName(rtSceneName);

            foreach (var go in secondaryScene.GetRootGameObjects())
            {
                rtCanvasRenderer = go.GetComponentInChildren<Jnc_RTCanvasRenderer>();
                if (rtCanvasRenderer != null)
                    break;
            }

            if (rtCanvasRenderer == null)
            {
                Debug.LogError($"Could not find Jnc_RTCanvasRenderer in scene {rtSceneName}");
                return;
            }

            FancyStuff();
        };
    }

    void OnDisable()
    {
        SceneManager.UnloadSceneAsync(rtSceneName);
    }

    void FancyStuff()
    {
        const int PLANE_COUNT = 40;

        var names = new string[] { "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliet" };

        for (int i = 0; i < PLANE_COUNT; i++)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            var direction = Random.onUnitSphere;
            plane.transform.position = direction * Random.Range(1.7f, 2f) + Vector3.up * 2;
            plane.transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(90, 0, 0) * Quaternion.Slerp(Quaternion.identity, Random.rotationUniform, 0.1f);
            plane.transform.localScale = Vector3.one * 0.25f;

            var material = new Material(Shader.Find("UI/Unlit/Transparent"));
            var name = names[i % names.Length];
            plane.name = $"Plane {name}";
            rtCanvasRenderer.title = $"Hello {name}!";
            material.mainTexture = rtCanvasRenderer.RenderToTexture();
            plane.GetComponent<MeshRenderer>().material = material;
        }
    }

    [CustomEditor(typeof(Jnc_RTDisplayDemo))]
    public class Jnc_RTDisplayDemoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var rtDisplayDemo = (Jnc_RTDisplayDemo)target;

            EditorGUI.BeginChangeCheck();
            var sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(
                "Pick RT Scene",
                null,
                typeof(SceneAsset),
                allowSceneObjects: false
            );
            if (EditorGUI.EndChangeCheck() && sceneAsset != null)
            {
                rtDisplayDemo.rtSceneName = sceneAsset.name;
                EditorUtility.SetDirty(rtDisplayDemo);
            }
        }
    }
}
