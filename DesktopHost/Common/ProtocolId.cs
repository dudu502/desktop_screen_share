﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Think.Viewer.Common
{
    public enum C2S
    {
        Heartbeat,
        SearchHost,
        StartStreaming,
        StopStreaming,
    }
    public enum S2C
    {
        SearchHost,
        StartStreaming,
        Streaming,
        StopStreaming
    }
}