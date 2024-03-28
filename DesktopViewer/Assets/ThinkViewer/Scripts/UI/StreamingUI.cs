using Development.Net.Pt;
using Net;
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
        public const string POINTER_CLICK = "POINTER_CLICK";
        public const string POINTER_DOWN = "POINTER_DOWN";
        public const string POINTER_UP = "POINTER_UP";
        public const string POINTER_MOVE = "POINTER_MOVE";
        enum OpState
        {
            Idle,
            Click,
            Down,
            Up,
            DoubleClick,
            Move,
        }
        public RawImage rawImage;
        public Slider qualitySlider;
        private DataModule dataModule;
        private PointerEventData pointerEventData;
        public InputActionReference shiftButtonAction, selectButtonAction,triggerButtonAction;
        private Rect screenRect;
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
     
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                var movePos = ToPtVec2(local);
                if (movePos.X < screenRect.x || movePos.X >= (screenRect.x + screenRect.width)) return;
                if (movePos.Y < screenRect.y || movePos.Y >= (screenRect.y + screenRect.height)) return;
                Debug.LogError("Point Move" + movePos.X+" "+ movePos.Y);
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_DOWN).SetPosition(movePos);
                GameClientNetwork.Instance.SendUnconnectedRequest(C2S.StreamingOpLeftMouse, PtStreamingOp.Write(op));
            }
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
   
        void Start()
        {
            dataModule = ModuleManager.GetModule<DataModule>();
            screenRect = new Rect(dataModule.HostSetting.CaptureX, dataModule.HostSetting.CaptureY, dataModule.HostSetting.CaptureWidth, dataModule.HostSetting.CaptureHeight);
            rawImage.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);

            shiftButtonAction.action.performed += OnShiftButtonPerformed;
            selectButtonAction.action.performed += OnSelectButtonPerformed;
            triggerButtonAction.action.performed += OnTriggerButtonPerformed;
            triggerButtonAction.action.started += OnTriggerButtonStarted;
            triggerButtonAction.action.canceled += OnTriggerButtonCanceled;

            qualitySlider.value = Convert.ToInt32(dataModule.HostSetting.StreamingQuality);
            qualitySlider.onValueChanged.AddListener(OnQualitySliderChanged);
        }
        void OnQualitySliderChanged(float value)
        {
            //qualitySlider.value = value;
            Debug.LogWarning("OnQualitySliderChanged" + value);
            GameClientNetwork.Instance.SendUnconnectedRequest(C2S.ChangeQuality, Application.UUID, (int)value);
        }
        /// <summary>
        /// Shift Click(Right Click)
        /// </summary>
        /// <param name="context"></param>
        void OnShiftButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogWarning("StreamingUI OnShiftButtonPerformed Shift Click(Right Click) " + context.ToString());
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_CLICK).SetPosition(ToPtVec2(local));
                GameClientNetwork.Instance.SendUnconnectedRequest(C2S.StreamingOpRightMouse, PtStreamingOp.Write(op));
            }
        }
        /// <summary>
        /// Double Click
        /// </summary>
        /// <param name="context"></param>
        void OnSelectButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogWarning("StreamingUI OnSelectButtonPerformed Double Click " + context.ToString());

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_DOUBLE_CLICK).SetPosition(ToPtVec2(local));
                GameClientNetwork.Instance.SendUnconnectedRequest(C2S.StreamingOpLeftMouse, PtStreamingOp.Write(op));
            }
        }

        /// <summary>
        /// Down
        /// </summary>
        /// <param name="context"></param>
        void OnTriggerButtonStarted(InputAction.CallbackContext context)
        {
            Debug.LogWarning("StreamingUI OnTriggerButtonStarted Down " + context.ToString());
            //left click
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_DOWN).SetPosition(ToPtVec2(local));
                GameClientNetwork.Instance.SendUnconnectedRequest(C2S.StreamingOpLeftMouse, PtStreamingOp.Write(op));
            }
        }

        /// <summary>
        /// Up
        /// </summary>
        /// <param name="context"></param>
        void OnTriggerButtonCanceled(InputAction.CallbackContext context)
        {
            Debug.LogWarning("StreamingUI OnTriggerButtonCanceled Up " + context.ToString());
            //left click
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_UP).SetPosition(ToPtVec2(local));
                GameClientNetwork.Instance.SendUnconnectedRequest(C2S.StreamingOpLeftMouse, PtStreamingOp.Write(op));
            }
        }
        /// <summary>
        /// Click
        /// </summary>
        /// <param name="context"></param>
        void OnTriggerButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogWarning("StreamingUI OnTriggerButtonPerformed Click " + context.ToString());
            //left click
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_CLICK).SetPosition(ToPtVec2(local));
                GameClientNetwork.Instance.SendUnconnectedRequest(C2S.StreamingOpLeftMouse, PtStreamingOp.Write(op));
            }
        }
        // Update is called once per frame
        void Update()
        {
            while(dataModule!=null&& dataModule.StreamingRawFrameQueue!=null&&dataModule.StreamingRawFrameQueue.TryDequeue(out byte[] raw))
            {
                ((Texture2D)rawImage.texture).LoadImage(raw);
            }
        }
    }
}
