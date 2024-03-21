using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Module;
using UnityEngine;
using UnityEngine.UI;

namespace Think.Viewer.UI
{
    public class StreamingUI : UIView
    {
        public RawImage rawImage;
        void Start()
        {
            rawImage.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
