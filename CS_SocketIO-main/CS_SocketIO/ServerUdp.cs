using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace CS_SocketIO
{
    public class ServerUdp
    {
        private UdpClient udpServer;
        private Dictionary<string, Client> ClientsEndPoints;
        private Dictionary<string, DateTime> LastCommunicationTimes;
        private Dictionary<string, List<Action<object>>> EventsActions;

        private int Port;
        private const int TimeoutThreshold = 5000; // 30 seconds

        public ServerUdp(int port)
        {
            Port = port;
            udpServer = new UdpClient(port);

            ClientsEndPoints = new Dictionary<string, Client>();
            LastCommunicationTimes = new Dictionary<string, DateTime>();
            EventsActions = new Dictionary<string, List<Action<object>>>();
        }
       


        public void Listen()
        {
            Console.WriteLine("Servidor escuchando el puerto " + Port + "|UDP");
            var remoteEP = new IPEndPoint(IPAddress.Any, Port);
            while (true)
            {
                try
                {
                    remoteEP = new IPEndPoint(IPAddress.Any, Port);
                    byte[] data = udpServer.Receive(ref remoteEP);
                    // Update last communication time
                    LastCommunicationTimes[remoteEP.ToString()] = DateTime.Now;
                    HandleMessage(data, remoteEP);

                }
                catch (SocketException ex)
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }

                DateTime currentTime = DateTime.Now;
                foreach (var clientEntry in ClientsEndPoints)
                {
                    string clientIdentifier = clientEntry.Key;
                    DateTime lastCommunicationTime = LastCommunicationTimes[clientIdentifier];
                    if (currentTime.Subtract(lastCommunicationTime).TotalMilliseconds > TimeoutThreshold)
                    {
                        //Console.WriteLine("Client " + clientIdentifier + " disconnected.");

                        ClientsEndPoints.Remove(clientIdentifier);
                        LastCommunicationTimes.Remove(clientIdentifier);

                        Message disconnectionMessage = new Message { Header = "disconnect", Data = clientEntry.Value };

                        RunActions(disconnectionMessage);
                        clientEntry.Value.RunActions(disconnectionMessage);
                    }
                }

            }
        }
        private void HandleMessage(byte[] serializedMessage, IPEndPoint remoteEP)
        {
            Message receivedMessage = DeserializeMessage(serializedMessage);

            Client client;

            switch (receivedMessage.Header)
            {
                case "connection":
                    if (!ClientsEndPoints.ContainsKey(remoteEP.ToString()))
                    {
                        client = new Client(remoteEP, this);
                        ClientsEndPoints.Add(remoteEP.ToString(), client);

                        client.Data = receivedMessage.Data;
                        receivedMessage.Data = client;

                        NetworkClient _client = new NetworkClient { Id = client.Id };


                        SendTo(remoteEP, "connect", client.Id);
                        RunActions(receivedMessage);

                    }
                    break;
                case "Heartbeat_check_connection":
                    if (ClientsEndPoints.ContainsKey(remoteEP.ToString()))
                    {
                        DateTime currentTime = DateTime.Now;
                        LastCommunicationTimes[remoteEP.ToString()] = currentTime;
                    }
                    break;
                case "launchProjectile":
                    // Reenviar la información del proyectil a todos los clientes
                    Broadcast(remoteEP, "projectileLaunched", receivedMessage.Data);
                    break;

                default:
                    if (!ClientsEndPoints.ContainsKey(remoteEP.ToString()))
                    {
                        break;
                    }
                    client = ClientsEndPoints[remoteEP.ToString()];

                    RunActions(receivedMessage);
                    client.RunActions(receivedMessage);
                    break;
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

            byte[] serializedMessag = SerializeMessage(header, data);

            foreach (var client in ClientsEndPoints.Values)
            {
                udpServer.Send(serializedMessag, serializedMessag.Length, client.remoteEP);
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
        internal void SendTo(IPEndPoint remoteEP, string header, object data)
        {
            byte[] serializedMessage = SerializeMessage(header, data);
            udpServer.Send(serializedMessage, serializedMessage.Length, remoteEP);
        }
        internal void Broadcast(IPEndPoint remoteEP, string header, object data)
        {

            byte[] serializedMessag = SerializeMessage(header, data);

            foreach (var client in ClientsEndPoints.Values)
            {
                if (!client.remoteEP.Equals(remoteEP))
                {
                    udpServer.Send(serializedMessag, serializedMessag.Length, client.remoteEP);
                }
            }
        }

        internal static byte[] SerializeMessage(string header, object data)
        {

            string json = JsonConvert.SerializeObject(data);
            byte[] serializedMessage = Encoding.ASCII.GetBytes(header + "|" + json);

            return serializedMessage;
        }
        internal static Message DeserializeMessage(byte[] serializedMessage)
        {
            try
            {
                string receivedData = Encoding.ASCII.GetString(serializedMessage);
                string head = receivedData.Split('|')[0];
                string json = receivedData.Split('|')[1];

                object data = JsonConvert.DeserializeObject<object>(json);

                return new Message { Header = head, Data = data };

            }
            catch (Exception e)
            {

                return new Message { Header = "ERROR", Data = "Error Derilizing Data. \n" + e.Message };
            }

        }

        internal void DisconnectClient(Client client, string msg)
        {
            SendTo(client.remoteEP, "disconnect", msg);
            ClientsEndPoints.Remove(client.remoteEP.ToString());
            LastCommunicationTimes.Remove(client.remoteEP.ToString());

        }
    }

}
