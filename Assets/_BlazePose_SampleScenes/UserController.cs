using System;
using System.Collections.Generic;
using Mediapipe.BlazePose;
using UnityEngine;
using Bonjour.Time;
using Bonjour.Maths;

namespace Bonjour
{
	public class UserController : Singleton<UserController>
	{
        #region MediaPipe pipeline params
        [Header("Media Pipe Parameters")]
		public RenderTexture input;
		public BlazePoseModel blazePoseModel;
		[Range(0f, 1f)] public float poseThreshold = .75f;
		[Range(0f, 1f)] public float humanScoreThreshold = .95f;
		protected bool userDetected = false;
		private float userScore;
		protected bool userScoreOverHistory;
		public int userHistorySize = 120;
		private bool firstHistoryLoop = false;
        protected List<float> userDetectedHistory = new List<float>();

		private BlazePoseDetecter pipeline;
		public bool drawDebugBones;
        public bool drawDebugScore;
        public Texture2D bonesIco;
		[Range(2, 100)] public int debugBoneIcoSize = 50;
		public float score => userScore;
		#endregion

		#region Filtering data param
		[Header("Landmarks Filtering Parameters")]
		public BlazePoseLandmarkFilters.WhichBufferToFilter whichBufferToFilter;
		public BlazePoseLandmarkFilters.LandmarksToFilter landmarksToFilter = BlazePoseLandmarkFilters.LandmarksToFilter.FullBody;
		public DataFilters.FilterType filtertype;

		private BlazePoseLandmarkFilters landmarksFilters = new BlazePoseLandmarkFilters();

		[Range(0, 1f)] public float Q = 0.000001f;
		[Range(0, 1f)] public float R = 0.01f;
		[Range(0, 1f)] public float beta = 0.0001f;
		[Range(0, 1f)] public float minCutOff = 0.04f;


		public bool drawDebugFilteredBones;
		public Texture2D bonesFilteredIco;
		[Range(2, 100)]
		public int debugFilteredBoneIcoSize = 50;
		[Range(0f, 1f)]
		public float debugFilteredBone3DSize = 0.05f;
		#endregion

		#region Timer Params
		[Header("User timer Check")]
		public float timeout = 3;
		protected Timer userTimer;
		public bool logTimerEvents;
		#endregion

		#region Hand to Filtered landmarks
		public List<Vector4> poseLandmarksFiltered { get => landmarksFilters.poseLandmarksFiltered; }
		public List<Vector4> poseWorldLandmarksFiltered { get => landmarksFilters.poseWorldLandmarksFiltered; }
		public Vector4 GetFilteredPoseLandmark(int index) => landmarksFilters.poseLandmarksFiltered[index];
		public Vector4 GetFilteredPoseWorldLandmark(int index) => landmarksFilters.poseWorldLandmarksFiltered[index];
		#endregion

        #region Handles to BlazePoseDetector buffer + Filtered Landmark
        public ComputeBuffer outputBuffer {get => pipeline.outputBuffer;}
		public ComputeBuffer worldLandmarkBuffer{get => pipeline.outputBuffer;}
		public int vertexCount { get => pipeline.vertexCount; }
		public Vector4 GetPoseLandmark(int index) => pipeline.GetPoseLandmark(index);
		public Vector4 GetPoseWorldLandmark(int index) => pipeline.GetPoseWorldLandmark(index);
		#endregion

		public enum HandOrientation
        {
			Left,
			Right
        }

		#region Init & Update
		protected virtual void Awake()
        {
			Init();
		}

		protected virtual void Init()
        {
			//Init BlazePose Detector
			pipeline = new BlazePoseDetecter(blazePoseModel);

			//Init filtering
			landmarksFilters.Init();

			userTimer = new Timer("UserCheck", timeout);
			userTimer.StartTimer();
		}

		protected virtual void Update()
		{
			ProcessUserTracking();
		}

		protected virtual void OnDisable()
		{
			pipeline?.Dispose();

			userTimer.OnTimerStart.RemoveListener(OnUserTimerStart);
			userTimer.OnTimerEnd.RemoveListener(OnUserTimerEnd);
		}

		protected virtual void OnEnable()
		{
			userTimer.OnTimerStart.AddListener(OnUserTimerStart);
			userTimer.OnTimerEnd.AddListener(OnUserTimerEnd);
		}

