using Development.Net.Pt;
using System;
using System.Net;
using System.Net.Sockets;

namespace Development.Net.Pt
{
    public class UDPManager
    {
        private UdpClient udpClient;
        public int Port;
        public bool Running = false;
        public event Action<PtMessagePackage> MessageReceived;
        public UDPManager(int port) 
        {
            Port = port;
            udpClient = new UdpClient(port);
        }

        public void Start()
        {
            Running = true;
            StartReceiveAsync();
        }

        async public void StartSendAsync(PtMessagePackage msg)
        {
            byte[] bytes = PtMessagePackage.Write(msg);
            await udpClient.SendAsync(bytes, bytes.Length,new IPEndPoint(new IPAddress(msg.ToIp),msg.ToPort));
        }
        async void StartReceiveAsync()
        {
            while (Running)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    var bytes = result.Buffer;
                    PtMessagePackage msg = PtMessagePackage.Read(bytes);
                    //EventDispatcher<C2S, PtMessagePackage>.DispatchEvent((C2S)msg.MessageId, msg);
                    MessageReceived?.Invoke(msg);
                } catch(SocketException e)
                {
                    continue;
                }
            }
        }
    }
}
