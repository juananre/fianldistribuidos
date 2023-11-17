using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class Axis
    {
        public int Vertical;
        public int Horizontal;
    }

    public class GameState
    {
        public List<Player> Players { get; set; }
        public List<Coin> Coins { get; set; }
        public GameState()
        {
            Players = new List<Player>();
            Coins = new List<Coin>();
        }
    }
    internal class Game
    {
        const int WorldWidth = 500;
        const int WorldHeigh = 400;
        const int LoopPeriod = 10;
        const int MaxCoins = 15;
        const int Gravity = 5;
        const int JumpForce = 10;
        public GameState State { get; set; }

        private  Dictionary<string, Axis> Axes;
        public Game()
        {
            State = new GameState();
            Axes = new Dictionary<string, Axis>();

            StartGameLoop();
            StartSpawnCoins();
        }
        public void SpawnPlayer(string id,string username)
        {
            Random random = new Random();
            State.Players.Add(new Player()
            {
                Id = id,
                Username = username,
                x = random.Next(10, WorldWidth - 10),
                y = random.Next(10, WorldHeigh - 10),
                Speed = 2,
                Radius = 10
            });

            Axes[id] = new Axis{Horizontal= 0,Vertical= 0 };

        }

        public void SetAxis(string id,Axis axis)
        {
            Axes[id]= axis;
        }

        public void Update()
        {
            List<string> takedCoinsIds= new List<string>();

            foreach (var player in State.Players)
            {
                var axis = Axes[player.Id];

                if (axis.Horizontal > 0 && player.x < WorldWidth - player.Radius)
                {
                    player.x += player.Speed;
                }
                else if (axis.Horizontal < 0 && player.x > 0 + player.Radius)
                {
                    player.x -= player.Speed;
                }
                if (axis.Vertical > 0 && player.y < WorldHeigh - player.Radius)
                {
                    player.y += player.Speed;
                }
                else if (axis.Vertical < 0 && player.y > 0 + player.Radius)
                {
                    player.y -= player.Speed;
                }

                #region GRAVEDAD y Saltos
                //if (!player.isJumping && player.y > 0 + player.Radius)
                //{
                //    player.y -= Gravity;
                //}
                //if (player.y < 0 + player.Radius)
                //{
                //    player.y = 0 + player.Radius;
                //    player.isJumping = false; // El jugador ha vuelto al suelo y no está saltando
                //}
                //if (axis.Vertical > 0 && !player.isJumping)
                //{
                //    player.y += JumpForce;
                //    player.isJumping = true;

                //}
                //else
                //{
                //    player.isJumping = false;
                //}
                #endregion

                State.Coins = State.Coins.Where(coin => {
                    if (!coin.Take(player))
                    {
                        return true;
                    }
                    else
                    {
                        player.Score += coin.Points;
                        Console.WriteLine(player.Username+":"+player.Score);
                        return false;
                    }
                }).ToList();

                //foreach (Coin coin in State.Coins)
                //{
                //    if (coin.Take(player))
                //    {
                //        player.Score += coin.Points;
                //        Console.WriteLine("Coin taked");
                //        //takedCoinsIds.Add(coin.Id);
                //    }
                //}             

            }
            

        }
        public void RemovePlayer(string id)
        {
            State.Players = State.Players.Where(player => player.Id != id).ToList();
            Axes.Remove(id);
        }

        async Task StartGameLoop()
        {
            while (true)
            {
                Update();
                await Task.Delay(TimeSpan.FromMilliseconds(LoopPeriod)); 
            }
        }

        async Task StartSpawnCoins()
        {
            while (true)
            {
                SpawnCoin();
                await Task.Delay(TimeSpan.FromSeconds(2)); 
            }
        }

        void SpawnCoin()
        {
            Random random = new Random();

            if (State.Coins.Count <= MaxCoins) {
                Coin coin = new Coin {
                    Id = Guid.NewGuid().ToString(),
                    x = random.Next(10, WorldWidth - 10),
                    y = random.Next(10, WorldHeigh - 10),
                    Radius = 10,
                    Points = 1
                };
                State.Coins.Add(coin);
                Console.WriteLine(State.Coins.Count+":New Coin " +coin.Id);
            }
        }

    }
}