		private void ProcessUserTracking()
        {
			pipeline.ProcessImage(input, blazePoseModel, poseThreshold);

            // Output landmark values(33 values) and the score whether human pose is visible (1 values).
            for (int i = 0; i < pipeline.vertexCount + 1; i++)
            {
                /*
                0~32 index datas are pose landmark.
                Check below Mediapipe document about relation between index and landmark position.
                https://google.github.io/mediapipe/solutions/pose#pose-landmark-model-blazepose-ghum-3d
                Each data factors are
                x: x cordinate value of pose landmark ([0, 1]).
                y: y cordinate value of pose landmark ([0, 1]).
                z: Landmark depth with the depth at the midpoint of hips being the origin.
                   The smaller the value the closer the landmark is to the camera. ([0, 1]).
                   This value is full body mode only.
                   **The use of this value is not recommended beacuse in development.**
                w: The score of whether the landmark position is visible ([0, 1]).

                33 index data is the score whether human pose is visible ([0, 1]).
                This data is (score, 0, 0, 0).

				//right hand is index 15 and left is index 16
                */
                //Debug.LogFormat("{0}: {1}", i, detecter.GetPoseLandmark(i));
            }
            //Debug.Log("---");

            FilterPoseLandmarks();

			//33 index data is the score whether human pose is visible([0, 1]).  This data is (score, 0, 0, 0).
			//? : the scoring seems to return false value sometimes. Maybe we can grab the countBuffer from PoseDetector directly inside BlazePoseDetector.cs (need to grab the package as a writrable asset)
			ComputeUserScore();
		}

		protected virtual void ComputeUserScore()
        {
			userScore = (pipeline.GetPoseLandmark(33)).x;
			userDetected = userScore >= humanScoreThreshold ? true : false;

			int userHistoryActualSize = userHistorySize;

			//check first loop
			if(!firstHistoryLoop)
			{
				if (userDetectedHistory.Count < userHistoryActualSize)
				{
					userHistoryActualSize = userDetectedHistory.Count;
				}
				else
				{
					firstHistoryLoop = true;
                }
            }

			//add history
			userDetectedHistory.Add(userDetected ? 1f : 0f);

			if(userDetectedHistory.Count >= userHistorySize && firstHistoryLoop)
			{
				userDetectedHistory.RemoveAt(0);
			}

			float historyWeight = 0f;
			foreach(float value in userDetectedHistory)
			{
                historyWeight += value;
            }
			historyWeight /= userHistoryActualSize;
			userScoreOverHistory = historyWeight >= .5f ? true : false;
        }

		private void FilterPoseLandmarks()
		{
			if (whichBufferToFilter == BlazePoseLandmarkFilters.WhichBufferToFilter.None) return;

			float param1, param2;
			switch (filtertype)
			{
				case DataFilters.FilterType.Kalman: 
					param1 = Q; param2 = R; 
					break;
				default:
				case DataFilters.FilterType.OneEuro:
					param1 = beta; param2 = minCutOff;
					break;
			}

			landmarksFilters.Filters(ref pipeline, param1, param2, whichBufferToFilter, landmarksToFilter, filtertype);
		}
		#endregion

		#region User check
		protected virtual void OnUserTimerStart(TimerData timerdata) {
			if (logTimerEvents) Debug.Log("<color=#00ff00>OnUserTimerStart.Start</color>");

		}

		protected virtual void OnUserTimerEnd(TimerData timerdata)
		{
			if (logTimerEvents) Debug.Log("<color=#ff0000>OnUserTimerStart.End</color>");
			userTimer.StartTimer();
		}

		public bool HasUser()
        {
			return userDetected;

		}
		#endregion

