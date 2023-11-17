using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Coin
    {
        public string Id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int Points { get; set; }
        public int Radius { get; set; }
        public bool Taken { get; set; }

        public bool Take(Player player)
        {
            if (!Taken)
            {
                var dx = player.x - x;
                var dy = player.y - y;
                var rSum = Radius + player.Radius;
                
                return dx*dx + dy*dy <= rSum*rSum;
            }
            else
            { return false; }
        }
    }
}
