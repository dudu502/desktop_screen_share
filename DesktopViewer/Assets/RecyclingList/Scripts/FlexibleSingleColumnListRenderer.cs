/*
* File: FlexibleSingleColumnListRenderer.cs
*
* Copyright(c) 2023 Lenovo, Inc. and/or its subsidiaries. All rights reserved.
*
* Confidential and Proprietary
*
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Think.Viewer.Recycling
{
    public class FlexibleSingleColumnListRenderer : MonoBehaviour
    {
        public float Spacing;
        public GameObject RenderGO;
        protected List<IFlexible> DataProviders;
        protected ScrollRect _scrollRect;
        private Vector2 _itemSize;
        private Vector2 _maskSize;
        private Rect _maskRect;
        private Queue<FlexibleSingleColumnItem> _flexibleItemQueue;
        private RectTransform _rectTransform;
        private Action<FlexibleSingleColumnItem.Event> _onEventHandler;
        private bool _inited;
        private readonly List<FlexibleSingleColumnItem> flexibleSingleColumnItems = new List<FlexibleSingleColumnItem>();
        public void InitRendererList(Action<FlexibleSingleColumnItem.Event> onEvtHandler)
        {
            _onEventHandler = onEvtHandler;
            _rectTransform = transform as RectTransform;
            _flexibleItemQueue = new Queue<FlexibleSingleColumnItem>();
            _itemSize = RenderGO.GetComponent<RectTransform>().sizeDelta;
            _maskSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
            _maskRect = new Rect(0, -_maskSize.y, _maskSize.x, _maskSize.y);
            _scrollRect = transform.parent.GetComponent<ScrollRect>();
            _inited = true;
        }

        private FlexibleSingleColumnItem Reuse()
        {
            if (_flexibleItemQueue.Count == 0)
            {
                GameObject go = Instantiate(RenderGO);
                go.transform.SetParent(transform, false);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.layer = gameObject.layer;
                _flexibleItemQueue.Enqueue(go.GetComponent<FlexibleSingleColumnItem>());
            }
            FlexibleSingleColumnItem item = _flexibleItemQueue.Dequeue();
            item.gameObject.SetActive(true);
            item.OnEvent = _onEventHandler;

            return item;
        }
        private void Recycle(FlexibleSingleColumnItem item)
        {
            if (item != null && item.Data != null && item.OnEvent != null)
            {
                item.gameObject.SetActive(false);
                ((RectTransform)item.transform).anchoredPosition3D = Vector3.zero;
                ((RectTransform)item.transform).anchoredPosition = new Vector2(float.MinValue, float.MinValue);
                item.Data = null;
                item.OnEvent = null;
                _flexibleItemQueue.Enqueue(item);
            }
        }

        public void SetDataProvider(List<IFlexible> datas)
        {
            DataProviders = datas;
            ResizeItemRect(datas);
            Update();
        }

        /// <summary>
        /// sum up all data's height
        /// and assign to container's height
        /// </summary>
        /// <param name="datas"></param>
        public void ResizeItemRect(List<IFlexible> datas)
        {
            float y = 0f;
            for (int i = 0; i < datas.Count; ++i)
            {
                y -= datas[i].height;
                datas[i].rect = new Rect(new Vector2(0, y), new Vector2(_itemSize.x, datas[i].height));
                y -= Spacing;
            }
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, Mathf.Abs(y += Spacing));
        }

        public void RefreshDataProvider()
        {
            if (DataProviders == null) throw new Exception("DataProviders is null .please call SetDataProvider first");
            ResizeItemRect(DataProviders);
            Update();
        }

        public List<IFlexible> GetDataProvider() { return DataProviders; }
     
        FlexibleSingleColumnItem FindDataInItems(List<FlexibleSingleColumnItem> items, IFlexible data)
        {
            foreach (FlexibleSingleColumnItem item in items)
            {
                if (item.Data == data)
                    return item;
            }
            return null;
        }
        void UpdateRenderer()
        {
            _maskRect.y = -_maskSize.y - _rectTransform.anchoredPosition.y;
            GetComponentsInChildren(true, flexibleSingleColumnItems);
            foreach (FlexibleSingleColumnItem item in flexibleSingleColumnItems)
            {
                if (item.Data != null && !DataProviders.Contains(item.Data))
                    Recycle(item);
            }

            foreach (IFlexible data in DataProviders)
            {
                data.isOverlaps = _maskRect.Overlaps(data.rect);
                if (!data.isOverlaps)
                    Recycle(FindDataInItems(flexibleSingleColumnItems, data));
            }

            foreach (IFlexible data in DataProviders)
            {
                if (data.isOverlaps)
                {
                    FlexibleSingleColumnItem item = FindDataInItems(flexibleSingleColumnItems, data) ?? Reuse();
                    UpdateChildTransformPos(item, data);
                    item.SetData(data);
                }
            }
        }
        void UpdateChildTransformPos(FlexibleSingleColumnItem child, IFlexible data)
        {
            ((RectTransform)child.transform).anchoredPosition3D = Vector3.zero;
            ((RectTransform)child.transform).anchoredPosition = new Vector2(data.rect.position.x, data.rect.position.y + data.height);
            ((RectTransform)child.transform).sizeDelta = new Vector2(data.rect.size.x, data.height);
        }


        void Update()
        {
            if (_inited && DataProviders != null)
            {
                UpdateRenderer();
            }
        }
    }
}