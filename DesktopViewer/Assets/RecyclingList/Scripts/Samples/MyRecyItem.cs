

using Lenovo.VRX.Sample;
using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Recycling;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Think.Viewer.Sample
{
    [ExcludeFromCoverage]
    public class MyRecyItem : RecyclingItem
    {
        public Button MyBtn;
        public TMPro.TMP_Text MyText;
        public Image MyBg;
        public GameObject SelectedGo;
        // Start is called before the first frame update
        void Start()
        {
            MyBtn.onClick.AddListener(() =>
            {
                OnActionHandler?.Invoke(new RecyclingEvent()
                {
                    Type = "PLUS",
                    Target = GetData(),
                });
            });
        }
        protected override void OnRenderer()
        {
            base.OnRenderer();
            var item = GetData<MyListData>();
            MyText.text = item.value;
            MyBg.color = item.bgColor;
            SelectedGo.SetActive(item.isSelectd);
        }
    }
}