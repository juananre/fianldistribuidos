using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_SocketIO
{
    [Serializable]
    public class Message
    {
        public string Header { get; set; }
        public object Data { get; set; }
    }
}
