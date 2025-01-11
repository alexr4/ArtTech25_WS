using UnityEngine;
using Bonjour.Vision;
using UnityEngine.VFX;
using Unity.VisualScripting;

public class BindOpticalFlowToVFX : MonoBehaviour
{
    public VisualEffect vfx;
    public OpticalFlow ofController;
    public OFTrailSystemUpdater ofTrail;

    private bool hasTrail;

    private void Update()
    {

        if (ofTrail != null && ofTrail.GetOFTrail() != null) vfx.SetTexture("OpticalFlowMap", ofTrail.GetOFTrail());
        else if(ofController!= null && ofController.GetOpticalFlowMap() != null) vfx.SetTexture("OpticalFlowMap", ofController.GetOpticalFlowMap());
    }
}
