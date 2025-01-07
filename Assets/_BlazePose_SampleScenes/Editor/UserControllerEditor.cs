using UnityEngine;
using UnityEditor;
using System.Collections;
using Mediapipe.BlazePose;
using Bonjour.Maths;

namespace Bonjour
{
    [CustomEditor(typeof(UserController), true), CanEditMultipleObjects]
    public class UserControllerEditor : Editor
    {
        #region MediaPipe pipeline params
        protected SerializedProperty input;
        protected SerializedProperty blazePoseModel;
        protected SerializedProperty poseThreshold;
        protected SerializedProperty humanScoreThreshold;
        protected SerializedProperty drawDebugBones;
        protected SerializedProperty drawDebugScore;
        protected SerializedProperty bonesIco;
        protected SerializedProperty debugBoneIcoSize;
        #endregion

        #region Filtering data param
        protected SerializedProperty whichBufferToFilter;
        protected SerializedProperty landmarksToFilter;
        protected SerializedProperty filtertype;

        protected SerializedProperty Q;
        protected SerializedProperty R;
        protected SerializedProperty beta;
        protected SerializedProperty minCutOff;

        protected SerializedProperty drawDebugFilteredBones;
        protected SerializedProperty bonesFilteredIco;
        protected SerializedProperty debugFilteredBoneIcoSize;
        protected SerializedProperty debugFilteredBone3DSize;
        #endregion

        #region Timer Params
        protected SerializedProperty timeout;
        protected SerializedProperty logTimerEvents;
        #endregion

        protected virtual void OnEnable()
        {
            input               = serializedObject.FindProperty("input");
            blazePoseModel      = serializedObject.FindProperty("blazePoseModel");
            poseThreshold       = serializedObject.FindProperty("poseThreshold");
            humanScoreThreshold = serializedObject.FindProperty("humanScoreThreshold");
            drawDebugBones      = serializedObject.FindProperty("drawDebugBones");
            drawDebugScore      = serializedObject.FindProperty("drawDebugScore");
            bonesIco            = serializedObject.FindProperty("bonesIco");
            debugBoneIcoSize    = serializedObject.FindProperty("debugBoneIcoSize");

            whichBufferToFilter      = serializedObject.FindProperty("whichBufferToFilter");
            landmarksToFilter        = serializedObject.FindProperty("landmarksToFilter");
            filtertype               = serializedObject.FindProperty("filtertype");
            Q                        = serializedObject.FindProperty("Q");
            R                        = serializedObject.FindProperty("R");
            beta                     = serializedObject.FindProperty("beta");
            minCutOff                = serializedObject.FindProperty("minCutOff");
            drawDebugFilteredBones   = serializedObject.FindProperty("drawDebugFilteredBones");
            bonesFilteredIco         = serializedObject.FindProperty("bonesFilteredIco");
            debugFilteredBoneIcoSize = serializedObject.FindProperty("debugFilteredBoneIcoSize");
            debugFilteredBone3DSize  = serializedObject.FindProperty("debugFilteredBone3DSize");

            timeout        = serializedObject.FindProperty("timeout");
            logTimerEvents = serializedObject.FindProperty("logTimerEvents");
        }

        public override void OnInspectorGUI()
        {
            
            //EditorGUILayout.LabelField("Media Pipe Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(input);
            EditorGUILayout.PropertyField(blazePoseModel);
            EditorGUILayout.PropertyField(poseThreshold);
            EditorGUILayout.PropertyField(humanScoreThreshold);
            EditorGUILayout.PropertyField(drawDebugBones);
            EditorGUILayout.PropertyField(drawDebugScore);
            EditorGUILayout.PropertyField(bonesIco);
            EditorGUILayout.PropertyField(debugBoneIcoSize);

            //EditorGUILayout.LabelField("Landmarks Filtering Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(whichBufferToFilter);

            int filterindex = whichBufferToFilter.enumValueIndex;
            BlazePoseLandmarkFilters.WhichBufferToFilter filteraction = (BlazePoseLandmarkFilters.WhichBufferToFilter)filterindex;

            //Draws only the appropriate information based on the Action Type
            if (filteraction != BlazePoseLandmarkFilters.WhichBufferToFilter.None)
            {
                EditorGUILayout.PropertyField(landmarksToFilter);

                EditorGUILayout.PropertyField(filtertype);
                int filtertypeindex = filtertype.enumValueIndex;
                DataFilters.FilterType filtertypeaction = (DataFilters.FilterType)filtertypeindex;

                switch (filtertypeaction)
                {
                    case DataFilters.FilterType.Kalman:
                        EditorGUILayout.PropertyField(Q);
                        EditorGUILayout.PropertyField(R);
                        break;

                    case DataFilters.FilterType.OneEuro:
                        EditorGUILayout.PropertyField(beta);
                        EditorGUILayout.PropertyField(minCutOff); ;
                        break;
                }

                EditorGUILayout.PropertyField(drawDebugFilteredBones);
                if (filteraction == BlazePoseLandmarkFilters.WhichBufferToFilter.Both || filteraction == BlazePoseLandmarkFilters.WhichBufferToFilter.PoseLandmarks)
                {
                    EditorGUILayout.PropertyField(bonesFilteredIco);
                    EditorGUILayout.PropertyField(debugFilteredBoneIcoSize);
                }

                if (filteraction == BlazePoseLandmarkFilters.WhichBufferToFilter.Both || filteraction == BlazePoseLandmarkFilters.WhichBufferToFilter.PoseWorldLandmarks)
                {
                    EditorGUILayout.PropertyField(debugFilteredBone3DSize);
                }
            }

            //EditorGUILayout.LabelField("User timer Check", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(timeout);
            EditorGUILayout.PropertyField(logTimerEvents);
            
            //Update UI
            serializedObject.ApplyModifiedProperties();
        }
    }
}

