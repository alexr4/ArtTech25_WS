using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Bonjour.Time;

namespace Bonjour
{
    public class BlazePoseInputModule : StandaloneInputModule
    {

        [Space]
        public UserController.HandOrientation activeHand = UserController.HandOrientation.Right;

        [Header("Hover intent params")]
        public int timeout = 3;
        protected Timer hoverIntentTimer;
        protected float hoverIntentCompletion;

        [Header("Cursor")]
        public RectTransform cursor;
        private Image cursorImage;
        public RectTransform canvas;
        public bool showCursor;
        [Tooltip("To avoid avoid the cursos glitching (appearing/disapperaing) due to lost user we can use a history of user track and check the average over a certain number of frames")]
        public int historySize = 60 * 3;
        private List<int> hasUserList = new List<int>(); //HasUser bool are stroed in the array as int 0:false 1:true;

        
        protected override void Awake()
        {
            base.Awake();
            hoverIntentTimer = new Timer("Hover intent", timeout);
            cursorImage = cursor.GetComponent<Image>();
            cursorImage.material.SetFloat("_Completion", 0f);

            InitHistoryList();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            hoverIntentTimer.OnTimerStart.AddListener(OnHoverIntentStart);
            hoverIntentTimer.OnTimerUpdated.AddListener(OnHoverIntentUpdate);
            hoverIntentTimer.OnTimerEnd.AddListener(OnHoverIntentEnd);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            hoverIntentTimer.OnTimerStart.RemoveListener(OnHoverIntentStart);
            hoverIntentTimer.OnTimerUpdated.RemoveListener(OnHoverIntentUpdate);
            hoverIntentTimer.OnTimerEnd.RemoveListener(OnHoverIntentEnd);
        }
        

         private void Update()
         {
            if (showCursor)
            {
                AddUserToHistory(UserController.Instance.HasUser());
                bool display = hasUserAverage();
                cursor.gameObject.SetActive(display);
            }

            // check if cursos need to be shown
            if (UserController.Instance.HasUser()) { 

                //todo: how to set active hand ? Is it auto ?

                //Get Hand position and convert to Recttransform Space to move cursor
                // ! Remember MediaPipe landmark are in Mirror space so HandRight = HandLeft on MediaPipe Model
                Vector2 handScreenPosition = UserController.Instance.whichBufferToFilter == Mediapipe.BlazePose.BlazePoseLandmarkFilters.WhichBufferToFilter.None ? UserController.Instance.GetHandScreenPosition(activeHand) : UserController.Instance.GetFilteredHandScreenPosition(activeHand);
                handScreenPosition.y = Screen.height - handScreenPosition.y; //invert y axis on screen pos

                cursor.anchoredPosition = handScreenPosition;
                MoveCursorToScreenPos(handScreenPosition);

                //Grab Pointer Event Data and process raycast with button
                PointerEventData l_data = new PointerEventData(EventSystem.current);
                l_data.position = new Vector2(handScreenPosition.x, handScreenPosition.y);

                List<RaycastResult> results = new List<RaycastResult>();
                List<GameObject> btnsList = new List<GameObject>();
                EventSystem.current.RaycastAll(l_data, results);

                if (results.Count > 0)
                {
                    //There is an hit, /grab obly btn in hit list
                    foreach(RaycastResult result in results)
                    {
                        if (result.gameObject.GetComponent<Button>() != null && result.gameObject.GetComponent<Button>().interactable)
                            btnsList.Add(result.gameObject);
                    }
                }

                //Start/stop hoçver intent here
                if (btnsList.Count > 0) // || results.Count > 0)
                {
                    hoverIntentTimer.StartTimer();
                }
                else
                {
                    hoverIntentTimer.StopTimer();
                }
            }
        }
       
        public void MoveCursorToScreenPos(Vector2 position)
        {
            var pointerData = GetTouchPointerEventData(new Touch()
            {
                position = new Vector2(position.x, position.y),
            }, out bool b, out bool bb);

            ProcessMove(pointerData);
            ProcessMouseEvent();
        }

        public void ClickAt(Vector2 target)
        {
            Input.simulateMouseWithTouches = true;
            var pointerData = GetTouchPointerEventData(new Touch()
            {
                position = new Vector2(target.x, target.y),
            }, out bool b, out bool bb);

            ProcessTouchPress(pointerData, true, true);
        }

        #region Hover Intent
        protected void OnHoverIntentStart(TimerData timer)
        {
            //start Hover intent
            hoverIntentCompletion = 0f;
        }

        protected void OnHoverIntentUpdate(TimerData timer)
        {
            //Update Hover Intent
            hoverIntentCompletion = timer.normalizedTime;
            //todo: Do the cursor animation here (could be updating a text, changing color or using a loading bar)
            cursorImage.material.SetFloat("_Completion", hoverIntentCompletion);
        }

        protected void OnHoverIntentEnd(TimerData timer)
        {
            cursorImage.material.SetFloat("_Completion", 0f);
            if (hoverIntentCompletion >= 1f)
            {
                ClickAt(cursor.anchoredPosition);
            }
        }
        #endregion

        #region hasUserHistory
        private void InitHistoryList()
        {
            if (historySize == 0) return;

            for(int i = 0; i<historySize; i++)
            {
                hasUserList.Add(0);
            }
        }

        private void AddUserToHistory(bool hasUser)
        {
            if (historySize == 0) return;

            hasUserList.Add(hasUser ? 1 : 0);

            if(hasUserList.Count > historySize)
                hasUserList.RemoveAt(0);
        }

        private bool hasUserAverage()
        {
            if (historySize == 0) return UserController.Instance.HasUser();

            int average = 0;
            foreach(int hasUserFrames in hasUserList)
            {
                average += hasUserFrames;
            }

            if(average > historySize / 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
