using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Klak.TestTools;
using MediaPipe.FaceMesh;
using System.Collections.Generic;
using System;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using Bonjour;
using Mediapipe.BlazePose;
using static Mediapipe.BlazePose.BlazePoseLandmarkFilters;
using UnityEngine.SocialPlatforms.Impl;

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes
    [Header("AI params")]
    [SerializeField] RenderTexture _source = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;

    [Header("Rendering")]
    public ComputeShader compute;

    [Header("Debug section")]
    [SerializeField] Shader _shader = null;
    [Space]
    [SerializeField] RawImage _mainUI = null;
    [SerializeField] RawImage _faceUI = null;
    [SerializeField] RawImage _leftEyeUI = null;
    [SerializeField] RawImage _rightEyeUI = null;
    [Space]
    [SerializeField] bool drawDebug;
    [SerializeField] float debugIcoSize;
    [SerializeField] Texture2D debugIco;


    #endregion

    #region Private members

    private FacePipeline _pipeline;
    private Material _material;
    private RenderTexture _outputTexture = null;
    private ComputeShader _compute;
    private ComputeBuffer vertexBuffer, normalBuffer;
    private int kernelVertexHandle, kernelNormalHandle;
    private Mesh mesh;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _pipeline = new FacePipeline(_resources);
        _material = new Material(_shader);
        _outputTexture = new RenderTexture(1024, 1024, 0);

        _compute = Instantiate(compute);
        kernelVertexHandle = _compute.FindKernel("UpdateVertices");
        kernelNormalHandle = _compute.FindKernel("UpdateNormals");

        ResetWindingOrder();
    }

    void OnDestroy()
    {
        _pipeline.Dispose();
        Destroy(_material);

        vertexBuffer?.Dispose();
        normalBuffer?.Dispose();
        if (_compute != null) Destroy(_compute);
    }

    void LateUpdate()
    {
        Blit(_source);

        // Processing on the face pipeline
        _pipeline.ProcessImage(_outputTexture);

        // UI update for debug
        _mainUI.texture = _outputTexture;
        _faceUI.texture = _pipeline.CroppedFaceTexture;
        _leftEyeUI.texture = _pipeline.CroppedLeftEyeTexture;
        _rightEyeUI.texture = _pipeline.CroppedRightEyeTexture;

        //GPU-CPU Update Mesh
        UpdateMeshGPU();
        //UpdateMeshCPU();
    }

    private void UpdateMeshCPU()
    {
        if (_pipeline.RefinedFaceVertexBuffer == null) return;

        Vector4[] vertexBuffer = new Vector4[_pipeline.RefinedFaceVertexBuffer.count];
        _pipeline.RefinedFaceVertexBuffer.GetData(vertexBuffer);


        // Get the MeshFilter component
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
        {
           
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
            mesh.SetVertices(vertices);

            mesh.RecalculateNormals();
            // mesh.RecalculateBounds();

        }
    }

    private void UpdateMeshGPU()
    {
        if(_pipeline.RefinedFaceVertexBuffer == null) return;

        //Update vertex position
        Vector3[] vertices = mesh.vertices;

        if(vertexBuffer == null) vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        vertexBuffer.SetData(vertices);

        _compute.SetBuffer(kernelVertexHandle, "vertexBuffer", vertexBuffer);
        _compute.SetBuffer(kernelVertexHandle, "refinedFaceVertexBuffer", _pipeline.RefinedFaceVertexBuffer);

        int threadVertexGroups = Mathf.CeilToInt(vertices.Length / 256.0f);
        _compute.Dispatch(kernelVertexHandle, threadVertexGroups, 1, 1);

        vertexBuffer.GetData(vertices);
        mesh.vertices = vertices;

        //Correct Normal
        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
    }

    private void ResetWindingOrder()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        var indices = mesh.triangles;
        var triangleCount = indices.Length / 3;
        for (var i = 0; i < triangleCount; i++)
        {
            var tmp = indices[i * 3];
            indices[i * 3] = indices[i * 3 + 1];
            indices[i * 3 + 1] = tmp;
        }
        mesh.triangles = indices;
    }


    private void Blit(Texture source, bool vflip = false)
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

    
    //This methods doesn't render in HDRP
    private void OnRenderObject()
    {
        // Main view overlay
        var mv = float4x4.Translate(math.float3(-0.875f, -0.5f, 0));
        _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(_resources.faceLineTemplate, mv);

        // Face view
        // Face mesh
        /*
        var fF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, -0.5f));
        _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
        _material.SetPass(0);
        Graphics.DrawMeshNow(_resources.faceMeshTemplate, fF);
        */

        /*
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
        */
        // Debug views
        // Face mesh
        /*
        var dF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, 0));
        _material.SetBuffer("_Vertices", _pipeline.RawFaceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(_resources.faceLineTemplate, dF);
        */
        /*
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
        */
    }
    

    #endregion

    #region Draw Debug
    private void OnGUI()
    {
        if (_pipeline != null && drawDebug)
        {
            for (int i = 0; i < mesh.vertices.Length - 1; i++)
            {
                Vector3 vertex = mesh.vertices[i];
                Vector2 screenPosition = Camera.main.WorldToScreenPoint(vertex);
                screenPosition.y = Screen.height - screenPosition.y;
                GUI.DrawTexture(new Rect(screenPosition.x - debugIcoSize * .5f, screenPosition.y - debugIcoSize * .5f, debugIcoSize, debugIcoSize), debugIco);
                GUI.Label(new Rect(screenPosition.x + debugIcoSize * .75f, screenPosition.y, 100, 20), $"<color=#ffffff>{i}.</color>", new GUIStyle()
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold
                });
            }
        }

    }
    #endregion
}
