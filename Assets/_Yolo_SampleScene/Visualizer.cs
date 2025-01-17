using Bonjour;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace YoloV4Tiny
{ 
    public class Visualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] RenderTexture _source = null;
        [SerializeField, Range(0, 1)] float _threshold = 0.5f;
        [SerializeField] ResourceSet _resources = null;

        [Header("Debug")]
        [SerializeField] bool debug = true;
        [SerializeField] RawImage _preview = null;
        [SerializeField] Marker _markerPrefab = null;


        #endregion

        #region Internal objects

        ObjectDetector _detector;
        Marker[] _markers = new Marker[50];

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _detector = new ObjectDetector(_resources);
            for (var i = 0; i < _markers.Length; i++)
                _markers[i] = Instantiate(_markerPrefab, _preview.transform);
        }

        void OnDisable()
        {
            _detector.Dispose();
        }

        void OnDestroy()
        {
            for (var i = 0; i < _markers.Length; i++) Destroy(_markers[i]);
        }

        void Update()
        {
            _detector.ProcessImage(_source, _threshold);

            if(debug) UpdateMarkerInUI();

            _preview.texture = _source;
        }

        private void UpdateMarkerInUI()
        {

            var i = 0;
            foreach (var d in _detector.Detections)
            {
                if (i == _markers.Length) break;
                _markers[i++].SetAttributes(d);
            }

            for (; i < _markers.Length; i++) _markers[i].Hide();
        }

        private void OnGUI()
        {
            //! each detected object is identified in the _detector.Detections array of Detection stuct with the following params
            /* float x, y, w, h; (position and size)
                uint classIndex; type of object
                float score; score for the object

                the class are : 
                0: Plane
                1: Bicycle
                1: Bird
                3 Boat
                4: Bottle
                5: Bus
                6: Car
                7: Cat
                8: Chair
                9: Cow
                10: Table
                11: Dog
                12: Horse
                13: Motorbike
                14: Person
                15: Plant
                16: Sheep
                17: Sofa
                18: Train
                19: TV
                */
            if (debug)
            {
                List<Detection> detections = _detector.Detections.ToList();
                for (int i = 0; i < detections.Count; i++)
                {
                    var detection = detections[i];
                    Vector2 normalizedPosition = new Vector2(detection.x, detection.y);
                    Vector2 position = normalizedPosition * new Vector2(Screen.width, Screen.height);
                    Vector2 size = new Vector2(detection.w, detection.h);
                    uint classIndex = detection.classIndex;
                    float score = detection.score * 100;

                    Debug.Log(position);
                    if(classIndex == 14)//we will only visualize Human
                    {
                        GUI.Label(new Rect(position.x, position.y, 100, 20), $"<color=#ffffff>I am only Human at : {score}%</color>", new GUIStyle()
                        {
                            fontSize = 30,
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.MiddleCenter,
                        });
                    }
                    
                }
            }
        }

            #endregion
    }
}
