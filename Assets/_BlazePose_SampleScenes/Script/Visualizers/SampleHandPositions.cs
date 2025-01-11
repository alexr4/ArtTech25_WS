using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bonjour;

public class SampleHandPositions : Singleton<SampleHandPositions>
{
    public RenderTexture tracerTexture;
    public Shader tracerShader;
    public float radius = 0.05f;
    public float fadeOutfactor = 0.95f;
    private Material tracerMat;

    private Vector4 prevRightHand;
    private Vector4 prevLeftHand;

    public enum TraceType
    {
        BOTH,
        RIGHT,
        LEFT
    }

    public TraceType tracetype = TraceType.BOTH;

    // Start is called before the first frame update
    void Start()
    {
        tracerMat = new Material(tracerShader);
    }

    #region Trace Hand
    // Update is called once per frame
    void Update()
    {
        /*
        if (prevRightHand == null) prevRightHand = UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Right);
        if (prevLeftHand == null) prevLeftHand = UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Left);

        Trace(prevRightHand, UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Right));
        Trace(prevLeftHand, UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Left));

        prevRightHand = UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Right);
        prevLeftHand = UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Left);
        */

        switch (tracetype)
        {
            default:
            case TraceType.BOTH:
                TraceHand(ref prevRightHand, UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Right));
                TraceHand(ref prevLeftHand, UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Left));
                break;
            case TraceType.RIGHT:
                TraceHand(ref prevLeftHand, UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Left));
                break;
            case TraceType.LEFT:
                TraceHand(ref prevRightHand, UserController.Instance.GetFilteredHandNormalScreenPosition(UserController.HandOrientation.Right));
                break;
        }
    }

    private void TraceHand(ref Vector4 prevHand, Vector4 hand)
    {
        if (prevHand == null) prevHand = hand;

        Trace(prevHand, hand);

        prevHand = hand;
    }

    public void Trace(Vector4 prevhand, Vector4 hand)
    {
        tracerMat.SetVector("prevbone", new Vector4(prevhand.x, prevhand.y, 0, 0));
        tracerMat.SetVector("bone", new Vector4(hand.x, hand.y, 0, 0));
        tracerMat.SetFloat("_Radius", radius);
        tracerMat.SetFloat("_FadeOutFactor", fadeOutfactor);
        tracerMat.SetFloat("_Aspect", (float)(tracerTexture.width) / (float)(tracerTexture.height));

        RenderTexture _TMP = RenderTexture.GetTemporary(tracerTexture.descriptor);
        Graphics.Blit(tracerTexture, _TMP, tracerMat);
        Graphics.Blit(_TMP, tracerTexture);

        RenderTexture.ReleaseTemporary(_TMP);
    }
    #endregion
    public void ResetTargetRT()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = tracerTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
    }

    #region Manage Trace type
    public void SetTraceType(TraceType type)
    {
        ResetTargetRT();
        tracetype = type;
    }

    public void SetTraceTypeToBoth()
    {
        SetTraceType(TraceType.BOTH);
    }

    public void SetTraceTypeToRight()
    {
        SetTraceType(TraceType.RIGHT);
    }

    public void SetTraceTypeToLeft()
    {
        SetTraceType(TraceType.LEFT);
    }
    #endregion


    private void OnDisable()
    {
        tracerTexture?.Release();
    }
}
