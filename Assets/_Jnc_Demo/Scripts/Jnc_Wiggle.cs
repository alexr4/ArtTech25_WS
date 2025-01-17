using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Jnc_Wiggle : MonoBehaviour
{
    public int seed;

    [Range(0.1f, 10)]
    public float frequency = 3;

    [Range(0.1f, 10)]
    public float amplitude = 1;

    [Range(1, 10)]
    public int count = 4;

    public bool randomSeed = true;

    Jnc_Utils.WigglePosition wiggle = new();

    Vector3 initialPosition;

    void Initialize()
    {
        if (randomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        wiggle.Initialize(seed, count);
    }

    void Start()
    {
        initialPosition = transform.position;
        Initialize();
    }

    void Update()
    {
        wiggle.amplitude = amplitude;
        wiggle.frequency = frequency;
        wiggle.Update(Time.deltaTime);

        transform.position = initialPosition + wiggle.Delta;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        wiggle.DrawGizmos(transform.position);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Jnc_Wiggle)), CanEditMultipleObjects]
    class Jnc_WiggleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reset Wiggle"))
            {
                var wiggle = target as Jnc_Wiggle;
                wiggle.Initialize();
            }
        }
    }
#endif
}
