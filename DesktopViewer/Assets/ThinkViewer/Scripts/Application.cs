using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Manager;
using Think.Viewer.Event;

using UnityEngine;
using Think.Viewer.Scripts.Net;
using Think.Viewer.Module;
using Think.Viewer.Net;
using System;

namespace Think.Viewer
{
    /// <summary>
    /// Entry Point
    /// </summary>
    public class Application : MonoBehaviour
    {
        public static string UUID = Guid.NewGuid().ToString();
        [SerializeField] UIModule uiModule;
        NetworkController controller;
        void Start()
        {
            GameClientNetwork.Instance.Launch();
            GameClientNetwork.Instance.Start();
            ModuleManager.Add(uiModule);
            ModuleManager.Add(new MessageModule());
            ModuleManager.Add(new StreamingModule());
            ModuleManager.Add(new DataModule());

            uiModule.Push(UITypes.WelcomeUI, Layer.Default);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}