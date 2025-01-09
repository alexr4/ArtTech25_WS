using UnityEngine;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Jnc_DynamicLightningDemo : MonoBehaviour
{
    [System.Serializable]
    public struct CutParameters
    {
        [Range(0, 1)]
        public float duration;

        [Range(0, 1)]
        public float delay;

        [Range(0.1f, 10)]
        public float speed;

        public static CutParameters Default => new()
        {
            duration = 1f,
            delay = 0.5f,
            speed = 4f
        };

        public readonly (float cutIn, float cutOut) ComputeCut(float time)
        {
            var tx = 1f / speed;
            var t = (time * speed) % (duration + 1 + delay);
            var t0 = t - duration;
            var t1 = t;
            var cutIn = Mathf.Clamp01(t0);
            var cutOut = Mathf.Clamp01(t1);

            return (cutIn, cutOut);
        }
    }

    public int subdivisions = 8;

    public int seed = 0;

    public float frequency = 18;

    public int randomExtraSubdivisions = 5;

    public CutParameters animCut = CutParameters.Default;

    public Vector3 start = Vector3.zero;
    public Vector3 end = Vector3.right * 8;
    public Vector3 normal = Vector3.back;

    [Range(0.01f, 5f)]
    public float tapeWidth = 0.75f;

    [Range(0.01f, 1f)]
    public float lightningWidthRatio = 0.15f;

    [Tooltip("If true, the start and end points will be set to the first two children of this object.")]
    public bool useChildren = true;

    float time;
    float redrawTime;
    Random.State randomState;

    void RandomReset()
    {
        Random.InitState(seed);
        randomState = Random.state;
    }

    void ProcessChildren()
    {
        while (transform.childCount < 2)
        {
            new GameObject("Point").transform.SetParent(transform);
        }

        transform.GetChild(0).gameObject.name = "Start";
        transform.GetChild(1).gameObject.name = "End";

        start = transform.GetChild(0).localPosition;
        end = transform.GetChild(1).localPosition;
    }

    Vector3[] CreatePoints()
    {
        Random.state = randomState;

        var count = 1 + subdivisions + Random.Range(0, randomExtraSubdivisions);

        var delta = end - start;
        var length = delta.magnitude;
        var direction = delta / length;

        var points = new Vector3[count];
        points[0] = start;
        points[count - 1] = end;

        var binormal = Vector3.Cross(direction, normal).normalized;

        for (var i = 1; i < count - 1; i++)
        {
            var t = (float)(i + 1) / count;
            var normalOffset = Random.Range(-0.5f, 0.5f) * lightningWidthRatio * length * binormal;
            points[i] = start + t * length * direction + normalOffset;
        }

        randomState = Random.state;

        return points;
    }

    void ProcessMesh()
    {
        var points = CreatePoints();
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        Jnc_DynamicMesh.GenerateTapeGeometry(mesh, points, new()
        {
            width = tapeWidth,
            rotation = 0,
            normal = normal
        });
    }

    void ProcessRedraw()
    {
        redrawTime += Time.deltaTime;

        if (redrawTime > 1 / frequency)
        {
            redrawTime -= 1 / frequency;

            if (useChildren)
                ProcessChildren();

            ProcessMesh();
        }
    }

    void OnEnable()
    {
        RandomReset();
    }

    void OnValidate()
    {
        RandomReset();
    }

    void Update()
    {
        if (Application.isPlaying == false)
            RandomReset();

        time += Time.deltaTime;

        ProcessRedraw();

        if (Application.isPlaying)
        {
            // var duration = 2f;
            // var delay = 0.5f;
            // var speed = 2f;

            // var tx = 1f / speed;
            // var t = (time * speed) % (duration + tx + delay);
            // var t0 = t - duration;
            // var t1 = t;
            // var cutIn = Mathf.Max(t0, 0);
            // var cutOut = Mathf.Max(t1, 0);

            var (cutIn, cutOut) = animCut.ComputeCut(time);
            var material = GetComponent<MeshRenderer>().material;
            material.SetFloat("_CutIn", cutIn);
            material.SetFloat("_CutOut", cutOut);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Jnc_DynamicLightningDemo))]
    public class Jnc_DynamicLightningDemoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var demo = (Jnc_DynamicLightningDemo)target;

            if (GUILayout.Button("Process Children"))
                demo.ProcessChildren();
        }
    }
#endif
}
