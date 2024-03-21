/*
 * File: RecyclingItem.cs
 *
 * Copyright(c) 2023 Lenovo, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary
 *
 */
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace Think.Viewer.Recycling
{
    public class RecyclingItem : MonoBehaviour
    {
        public class RecyclingEvent
        {
            public string Type;
            public object Target;
            
        }
        public delegate void OnSelect(RecyclingItem item);
        public delegate void OnUpdateData(RecyclingItem item);
        public OnSelect OnSelectHandler;
        public OnUpdateData OnUpdateDataHandler;
        public Action<RecyclingEvent> OnActionHandler;
        protected RecyclingItemRect ItemRect;
        protected object Data;

        public RecyclingItemRect CurrentItemRect
        {
            set
            {
                ItemRect = value;
                //gameObject.SetActive(value != null); //SetActive will cost a lot time.
                //if (group == null) group = GetComponent<CanvasGroup>();
                //group.alpha = value != null ? 1 : 0;
            }
            get { return ItemRect; }
        }
        
        public Vector2 OverrideSize;

        public void SetData(object data)
        {
            if (data == null)
            {
                return;
            }

            Data = data;
            if (null != OnUpdateDataHandler)
                OnUpdateDataHandler(this);
            OnRenderer();
        }

        protected virtual void OnRenderer()
        {

        }

        [ExcludeFromCoverage]
        protected void DispatchEvent(string evtType)
        {
            OnActionHandler?.Invoke(new RecyclingEvent() 
            {
                Type = evtType,
                Target = Data,
            });
        } 

        public object GetData()
        {
            return Data;
        }

        public T GetData<T>()
        {
            return (T)Data;
        }
    }
}