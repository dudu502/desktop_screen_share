using Development.Net.Pt;
using Net;
using Switch.Structure.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Common;
using Think.Viewer.Manager;
using Think.Viewer.Module;
using Think.Viewer.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Think.Viewer.UI
{
    public class StreamingUI : UIView, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        public const string INPUT_EVENT_DOWN = "INPUT_EVENT_DOWN";
        public const string INPUT_EVENT_UP = "INPUT_EVENT_UP";
        enum InputState
        {
            Idle,
            Click,
            Up
        }
        public RawImage rawImage;
        public Slider qualitySlider;
        private DataModule dataModule;
        private PointerEventData pointerEventData;
        public InputActionReference shiftButtonAction, selectButtonAction,triggerButtonAction;
        private Rect screenRect;
        private StateMachine<StreamingUI> inputFsm;
        private Timer timer;
        private List<Switch.Structure.FSM.EventArgs> fsmEvents;
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogError("Point Click"+eventData.position);
            pointerEventData = eventData;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.LogError("Point down" + eventData.position);
            pointerEventData = eventData;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            pointerEventData = eventData;
            SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_MOVE);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.LogError("Point up" + eventData.position);
            pointerEventData = eventData;
        }
        

        PtVec2 ToPtVec2(Vector2 vec)
        {
            vec.x += dataModule.HostSetting.CaptureWidth / 2;
            vec.y += dataModule.HostSetting.CaptureHeight / 2;
            return new PtVec2().SetX((int)vec.x).SetY((int)vec.y);
        }
        bool isKeyDown = false;
        void Start()
        {
            dataModule = ModuleManager.GetModule<DataModule>();
            screenRect = new Rect(dataModule.HostSetting.CaptureX, dataModule.HostSetting.CaptureY, dataModule.HostSetting.CaptureWidth, dataModule.HostSetting.CaptureHeight);
            rawImage.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);

            shiftButtonAction.action.performed += OnShiftButtonPerformed;
            selectButtonAction.action.performed += OnSelectButtonPerformed;
            //triggerButtonAction.action.performed += OnTriggerButtonPerformed;
            triggerButtonAction.action.started += OnTriggerButtonStarted;
            triggerButtonAction.action.canceled += OnTriggerButtonCanceled;

            qualitySlider.value = Convert.ToInt32(dataModule.HostSetting.StreamingQuality);
            qualitySlider.onValueChanged.AddListener(OnQualitySliderChanged);
            timer = new Timer();
            //StateMachineDebug.Filter = StateMachineDebug.LogFilter.Everything;
            //StateMachineDebug.Log = Debug.Log;
            inputFsm = new StateMachine<StreamingUI>(this)
                .State(InputState.Idle)
                    .EarlyUpdate(so =>
                    {
                        if (Switch.Structure.FSM.EventArgs.Poll(fsmEvents, INPUT_EVENT_DOWN))
                        {
                            so.timer.Reset();
                            isKeyDown = true;
                        }
                    })
                    .Transition(so=> Switch.Structure.FSM.EventArgs.Poll(fsmEvents, INPUT_EVENT_UP) && so.timer<0.15f).To(InputState.Click).End()
                    .Transition(so=> isKeyDown && so.timer>=0.15f).To(InputState.Up).End()
                .End()
                .State(InputState.Click)
                    .Enter(so=> { SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_CLICK); Debug.LogWarning("FSM OP_CLICK"); isKeyDown = false; })
                    .Transition(so=>true).To(InputState.Idle).Transfer(so => fsmEvents.Clear()).End()
                .End()
                .State(InputState.Up)
                    .Enter(so=> { SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_DOWN); Debug.LogWarning("FSM OP_DOWN"); isKeyDown = false; })
                    .Exit(so => { SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_UP);Debug.LogWarning("FSM OP_UP"); })
                    .Transition(so=>Switch.Structure.FSM.EventArgs.Poll(fsmEvents,INPUT_EVENT_UP)).To(InputState.Idle).Transfer(so=> fsmEvents.Clear()).End()
                .End()
                .SetDefault(InputState.Idle).Build();
            fsmEvents = inputFsm.GetEventArgs();
        }
        void OnQualitySliderChanged(float value)
        {
            //qualitySlider.value = value;
            Debug.LogError("OnQualitySliderChanged" + value);
            GameClientNetwork.Instance.SendUnconnectedRequest(C2S.ChangeQuality, Application.UUID, (int)value);
        }
        /// <summary>
        /// Shift Click(Right Click)
        /// </summary>
        /// <param name="context"></param>
        void OnShiftButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogError("StreamingUI OnShiftButtonPerformed Shift Click(Right Click) " + context.ToString());
            SendInputOp(C2S.StreamingOpRightMouse, Const.STREAMING_OP_CLICK);
        }
        /// <summary>
        /// Double Click
        /// </summary>
        /// <param name="context"></param>
        void OnSelectButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogError("StreamingUI OnSelectButtonPerformed Double Click " + context.ToString());
            SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_DOUBLE_CLICK);
        }

        /// <summary>
        /// Down
        /// </summary>
        /// <param name="context"></param>
        void OnTriggerButtonStarted(InputAction.CallbackContext context)
        {
            Debug.LogError("StreamingUI OnTriggerButtonStarted Down " );
            fsmEvents.Add(new Switch.Structure.FSM.EventArgs(INPUT_EVENT_DOWN,null));
            //left click
            //SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_DOWN);
        }

        /// <summary>
        /// Up
        /// </summary>
        /// <param name="context"></param>
        void OnTriggerButtonCanceled(InputAction.CallbackContext context)
        {
            Debug.LogError("StreamingUI OnTriggerButtonCanceled Up " );
            fsmEvents.Add(new Switch.Structure.FSM.EventArgs(INPUT_EVENT_UP, null));
            //SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_UP);
        }
        /// <summary>
        /// Click
        /// </summary>
        /// <param name="context"></param>
        void OnTriggerButtonPerformed(InputAction.CallbackContext context)
        {
            //Debug.LogError("StreamingUI OnTriggerButtonPerformed Click " + context.ToString());
            //SendInputOp(C2S.StreamingOpLeftMouse, Const.STREAMING_OP_CLICK);
        }

        void SendInputOp(C2S pid,byte opType)
        {
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                var pos = ToPtVec2(local);
                if (pos.X < screenRect.x || pos.X >= (screenRect.x + screenRect.width)) return;
                if (pos.Y < screenRect.y || pos.Y >= (screenRect.y + screenRect.height)) return;
                PtStreamingOp op = new PtStreamingOp().SetOpType(opType).SetPosition(pos);
                GameClientNetwork.Instance.SendUnconnectedRequest(pid, PtStreamingOp.Write(op));
            }
        }
        
        // Update is called once per frame
        void Update()
        {
            inputFsm.Update();
            while (dataModule != null && dataModule.StreamingRawFrameQueue != null && dataModule.StreamingRawFrameQueue.TryDequeue(out byte[] raw))
            {
                ((Texture2D)rawImage.texture).LoadImage(raw);
            }

        }
    }
}
