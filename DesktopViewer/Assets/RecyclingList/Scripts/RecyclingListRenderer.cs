/* Copyright (C) 2021 Lenovo */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.TestTools;

namespace Think.Viewer.Recycling
{
    [ExcludeFromCoverage]
    public class RecyclingItemRect
    {    
        private Rect _rect;
        public int Index;
        public RecyclingItemRect(float x, float y, float width, float height, int index)
        {
            Index = index;
            _rect = new Rect(x, y, width, height);
        }

        public bool Overlaps(RecyclingItemRect otherRect)
        {
            return _rect.Overlaps(otherRect._rect);
        }

        public bool Overlaps(Rect otherRect)
        {
            return _rect.Overlaps(otherRect);
        }
        public override string ToString()
        {
            return string.Format("index:{0},x:{1},y:{2},w:{3},h:{4}", Index, _rect.x, _rect.y, _rect.width, _rect.height);
        }
    }


    public class RecyclingListRenderer : MonoBehaviour
    {
        public int CurrentLocatedIndex = 0;

        public Vector2 CellSize;

        public Vector2 SpacingSize;

        public int ColumnCount;

        public GameObject RenderGO;

        [SerializeField] private float yPadding;

        protected int _rendererCount;

        private Vector2 _maskSize;

        private Rect _rectMask;
        protected ScrollRect _scrollRect;
  
        protected RectTransform _rectTransformContainer;

        protected List<RecyclingItem> _listItems;

        private Dictionary<int, RecyclingItemRect> _dictRect;

        protected IList DataProviders;
        protected bool HasInited = false;
        protected Coroutine m_Coroutine = null;

        private Dictionary<int, RecyclingItemRect> m_InOverlaps = new Dictionary<int, RecyclingItemRect>();
        public virtual void InitRendererList(Action<RecyclingItem.RecyclingEvent> OnAction)
        {
            if (HasInited) return;
            _rectTransformContainer = transform as RectTransform;
            _maskSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
            _scrollRect = transform.parent.GetComponent<ScrollRect>();
            _rectMask = new Rect(0, -_maskSize.y, _maskSize.x, _maskSize.y);
            _rendererCount = ColumnCount * (Mathf.CeilToInt(_maskSize.y / GetBlockSizeY()) + 1);
            UpdateRecyclingItemRects(_rendererCount);
            _listItems = new List<RecyclingItem>();
            for (int i = 0; i < _rendererCount; ++i)
            {
                GameObject child = GameObject.Instantiate(RenderGO);
                child.name += i.ToString();
                child.transform.SetParent(transform);
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                child.layer = gameObject.layer;
                RecyclingItem dfItem = child.GetComponent<RecyclingItem>();
                if (dfItem == null)
                    throw new Exception("Render must extend RecyclingItem");
                _listItems.Add(dfItem);
                _listItems[i].CurrentItemRect = _dictRect[i];
                _listItems[i].OnActionHandler = OnAction;
                child.SetActive(true); // SetActive will cost a lot time.
                UpdateChildTransformPos(child, i);
            }
            SetListRenderSize(_rendererCount);
            HasInited = true;
        }

        void SetListRenderSize(int count)
        {
            _rectTransformContainer.sizeDelta = new Vector2(_rectTransformContainer.sizeDelta.x, Mathf.CeilToInt((count * 1.0f / ColumnCount)) * GetBlockSizeY() + yPadding * 2);
            _scrollRect.vertical = _rectTransformContainer.sizeDelta.y > _maskSize.y;
            CurrentLocatedIndex = 0;
        }

        void UpdateChildTransformPos(GameObject child, int index)
        {
            int row = index / ColumnCount;
            int column = index % ColumnCount;
            Vector2 v2Pos = new Vector2();
            v2Pos.x = GetColumnXPosition(column);
            v2Pos.y = GetRowYPosition(row);
            ((RectTransform)child.transform).anchoredPosition3D = Vector3.zero;
            ((RectTransform)child.transform).anchoredPosition = v2Pos;
        }

        protected float GetBlockSizeY() { return CellSize.y + SpacingSize.y; }
        protected float GetBlockSizeX() { return CellSize.x + SpacingSize.x; }

        void UpdateRecyclingItemRects(int count)
        {
            _dictRect = new Dictionary<int, RecyclingItemRect>();
            for (int i = 0; i < count; ++i)
            {
                int row = i / ColumnCount;
                int column = i % ColumnCount;
                RecyclingItemRect dRect = new RecyclingItemRect(GetColumnXPosition(column), GetRowYPosition(row), CellSize.x, CellSize.y, i);
                _dictRect[i] = dRect;
            }
        }

