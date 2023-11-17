
using CS_SocketIO;

ServerUdp io = new ServerUdp(11000);
io.On("connection", (_client) =>
{
    Client client = (Client)_client;
    string username = ((dynamic)client.Data)?.Username;

    //if(string.IsNullOrEmpty(username))
    //{
    //    client.Disconnect("Debe enviar el username en la data inicial");
    //}


    Console.WriteLine("Cliente conectado " + username);

    client.Emit("welcome", "Bienvenido al servidor udp del curso");


    client.On("chat", (data) =>
    {
        string msg = ((dynamic)data)?.Message;
        Console.WriteLine("chat message from " + client.Id);
        Console.WriteLine("msg " + msg);

        client.Broadcast("chat", (new { Username = username, Message = msg }));
    });

    client.On("disconnect", (data) =>
    {
        Console.WriteLine("usuario desconectado " + client.Id);
    });

});
io.Listen();


