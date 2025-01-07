// 05/01/2025 AI-Tag
// This was created with assistance from Muse, a Unity Artificial Intelligence product

using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ProceduralTriangle : MonoBehaviour
{
    private Mesh mesh;

    void Start()
    {
        // Create a new mesh
        mesh = new Mesh();

        // Define vertices of the triangle
        Vector3[] vertices = {
            new Vector3(0, 1, 0),   // Top
            new Vector3(-1, -1, 0), // Bottom Left
            new Vector3(1, -1, 0)   // Bottom Right
        };

        // Define colors for each vertex (RGB)
        Color[] colors = {
            Color.red,   // Top (Red)
            Color.green, // Bottom Left (Green)
            Color.blue   // Bottom Right (Blue)
        };

        // Define the indices for the triangle
        int[] indices = { 0, 1, 2 };

        // Assign vertices, colors, and indices to the mesh
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        // Get the MeshFilter and assign the created mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Ensure a basic material is used
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("HDRP/Unlit"));
    }
}