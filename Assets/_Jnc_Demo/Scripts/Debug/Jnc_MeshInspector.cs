using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Jnc_MeshInspector : MonoBehaviour
{
    Mesh mesh;

    int triangleIndex = 0;

    void Update()
    {
        if (TryGetComponent<MeshFilter>(out var meshFilter))
            mesh = meshFilter.sharedMesh;
    }

    void OnDrawGizmos()
    {
        if (mesh == null)
            return;

        var triangles = mesh.triangles;
        if (triangleIndex < 0 || triangleIndex >= triangles.Length / 3)
            return;

        var i = triangleIndex * 3;
        var a = mesh.vertices[triangles[i + 0]];
        var b = mesh.vertices[triangles[i + 1]];
        var c = mesh.vertices[triangles[i + 2]];

        Gizmos.color = Color.red;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, a);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(a, 0.04f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(b, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(c, 0.06f);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Jnc_MeshInspector))]
    public class Jnc_MeshInspectorEditor : Editor
    {
        Jnc_MeshInspector t => (Jnc_MeshInspector)target;

        void InspectMesh(Mesh mesh)
        {
            var triCount = mesh.triangles.Length / 3;
            EditorGUI.BeginChangeCheck();
            t.triangleIndex = EditorGUILayout.IntSlider("Triangle Index", t.triangleIndex, 0, triCount - 1);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
                t.triangleIndex = (t.triangleIndex - 1 + triCount) % triCount;
            if (GUILayout.Button("Next"))
                t.triangleIndex = (t.triangleIndex + 1 + triCount) % triCount;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            EditorGUILayout.HelpBox($"Triangle {t.triangleIndex}/{triCount}", MessageType.None);
            EditorGUILayout.HelpBox($"Triangle {mesh.triangles[t.triangleIndex * 3]} {mesh.triangles[t.triangleIndex * 3 + 1]} {mesh.triangles[t.triangleIndex * 3 + 2]}", MessageType.None);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (t.mesh != null)
                InspectMesh(t.mesh);
        }
    }
#endif
}
