
using CS_SocketIO;
using GameServer;

const int SERVER_TIME_STEP = 100;
ServerUdp io = new ServerUdp(11000);
Game game = new Game();


io.On("connection",onConnection);


UpdateState();

void onConnection(object _client)
{
    Client client = (Client)_client;
    string username = ((dynamic)client.Data)?.Username;

    if (string.IsNullOrEmpty(username))
    {
        client.Disconnect("Debe enviar el username en la data inicial");
        return;
    }

    Console.WriteLine("Cliente conectado " + username);
    
    game.SpawnPlayer(client.Id,username);

    client.Emit("welcome", new {
            Message="Bienvenido al juego",
            Id = client.Id,
            State =game.State,
        });
    client.Broadcast("newPlayer",new { Id=client.Id , Username=username });
    client.On("move", (axis) => {
        int  horizontal = ((dynamic)axis).Horizontal;
        int vertical = ((dynamic)axis).Vertical;

        game.SetAxis(client.Id,new Axis {Horizontal=horizontal,Vertical = vertical  });
    });
    



    client.On("disconnect", (_client) =>
    {
        game.RemovePlayer(client.Id);

         Console.WriteLine("usuario desconectado " + client.Id);
    });
}
io.Listen();



 async Task UpdateState()
{
    while (true)
    {
        io.Emit("updateState", new { State = game.State });
        await Task.Delay(TimeSpan.FromMilliseconds(SERVER_TIME_STEP));
    }
}
