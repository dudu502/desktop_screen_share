﻿using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Think.Viewer.Event
{
    public class NetMessageEvt
    {
        public NetPeer Peer { private set; get; }
        public byte[] Content { private set; get; }
        public NetMessageEvt(NetPeer peer,byte[] content)
        {
            Peer = peer;
            Content = content;
        }
    }
}
