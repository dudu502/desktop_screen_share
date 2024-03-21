

using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Recycling;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lenovo.VRX.Sample
{
    [ExcludeFromCoverage]
    public class MyListData
    {
        public string value;
        public Color bgColor = new Color(0.4f, 0, 0.6f);

        public bool isSelectd;
        public MyListData(string v) { value = v; }
    }
    [ExcludeFromCoverage]
    public class MyFlexibleData : IFlexible
    {
        public float height { set; get; }
        public Rect rect { set; get; }
        public string value;
        public int count;
        public bool isOverlaps { set; get; }
        public MyFlexibleData(string v)
        {
            height = 100;
            value = v;
            count = UnityEngine.Random.Range(1, 100);
        }
        public override string ToString()
        {
            return $"{value} h {height} c {count}";
        }
    }
    [ExcludeFromCoverage]
    public class ListPanelForTest : MonoBehaviour
    {
        public RecyclingListRenderer recyclingListRenderer;
        public RecyclingListRenderer recyclingListRenderer2;
        public FlexibleSingleColumnListRenderer flexibleRenderer;
        void Start()
        {
            recyclingListRenderer.InitRendererList(OnActionHandler);
            var list = new List<MyListData>();
            recyclingListRenderer.SetDataProvider(list);

            for (int i = 0; i < 300; ++i)
            {
                list.Add(new MyListData("a_" + i));
            }
            recyclingListRenderer.RefreshDataProvider();


            recyclingListRenderer2.InitRendererList(OnActionHandler2);
            var list2 = new List<MyListData>();
            recyclingListRenderer2.SetDataProvider(list2);
            for (int i = 0; i < 1000; ++i)
            {
                list2.Add(new MyListData("b_" + i));
            }
            recyclingListRenderer2.RefreshDataProvider();



            flexibleRenderer.InitRendererList(OnActionHandler3);

            var list3 = new List<IFlexible>(); 
            flexibleRenderer.SetDataProvider(list3);
            for (int i = 0; i < 150; ++i)
            {
                list3.Add(new MyFlexibleData(""+i));
            }
            flexibleRenderer.RefreshDataProvider();

        }

        void OnActionHandler(RecyclingItem.RecyclingEvent evt)
        {         
            MyListData myListData = evt.Target as MyListData;
            foreach (MyListData data in recyclingListRenderer.GetDataProvider())
                data.isSelectd = data == myListData;
            myListData.bgColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            recyclingListRenderer.RefreshDataProvider();
        }

        void OnActionHandler2(RecyclingItem.RecyclingEvent evt)
        {
            MyListData myListData = evt.Target as MyListData;
            myListData.isSelectd = !myListData.isSelectd;
            recyclingListRenderer2.RefreshDataProvider();
        }

        void OnActionHandler3(FlexibleSingleColumnItem.Event evt)
        {
            if (evt.Type == "Change")
            {
                Debug.LogWarning("OnActionHandler3 Change");
                evt.Target.height = Random.Range(100, 200);
            }
            else if(evt.Type == "Close")
            {
                flexibleRenderer.GetDataProvider().Remove(evt.Target);
            }
            flexibleRenderer.RefreshDataProvider();
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                recyclingListRenderer.RefreshDataProvider();
                foreach (IFlexible item in flexibleRenderer.GetDataProvider())
                    item.height++;
                flexibleRenderer.RefreshDataProvider();  
            }
            else if(Input.GetKeyDown(KeyCode.A))
            {
                recyclingListRenderer.GetDataProvider().Add(new MyListData("v"));
                recyclingListRenderer.RefreshDataProvider();
                flexibleRenderer.GetDataProvider().Add(new MyFlexibleData("add"+Random.Range(1,100)));
                flexibleRenderer.RefreshDataProvider();
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                if (flexibleRenderer.GetDataProvider().Count > 0)
                {
                    flexibleRenderer.GetDataProvider().RemoveAt(0);
                    flexibleRenderer.RefreshDataProvider();
                }             
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                flexibleRenderer.SetDataProvider(new List<IFlexible>());
                flexibleRenderer.RefreshDataProvider();
            }
        }

    }
}