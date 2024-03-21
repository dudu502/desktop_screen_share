/*
 * File: FlexibleSingleColumnItem.cs
 *
 * Copyright(c) 2023 Lenovo, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Think.Viewer.Recycling
{
    public interface IFlexible
    {
        float height { set; get; }
        bool isOverlaps { set; get; }
        Rect rect { set; get; }
    }
    [RequireComponent(typeof(CanvasGroup))]
    public class FlexibleSingleColumnItem : MonoBehaviour
    {
        public class Event
        {
            public string Type;
            public IFlexible Target;
        }

        public IFlexible Data;

        public Action<Event> OnEvent;

        public void SetData(IFlexible data)
        {
            if (data == null)
                return;
            Data = data;
            OnRenderer();
        }

        protected virtual void OnRenderer()
        {

        }
    
        void Update()
        {

        }
    }
}
