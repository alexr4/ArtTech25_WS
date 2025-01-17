using UnityEngine;

[ExecuteAlways]
public class Jnc_DynamicTapeMeshDemo : MonoBehaviour
{
    public Jnc_DynamicMesh.TapeGeometryParameters parameters = Jnc_DynamicMesh.TapeGeometryParameters.Default;

    void EnsureMesh()
    {
        if (TryGetComponent<MeshFilter>(out var meshFilter) == false)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        if (TryGetComponent<MeshRenderer>(out var meshRenderer) == false)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        var mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            meshFilter.sharedMesh = mesh;
        }
    }

    int ComputeMeshHashCode()
    {
        var hashCode = parameters.GetHashCode();
        foreach (Transform child in transform)
            hashCode ^= child.position.GetHashCode();
        return hashCode;
    }

    Vector3[] GetChildrenPositions()
    {
        var positions = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            positions[i] = transform.GetChild(i).localPosition;
        return positions;
    }

    int cache;

    void Update()
    {
        if (Application.isPlaying == false)
            EnsureMesh();

        if (cache != ComputeMeshHashCode())
        {
            cache = ComputeMeshHashCode();
            var positions = GetChildrenPositions();

            if (positions.Length > 1)
            {
                var mesh = GetComponent<MeshFilter>().sharedMesh;
                Jnc_DynamicMesh.GenerateTapeGeometry(mesh, positions, parameters);
            }
        }
    }
}
