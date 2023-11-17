using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CS_SocketIO
{
    public class ClientUdp
    {
        private UdpClient udpClient;
        private IPEndPoint ServerEndPoint;
        private bool isListening = true;
        private Task ListenTask;
        private bool connected;
        private Dictionary<string, List<Action<object>>> EventsActions;

        public string Id { get; set; }

        public ClientUdp(string ip, int port)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Start(null);
        }

        public ClientUdp(string ip, int port, object data)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Start(data);
        }

        private void Start(object data)
        {
            EventsActions = new Dictionary<string, List<Action<object>>>();
            udpClient = new UdpClient();
            udpClient.Connect(ServerEndPoint);

            data = data == null ? new object() : data;
            Emit("connection", data);

            StartHeartbeat();
        }

        private void StartHeartbeat()
        {
            var timer = new Timer((e) => SendHeartbeat(), null, 0, 1000);
        }

        public void SendHeartbeat()
        {
            Emit("Heartbeat_check_connection", "");
        }
        public void Listen()
        {
            Console.WriteLine("Cliente conectado al servidor en " + ServerEndPoint.ToString() + "|UDP");

            while (isListening)
            {
                var receivedData = udpClient.Receive(ref ServerEndPoint);
                HandleMessage(receivedData);


            }
        }



        private void HandleMessage(byte[] serializedMessage)
        {
            Message receivedMessage = ServerUdp.DeserializeMessage(serializedMessage);

            switch (receivedMessage.Header)
            {
                case "connect":
                    Id = ((string)receivedMessage.Data);
                    connected = true;
                    RunActions(receivedMessage);
                    break;
                case "disconnect":
                    connected = false;
                    udpClient.Close();
                    RunActions(receivedMessage);
                    break;
                default:
                    if (!connected)
                    {
                        break;
                    }

                    RunActions(receivedMessage);
                    break;
            }

        }
        private void RunActions(Message receivedMessage)
        {
            if (EventsActions.ContainsKey(receivedMessage.Header))
            {
                foreach (var action in EventsActions[receivedMessage.Header])
                {
                    action(receivedMessage.Data);
                }
            }
        }

        public void On(string name, Action<object> action)
        {
            if (!EventsActions.ContainsKey(name))
            {
                EventsActions.Add(name, (new List<Action<object>> { action }));
            }
            else
            {
                EventsActions[name].Add(action);
            }
        }

        public void Emit(string header, object data)
        {
            Send(header, data);
        }

        public void Broadcast(string header, object data)
        {
            Broadcast(header, data);
        }

        internal void Send(string header, object data)
        {
            byte[] serializedMessage = ServerUdp.SerializeMessage(header, data);
            udpClient.Send(serializedMessage, serializedMessage.Length);
        }



    }

    class NetworkClient
    {
        public string Id;
    }
}
