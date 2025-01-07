using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ProceduralDrawingHDRP : CustomPass
{
    public Material proceduralMaterial;
    public MeshTopology topology = MeshTopology.Triangles;
    public int vertexCount = 100;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Initialize the material (ensure it's HDRP compatible)
        if (proceduralMaterial == null)
        {
            Debug.LogError("Procedural Material is not assigned.");
            return;
        }
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (proceduralMaterial == null) return;

        CommandBuffer cmd = ctx.cmd;
        Camera camera = ctx.hdCamera.camera;

        // Set view and projection matrices
        cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

        // Draw procedural geometry
        cmd.DrawProcedural(Matrix4x4.identity, proceduralMaterial, 0, topology, vertexCount);
    }

    protected override void Cleanup()
    {
        // Cleanup resources if needed
    }
}