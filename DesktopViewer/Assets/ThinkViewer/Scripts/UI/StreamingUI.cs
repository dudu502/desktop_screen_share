using Development.Net.Pt;
using Net;
using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Common;
using Think.Viewer.FSM;
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
        private DataModule dataModule;
        private PointerEventData pointerEventData;
        public InputActionReference shiftButtonAction, selectButtonAction,triggerButtonAction;
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
            Debug.LogError("Point move" + eventData.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_MOVE).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftMouse, op);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.LogError("Point up" + eventData.position);
            pointerEventData = eventData;
        }
        void SendOp(C2S pid, PtStreamingOp op)
        {
            var currentRemoteEndPoint = ModuleManager.GetModule<DataModule>().CurrentEndPoint;
            if (currentRemoteEndPoint != null)
                GameClientNetwork.Instance.SendUnconnectedRequest(PtMessagePackage.Build((ushort)pid,PtStreamingOp.Write(op)), currentRemoteEndPoint);
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
            rawImage.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);

            shiftButtonAction.action.performed += OnShiftButtonPerformed;
            selectButtonAction.action.performed += OnSelectButtonPerformed;
            //triggerButtonAction.action.performed += OnTriggerButtonPerformed;
            triggerButtonAction.action.started += OnTriggerButtonStarted;
            triggerButtonAction.action.canceled += OnTriggerButtonCanceled;

        }

        void OnShiftButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogWarning("OnShiftButtonPerformed " + context.ToString());
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_RIGHT_CLICK).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpRightMouse, op);
           
            }
        }
        void OnSelectButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogWarning("OnSelectButtonPerformed " + context.ToString());

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_DOUBLE_CLICK).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftMouse, op);
              
            }
        }
        void OnTriggerButtonStarted(InputAction.CallbackContext context)
        {
            Debug.LogWarning("OnTriggerButtonStarted " + context.ToString());
            //left click
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_LEFT_DOWN).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftMouse, op);
            }
        }
        void OnTriggerButtonCanceled(InputAction.CallbackContext context)
        {
            Debug.LogWarning("OnTriggerButtonCanceled " + context.ToString());
            //left click
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_LEFT_UP).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftMouse, op);
                
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_LEFT_CLICK).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftMouse, op);
              
            }
        }
        void OnTriggerButtonPerformed(InputAction.CallbackContext context)
        {
            Debug.LogWarning("OnTriggerButtonPerformed " + context.ToString());
            //left click
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, pointerEventData.position, Camera.main, out var local))
            {
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_LEFT_CLICK).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftMouse, op);
                
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