        private float GetColumnXPosition(int column)
        {
            return column * GetBlockSizeX();
        }

        private float GetRowYPosition(int row)
        {
            return -CellSize.y - row * GetBlockSizeY() - yPadding;
        }


        public void SetDataProvider(IList datas)
        {
            UpdateRecyclingItemRects(datas.Count);
            SetListRenderSize(datas.Count);
            DataProviders = datas;
            ClearAllListRenderDr();
        }

        void ClearAllListRenderDr()
        {
            if (_listItems != null)
            {
                int len = _listItems.Count;
                for (int i = 0; i < len; ++i)
                {
                    RecyclingItem item = _listItems[i];
                    item.CurrentItemRect = null;
                }
            }
        }

        public IList GetDataProvider() { return DataProviders; }

        public void RefreshDataProvider()
        {           
            if (DataProviders == null)
                throw new Exception("DataProviders is null .please call SetDataProvider first");
            UpdateRecyclingItemRects(DataProviders.Count);
            SetListRenderSize(DataProviders.Count);
            ClearAllListRenderDr();
        }

        public virtual void LocateRenderItemAtTarget(object target, float delay)
        {
            LocateRenderItemAtIndex(DataProviders.IndexOf(target), delay);
        }

        public virtual void LocateRenderItemAtIndex(int index, float delay)
        {
            if (index < 0 || index > DataProviders.Count - 1)
                throw new Exception("Locate Index Error " + index);
            index = Math.Min(index, DataProviders.Count - _rendererCount + 2);
            index = Math.Max(0, index);
            Vector2 pos = _rectTransformContainer.anchoredPosition;
            int row = index / ColumnCount;
            Vector2 v2Pos = new Vector2(pos.x, row * GetBlockSizeY());
            m_Coroutine = StartCoroutine(TweenMoveToPos(pos, v2Pos, delay));
            CurrentLocatedIndex = index;
        }

        protected IEnumerator TweenMoveToPos(Vector2 pos, Vector2 v2Pos, float delay)
        {
            bool running = true;
            float passedTime = 0f;
            while (running)
            {
                yield return new WaitForEndOfFrame();
                passedTime += Time.deltaTime;
                Vector2 vCur;
                if (passedTime >= delay)
                {
                    vCur = v2Pos;
                    running = false;
                    StopCoroutine(m_Coroutine);
                    m_Coroutine = null;
                }
                else
                {
                    vCur = Vector2.Lerp(pos, v2Pos, passedTime / delay);
                }
                _rectTransformContainer.anchoredPosition = vCur;
            }
        }

        protected void UpdateRender()
        {
            _rectMask.y = -_maskSize.y - _rectTransformContainer.anchoredPosition.y;
            m_InOverlaps.Clear();

            // template's rect. 
            foreach (RecyclingItemRect dR in _dictRect.Values)
            {
                if (dR.Overlaps(_rectMask))
                {
                    m_InOverlaps.Add(dR.Index, dR);
                }
            }

            foreach(var item in _listItems)
            {
                if (item.CurrentItemRect != null && !m_InOverlaps.ContainsKey(item.CurrentItemRect.Index))
                    item.CurrentItemRect = null;
            }

            foreach (RecyclingItemRect dR in m_InOverlaps.Values)
            {
                if (GetRecyclingItem(dR) == null)
                {
                    RecyclingItem item = GetEmptyRecyclingItem();
                    item.CurrentItemRect = dR;
                    UpdateChildTransformPos(item.gameObject, dR.Index);

                    if (DataProviders != null && dR.Index < DataProviders.Count)
                    {
                        item.SetData(DataProviders[dR.Index]);
                    }
                }
            }
            _listItems.ForEach(i => i.gameObject.SetActive(i.CurrentItemRect != null));
        }

        RecyclingItem GetEmptyRecyclingItem()
        {
            int len = _listItems.Count;
            for (int i = 0; i < len; ++i)
            {
                RecyclingItem item = _listItems[i];
                if (item.CurrentItemRect == null)
                    return item;
            }
            return null;
        }

        RecyclingItem GetRecyclingItem(RecyclingItemRect rect)
        {
            int len = _listItems.Count;
            for (int i = 0; i < len; ++i)
            {
                RecyclingItem item = _listItems[i];
                if (item.CurrentItemRect == null)
                    continue;
                if (rect.Index == item.CurrentItemRect.Index)
                    return item;
            }
            return null;
        }

        void Update()
        {
            if (HasInited)
                UpdateRender();
        }
    }
}