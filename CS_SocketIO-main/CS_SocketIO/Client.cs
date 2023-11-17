using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CS_SocketIO
{
    public class Client
    {
        public string Id { get; internal set; }

        public IPEndPoint remoteEP { get; set; }
        public object Data { get; internal set; }

        private ServerUdp socketUdp;

        private Dictionary<string, List<Action<object>>> EventsActions;

        public Client(IPEndPoint remoteEP, ServerUdp socketUdp)
        {

            this.Id = Guid.NewGuid().ToString();
            this.remoteEP = remoteEP;
            this.socketUdp = socketUdp;

            EventsActions = new Dictionary<string, List<Action<object>>>();


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
            socketUdp.SendTo(remoteEP, header, data);
        }

        public void Broadcast(string header, object data)
        {
            socketUdp.Broadcast(remoteEP, header, data);
        }

        public void RunActions(Message receivedMessage)
        {
            if (EventsActions.ContainsKey(receivedMessage.Header))
            {
                foreach (var action in EventsActions[receivedMessage.Header])
                {
                    action(receivedMessage.Data);
                }
            }
        }

        public void Disconnect(string message)
        {
            socketUdp.DisconnectClient(this, message);
        }
    }
}
