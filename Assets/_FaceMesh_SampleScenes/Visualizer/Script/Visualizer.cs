using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Klak.TestTools;
using MediaPipe.FaceMesh;
using System.Collections.Generic;
using System;

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] RenderTexture _source = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _shader = null;
    [Space]
    [SerializeField] RawImage _mainUI = null;
    [SerializeField] RawImage _faceUI = null;
    [SerializeField] RawImage _leftEyeUI = null;
    [SerializeField] RawImage _rightEyeUI = null;
    

    #endregion

    #region Private members

    FacePipeline _pipeline;
    Material _material;
    RenderTexture _outputTexture = null;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _pipeline = new FacePipeline(_resources);
        _material = new Material(_shader);
        _outputTexture = new RenderTexture(1024, 1024, 0);
    }

    void OnDestroy()
    {
        _pipeline.Dispose();
        Destroy(_material);
    }

    void LateUpdate()
    {
        Blit(_source);

        // Processing on the face pipeline
        _pipeline.ProcessImage(_outputTexture);

        // UI update
        _mainUI.texture = _outputTexture;
        _faceUI.texture = _pipeline.CroppedFaceTexture;
        _leftEyeUI.texture = _pipeline.CroppedLeftEyeTexture;
        _rightEyeUI.texture = _pipeline.CroppedRightEyeTexture;

        //Update Mesh Filter
        //bind GPU vertex buffer to CPU public vertex buffer 
        Vector4[] vertexBuffer = new Vector4[_pipeline.RefinedFaceVertexBuffer.count] ;
        _pipeline.RefinedFaceVertexBuffer.GetData(vertexBuffer);

        // Get the MeshFilter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        //var mv = float4x4.Translate(math.float3(-1f, -0.5f, 0)); // inly if we want to use matrix multiplication

        if (meshFilter != null)
        {
            // Get the mesh
            Mesh mesh = meshFilter.mesh;

            Vector3[] vertices = new Vector3[vertexBuffer.Length];

            // Modify the vertices (e.g., move each vertex upwards by 1 unit)
            for (int i = 0; i < vertices.Length; i++)
            {
                //float4 viewVertex = math.mul(mv, vertexBuffer[i]);  // only if we want to use matrix multiplication
                //Vector3 vertex = new Vector3(viewVertex.x, viewVertex.y, viewVertex.z);
                
                //remap to the center
                float3 sampleVertex = new Vector3(vertexBuffer[i].x - .5f, vertexBuffer[i].y - .5f, vertexBuffer[i].z);

                vertices[i] = sampleVertex;
            }

            // Update the mesh with the new vertices
            mesh.vertices = vertices;

            // Recalculate bounds and normals to ensure the mesh is correctly updated
            //mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }

    void Blit(Texture source, bool vflip = false)
    {
        if (source == null) return;

        var aspect1 = (float)source.width / source.height;
        var aspect2 = (float)_outputTexture.width / _outputTexture.height;

        var scale = new Vector2(aspect2 / aspect1, aspect1 / aspect2);
        scale = Vector2.Min(Vector2.one, scale);
        if (vflip) scale.y *= -1;

        var offset = (Vector2.one - scale) / 2;

        Graphics.Blit(source, _outputTexture, scale, offset);
    }

    void OnRenderObject()
    {
        // Main view overlay
        var mv = float4x4.Translate(math.float3(-0.875f, -0.5f, 0));
        _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(_resources.faceLineTemplate, mv);

        // Face view
        // Face mesh
        var fF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, -0.5f));
        _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
        _material.SetPass(0);
        Graphics.DrawMeshNow(_resources.faceMeshTemplate, fF);

        // Left eye
        var fLE = math.mul(fF, _pipeline.LeftEyeCropMatrix);
        _material.SetMatrix("_XForm", fLE);
        _material.SetBuffer("_Vertices", _pipeline.RawLeftEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Right eye
        var fRE = math.mul(fF, _pipeline.RightEyeCropMatrix);
        _material.SetMatrix("_XForm", fRE);
        _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Debug views
        // Face mesh
        var dF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, 0));
        _material.SetBuffer("_Vertices", _pipeline.RawFaceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(_resources.faceLineTemplate, dF);

        // Left eye
        var dLE = MathUtil.ScaleOffset(0.25f, math.float2(0.625f, 0.25f));
        _material.SetMatrix("_XForm", dLE);
        _material.SetBuffer("_Vertices", _pipeline.RawLeftEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Right eye
        var dRE = MathUtil.ScaleOffset(0.25f, math.float2(0.625f, 0f));
        _material.SetMatrix("_XForm", dRE);
        _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
    }

    #endregion
}