		#region Hand Tracking in 2D Only
		//Raw 2D Landmark Hand in ScreenSpace
		private Vector4 GetNormalScreenSpaceMiddleHand(HandOrientation orientation)
		{
			// Pose Landmark Model is refered https://google.github.io/mediapipe/solutions/pose#ml-pipeline.
			Vector4 wrist = pipeline.GetPoseLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightWrist : BlazePoseLandmarkModel.LeftWrist);
			Vector4 thumb = pipeline.GetPoseLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightThumb : BlazePoseLandmarkModel.LeftThumb);
			Vector4 index = pipeline.GetPoseLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightIndex : BlazePoseLandmarkModel.LeftIndex);
			Vector4 pinky = pipeline.GetPoseLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightPinky : BlazePoseLandmarkModel.LeftPinky);

			Vector4 gravityCenter = (wrist + thumb + index + pinky) / 4f;
			return gravityCenter;
		}

		public Vector4 GetHandScreenPosition(HandOrientation orientation)
		{
			return MathsCoordUtils.RemapKeyPointScreenSpace(GetNormalScreenSpaceMiddleHand(orientation));
		}

		public Vector4 GetHandNormalScreenPosition(HandOrientation orientation)
		{
			return GetNormalScreenSpaceMiddleHand(orientation);
		}
		
		//Filertered 2D Landmark Hand in ScreenSpace
		private Vector4 GetNormalScreenSpaceFilteredMiddleHand(HandOrientation orientation)
		{
			// Pose Landmark Model is refered https://google.github.io/mediapipe/solutions/pose#ml-pipeline.
			Vector4 wrist = landmarksFilters.poseLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightWrist : BlazePoseLandmarkModel.LeftWrist];
			Vector4 thumb = landmarksFilters.poseLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightThumb : BlazePoseLandmarkModel.LeftThumb];
			Vector4 index = landmarksFilters.poseLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightIndex : BlazePoseLandmarkModel.LeftIndex];
			Vector4 pinky = landmarksFilters.poseLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightPinky : BlazePoseLandmarkModel.LeftPinky];

			Vector4 gravityCenter = (wrist + thumb + index + pinky) / 4f;
			return gravityCenter;
		}

		public Vector4 GetFilteredHandScreenPosition(HandOrientation orientation)
		{
			return MathsCoordUtils.RemapKeyPointScreenSpace(GetNormalScreenSpaceFilteredMiddleHand(orientation));

		}

		public Vector4 GetFilteredHandNormalScreenPosition(HandOrientation orientation)
        {
			return GetNormalScreenSpaceFilteredMiddleHand(orientation);
		}

		//Raw & filtered 3D Landmark Hand in ScreenSpace
		private Vector4 Get3DSpaceMiddleHand(HandOrientation orientation)
		{
			// Pose Landmark Model is refered https://google.github.io/mediapipe/solutions/pose#ml-pipeline.
			Vector4 wrist = pipeline.GetPoseWorldLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightWrist : BlazePoseLandmarkModel.LeftWrist);
			Vector4 thumb = pipeline.GetPoseWorldLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightThumb : BlazePoseLandmarkModel.LeftThumb);
			Vector4 index = pipeline.GetPoseWorldLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightIndex : BlazePoseLandmarkModel.LeftIndex);
			Vector4 pinky = pipeline.GetPoseWorldLandmark(orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightPinky : BlazePoseLandmarkModel.LeftPinky);

			Vector4 gravityCenter = (wrist + thumb + index + pinky) / 4f;
			return gravityCenter;
		}

		private Vector4 GetFiltered3DSpaceMiddleHand(HandOrientation orientation)
		{
			// Pose Landmark Model is refered https://google.github.io/mediapipe/solutions/pose#ml-pipeline.
			Vector4 wrist = landmarksFilters.poseWorldLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightWrist : BlazePoseLandmarkModel.LeftWrist];
			Vector4 thumb = landmarksFilters.poseWorldLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightThumb : BlazePoseLandmarkModel.LeftThumb];
			Vector4 index = landmarksFilters.poseWorldLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightIndex : BlazePoseLandmarkModel.LeftIndex];
			Vector4 pinky = landmarksFilters.poseWorldLandmarksFiltered[orientation == HandOrientation.Right ? BlazePoseLandmarkModel.RightPinky : BlazePoseLandmarkModel.LeftPinky];

			Vector4 gravityCenter = (wrist + thumb + index + pinky) / 4f;
			return gravityCenter;
		}

		public Vector4 GetHand3DSpace(HandOrientation orientation) => Get3DSpaceMiddleHand(orientation);
		
		public Vector4 GetFilteredHand3DSpace(HandOrientation orientation) => GetFiltered3DSpaceMiddleHand(orientation);
		#endregion

		#region Draw Debug
		private void OnGUI()
        {
			//! TMP
			//todo: rewrittre full debug
			if (drawDebugScore)
			{
                GUI.color = Color.white;
                string color = userDetected ? "00ff00" : "ff0000";
                GUI.Label(new Rect(10, 10, 100, 20), $"<color=#ffffff>Human Score: {score}. \nHuman detected: </color><color=#{color}>{userDetected}</color>", new GUIStyle()
                {
                    fontSize = 30,
                    fontStyle = FontStyle.Bold
                });
            }

			//todo : this should be rewritten using Shader -> Bind ComputeBuffer to custom shader to draw bones using SDF or rewrite the original function for HDRP support
			if (pipeline != null && drawDebugBones)
			{
				for (int i = 0; i < pipeline.vertexCount - 1; i++)
				{
					Vector4 bone = pipeline.GetPoseLandmark(i);
					Vector3 screenSpaceBone = MathsCoordUtils.RemapKeyPointScreenSpace(bone);
					GUI.DrawTexture(new Rect(screenSpaceBone.x - debugBoneIcoSize * .5f, screenSpaceBone.y - debugBoneIcoSize * .5f, debugBoneIcoSize, debugBoneIcoSize), bonesIco);
					GUI.Label(new Rect(screenSpaceBone.x + debugBoneIcoSize * .75f, screenSpaceBone.y, 100, 20), $"<color=#ffffff>{i}.</color>", new GUIStyle()
					{
						fontSize = 30,
						fontStyle = FontStyle.Bold
					});
				}
			}

            if (drawDebugFilteredBones){
				if(whichBufferToFilter == BlazePoseLandmarkFilters.WhichBufferToFilter.Both || whichBufferToFilter == BlazePoseLandmarkFilters.WhichBufferToFilter.PoseLandmarks)
                {
					landmarksFilters.OnGUI(bonesFilteredIco, debugFilteredBoneIcoSize);
				}
			}
		}

        private void OnDrawGizmos()
        {
			if (drawDebugFilteredBones)
			{
				if (whichBufferToFilter == BlazePoseLandmarkFilters.WhichBufferToFilter.Both || whichBufferToFilter == BlazePoseLandmarkFilters.WhichBufferToFilter.PoseLandmarks)
				{
					landmarksFilters.OnDrawGizmo(debugFilteredBone3DSize);
				}
			}
		}
        #endregion

    }
}
