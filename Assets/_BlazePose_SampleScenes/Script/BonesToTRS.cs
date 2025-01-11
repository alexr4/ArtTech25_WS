using UnityEngine;
using Bonjour;

public class BonesToTRS : MonoBehaviour
{
    public bool is2D = true;
    public Transform[] riggTRS;
   
    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<riggTRS.Length; i++)
        {
            //we get index of the TRS rig (i) which is set to be the same as the MediaPipe bone mapping https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker?hl=fr

            //if is2D == true then we get the bone in 2D space else we get it in wolrd space
            Vector4 bone = Vector4.zero;
            if (is2D)
            {
                if (UserController.Instance.whichBufferToFilter == Mediapipe.BlazePose.BlazePoseLandmarkFilters.WhichBufferToFilter.PoseLandmarks
                   || UserController.Instance.whichBufferToFilter == Mediapipe.BlazePose.BlazePoseLandmarkFilters.WhichBufferToFilter.Both)
                {
                    //get 2D filtered bone
                    bone = UserController.Instance.GetFilteredPoseLandmark(i);
                }
                else 
                {
                    //get 2D unfiltered bone
                    bone = UserController.Instance.GetPoseLandmark(i);
                }

                //Check if bone exists and is a number (NaN meaning Not a Number)
                if (bone != null
                   && !float.IsNaN(bone.x)
                   && !float.IsNaN(bone.y)
                   && !float.IsNaN(bone.z))
                {
                    //we remap the bone into the correct screen space
                    bone = Bonjour.MathsCoordUtils.RemapKeyPointScreenSpace(bone);
                    //BackProject into 3D Space
                    Vector3 worldSpaceBone = Camera.main.ScreenToWorldPoint(new Vector3(bone.x, Screen.height - bone.y, Camera.main.nearClipPlane));
                    bone.x = worldSpaceBone.x;
                    bone.y = worldSpaceBone.y;
                    bone.z = worldSpaceBone.z;
                }
            }
            else
            {
                if (UserController.Instance.whichBufferToFilter == Mediapipe.BlazePose.BlazePoseLandmarkFilters.WhichBufferToFilter.PoseWorldLandmarks
                                  || UserController.Instance.whichBufferToFilter == Mediapipe.BlazePose.BlazePoseLandmarkFilters.WhichBufferToFilter.Both)
                {
                    //get 3D filtered bone
                    bone = UserController.Instance.GetFilteredPoseWorldLandmark(i);
                }
                else
                {
                    //get 3D unfiltered bone
                    bone = UserController.Instance.GetPoseWorldLandmark(i);
                }
            }
            

            //Check if bone exists and is a number (NaN meaning Not a Number)
            if (bone != null
               && !float.IsNaN(bone.x) 
               && !float.IsNaN(bone.y) 
               && !float.IsNaN(bone.z)){
                riggTRS[i].position = new Vector3(bone.x, bone.y, bone.z);
            }
        }
    }
}
