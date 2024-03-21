
using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Recycling;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Think.Viewer.Sample
{
    [ExcludeFromCoverage]
    public class MyFlexibleItem : FlexibleSingleColumnItem
    {
        public TMPro.TMP_Text _tmpText;
        public Button _button;
        public Button _close;
        // Start is called before the first frame update
        void Start()
        {
            _button.onClick.AddListener(() =>
            {
                OnEvent.Invoke(new Event()
                {
                    Type = "Change",
                    Target = Data,
                });
            });

            _close.onClick.AddListener(() =>
            {
                OnEvent.Invoke(new Event()
                {
                    Type = "Close",
                    Target = Data,
                });
            });
        }
        protected override void OnRenderer()
        {
            base.OnRenderer();
            _tmpText.text = Data.ToString();
        }
    }
}