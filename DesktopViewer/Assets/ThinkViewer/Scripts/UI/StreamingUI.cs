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
        private StateMachine<StreamingUI> streamingOpFSM;
        private List<EventArgs> fsmEvents;
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogError("Point Click"+eventData.position);
            var evt = new EventArgs(POINTER_CLICK, eventData.position);
            fsmEvents.Add(evt);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, eventData.position, Camera.main, out var local))
            {
                local.x += 1120;
                local.y += 700;
                PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_LEFT_DOWN).SetPosition(ToPtVec2(local));
                SendOp(C2S.StreamingOpLeftClick, op);
                Debug.LogError("Point Click Local " + local);
            }


            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.LogError("Point down" + eventData.position);
            fsmEvents.Add(new EventArgs(POINTER_DOWN, eventData.position));
            //PtStreamingOp op = new PtStreamingOp().SetOpType(Const.STREAMING_OP_TYPE_LEFT_DOWN).SetPosition(ToPtVec2(evt.ParameterAs<Vector2>()));
            //SendOp(C2S.StreamingOpLeftClick, op);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            fsmEvents.Add(new EventArgs(POINTER_MOVE, eventData.position));
            //Debug.LogError("Point move" + eventData.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.transform as RectTransform, eventData.position, Camera.main, out var local))
            {
                local.x += 1120;
                local.y += 700;
                //Debug.LogError("Point move local " + local);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.LogError("Point up" + eventData.position);
            fsmEvents.Add(new EventArgs(POINTER_UP, eventData.position));
        }
        void SendOp(C2S pid, PtStreamingOp op)
        {
            var currentRemoteEndPoint = ModuleManager.GetModule<DataModule>().CurrentEndPoint;
            if (currentRemoteEndPoint != null)
                GameClientNetwork.Instance.SendUnconnectedRequest(PtMessagePackage.Build((ushort)pid,PtStreamingOp.Write(op)), currentRemoteEndPoint);
        }

        PtVec2 ToPtVec2(Vector2 vec)
        {
            return new PtVec2().SetX((int)vec.x).SetY((int)vec.y);
        }
        void Start()
        {
            dataModule = ModuleManager.GetModule<DataModule>();
            rawImage.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);
            streamingOpFSM = new StateMachine<StreamingUI>(this)
                .State(OpState.Idle)
                .End()
                .State(OpState.Click)
                .End()
                .State(OpState.Down)
                .End()
                .State(OpState.Up)
                .End()
                .State(OpState.DoubleClick)
                .End()
                .State(OpState.Move)
                .End().SetDefault(OpState.Idle).Build();
            fsmEvents = streamingOpFSM.GetEventArgs();
        }

        // Update is called once per frame
        void Update()
        {
            streamingOpFSM.Update();
            while(dataModule!=null&& dataModule.StreamingRawFrameQueue!=null&&dataModule.StreamingRawFrameQueue.TryDequeue(out byte[] raw))
            {
                ((Texture2D)rawImage.texture).LoadImage(raw);
            }
        }
    }
}
