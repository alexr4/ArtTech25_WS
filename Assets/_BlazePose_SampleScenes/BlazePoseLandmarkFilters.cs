using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bonjour.Maths;

namespace Mediapipe.BlazePose { 
	public class BlazePoseLandmarkFilters
	{
		public enum WhichBufferToFilter
		{
			None,
			PoseLandmarks,
			PoseWorldLandmarks,
			Both
		}

		public enum LandmarksToFilter
		{
			FullBody,
			UpperBody,
			Hands
		}

        #region private var
        private List<DataFilters> _poseLandmarksFilters = new List<DataFilters>();
		private List<DataFilters> _poseWorldLandmarksFilters = new List<DataFilters>();
		private List<Vector4> _poseLandmarksFiltered = new List<Vector4>();
		private List<Vector4> _poseWorldLandmarksFiltered = new List<Vector4>();
		#endregion

		#region public var
		public List<Vector4> poseLandmarksFiltered { get => _poseLandmarksFiltered; }
		public List<Vector4> poseWorldLandmarksFiltered { get => _poseWorldLandmarksFiltered; }
        #endregion

        #region public metods
        public void Init()
        {
			foreach (int index in BlazePoseLandmarkModel.FullBody)
			{
				//create and add filter
				_poseLandmarksFilters.Add(new DataFilters());
				_poseWorldLandmarksFilters.Add(new DataFilters());

				_poseLandmarksFiltered.Add(Vector4.zero);
				_poseWorldLandmarksFiltered.Add(Vector4.zero);
			}
		}

		public void Filters(ref BlazePoseDetecter pipeline, 
							float param1, float param2,
							WhichBufferToFilter whichBufferToFilter = WhichBufferToFilter.Both, 
							LandmarksToFilter landmarksToFilter = LandmarksToFilter.FullBody, 
							DataFilters.FilterType filtertype = DataFilters.FilterType.OneEuro)
		{
			if (whichBufferToFilter == WhichBufferToFilter.None) return;

			int[] indices;
			switch (landmarksToFilter)
			{
				default:
				case LandmarksToFilter.FullBody: indices = BlazePoseLandmarkModel.FullBody; break;
				case LandmarksToFilter.UpperBody: indices = BlazePoseLandmarkModel.UpperBody; break;
				case LandmarksToFilter.Hands: indices = BlazePoseLandmarkModel.Hands; break;
			}

			//filter data
			if (whichBufferToFilter == WhichBufferToFilter.PoseLandmarks || whichBufferToFilter == WhichBufferToFilter.Both)
			{
				FilterList(ref pipeline, WhichBufferToFilter.PoseLandmarks, filtertype, param1, param2, ref _poseLandmarksFilters, ref _poseLandmarksFiltered, indices);
			}

			//filter data
			if (whichBufferToFilter == WhichBufferToFilter.PoseWorldLandmarks || whichBufferToFilter == WhichBufferToFilter.Both)
			{
				FilterList(ref pipeline, WhichBufferToFilter.PoseWorldLandmarks, filtertype, param1, param2, ref _poseWorldLandmarksFilters, ref _poseWorldLandmarksFiltered, indices);
			}
		}
        #endregion

        #region private methods
        private void FilterList(ref BlazePoseDetecter pipeline, WhichBufferToFilter whichBufferToFilter, DataFilters.FilterType filtertype, float param1, float param2, ref List<DataFilters> filtersList, ref List<Vector4> filteredDataList, int[] indicesToFilter)
		{
			foreach (int index in indicesToFilter)
			{
				Vector4 rawLandmark = whichBufferToFilter == WhichBufferToFilter.PoseLandmarks ? pipeline.GetPoseLandmark(index) : pipeline.GetPoseWorldLandmark(index);
				Vector3 rawLandmarkWithoutScoring = new Vector3(rawLandmark.x, rawLandmark.y, rawLandmark.z);
				Vector3 filteredLandmark = filtersList[index].Filter(rawLandmarkWithoutScoring, filtertype, param1, param2);
				filteredDataList[index] = new Vector4(filteredLandmark.x, filteredLandmark.y, filteredLandmark.z, rawLandmark.w);
			}
		}
        #endregion

        #region draw debug
        public void OnGUI(Texture ico, float icoSize)
        {
			foreach(Vector4 bone in _poseLandmarksFiltered)
			{
				Vector3 screenSpaceBone = Bonjour.MathsCoordUtils.RemapKeyPointScreenSpace(bone);
				GUI.DrawTexture(new Rect(screenSpaceBone.x - icoSize * .5f, screenSpaceBone.y - icoSize * .5f, icoSize, icoSize), ico);
			}
		}
        
		public void OnDrawGizmo(float gizmoSize)
        {
			foreach (Vector4 bone in _poseWorldLandmarksFiltered)
			{
				Gizmos.color = bone.w > .5f ? Color.green : Color.red;
				Gizmos.DrawSphere(new Vector3(bone.x, bone.y, bone.z), gizmoSize);
			}
		}
		#endregion
    }
}