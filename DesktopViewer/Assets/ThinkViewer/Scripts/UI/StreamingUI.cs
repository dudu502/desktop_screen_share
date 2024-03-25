using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Manager;
using Think.Viewer.Module;
using UnityEngine;
using UnityEngine.UI;

namespace Think.Viewer.UI
{
    public class StreamingUI : UIView
    {
        public RawImage rawImage;
        private DataModule dataModule;
        void Start()
        {
            dataModule = ModuleManager.GetModule<DataModule>();
            rawImage.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);
        }

        // Update is called once per frame
        void Update()
        {
            while(dataModule.StreamingRawFrameQueue.TryDequeue(out byte[] raw))
            {
                ((Texture2D)rawImage.texture).LoadImage(raw);
            }
        }
    }
}
