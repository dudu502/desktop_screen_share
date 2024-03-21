using System.Collections;
using System.Collections.Generic;
using Think.Viewer.Recycling;
using ThinkViewer.Scripts.Net.Data;
using UnityEngine;
using UnityEngine.UI;

public class HostItem : RecyclingItem
{
    public Image BackgroundImage;
    public Button Connect;
    public TMPro.TMP_Text Name;
    void Start()
    {
        
    }
    protected override void OnRenderer()
    {
        base.OnRenderer();
        HostNetInfo hostNetInfo = GetData<HostNetInfo>();
        Name.text = hostNetInfo.HostName;
    }
}
