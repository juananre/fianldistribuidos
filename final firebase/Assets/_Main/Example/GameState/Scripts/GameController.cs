using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameContainer;
    [SerializeField]
    private Transform PlayersContainer;
    [SerializeField]
    private Transform CoinsContainer;

    [SerializeField]
    private GameObject PlayerPrefab;
    [SerializeField]
    private GameObject CoinPrefab;

    private GameState State;
    private Dictionary<string, Transform> PlayersToRender;
    private Dictionary<string, Transform> CoinsToRender;
    [Header("jugadores")]
    private List<string> connectedPlayers = new List<string>();

    public event Action<Vector2> onProjectileLaunch;

    [SerializeField]
    private TMP_Text playerListPrefab; // Prefab de TextMeshProUGUI para representar a un jugador en la lista
    [SerializeField]
    private Transform scrollViewContent; // Asigna la referencia al contenido del ScrollView desde el Inspector
    [SerializeField]
    private GameObject ProjectilePrefab;

   
    internal void StartGame(GameState state)
    {
        PlayersToRender = new Dictionary<string, Transform>();
        CoinsToRender = new Dictionary<string, Transform>();

        GameObject.Find("PanelConnect").SetActive(false);
        GameContainer.SetActive(true);


        foreach (Player player in state.Players)
        {
            connectedPlayers.Add(player.Username);
            InstantiatePlayer(player);
        }

        var Socket = NetworkController._Instance.Socket;

        InputController._Instance.onAxisChange += (axis) => { Socket.Emit("move", axis); };
        Socket.On("projectileLaunched", OnProjectileLaunched);

        State = state;
        Socket.On("updateState", UpdateState);


    }

    private void OnProjectileLaunched(string json)
    {
        JsonData jsonData = JsonUtility.FromJson<JsonData>(json);
        Vector2 position = JsonUtility.FromJson<Vector2>(jsonData.Position);

        InstantiateProjectile(position);
    }
    private void InstantiatePlayer(Player player)
    {
        GameObject playerGameObject = Instantiate(PlayerPrefab, PlayersContainer);
        playerGameObject.transform.position = new Vector2(player.x, player.y);
        playerGameObject.GetComponent<GamePlayer>().Id = player.Id;
        playerGameObject.GetComponent<GamePlayer>().Username = player.Id;

        PlayersToRender[player.Id] = playerGameObject.transform;
    }

    private void UpdateState(string json)
    {
        GameStateData jsonData = JsonUtility.FromJson<GameStateData>(json);
        State = jsonData.State;

    }

    internal void NewPlayer(string id, string username)
    {
        InstantiatePlayer(new Player { Id = id, Username = username });
        connectedPlayers.Add(username);
    }

    void Update()
    {
        UpdateConnectedPlayersUI();
        if (State != null)
        {
            foreach (Player player in State.Players)
            {
                if (PlayersToRender.ContainsKey(player.Id))
                {
                    PlayersToRender[player.Id].position = new Vector2(player.x, player.y);
                }
                else
                {
                    InstantiatePlayer(player);
                }

            }
            var plarersToDelete = PlayersToRender.Where(item => !State.Players.Any(player => player.Id == item.Key)).ToList();
            foreach (var playerItem in plarersToDelete)
            {
                Destroy(playerItem.Value.gameObject);
                PlayersToRender.Remove(playerItem.Key);
            }
            foreach (Coin coin in State.Coins)
            {
                if (CoinsToRender.ContainsKey(coin.Id))
                {
                    CoinsToRender[coin.Id].position = new Vector2(coin.x, coin.y);
                }
                else
                {
                    InstantiateCoin(coin);
                }
            }
            var coinsToDelete = CoinsToRender.Where(item => !State.Coins.Any(coin => coin.Id == item.Key)).ToList();

            foreach (var coinItem in coinsToDelete)
            {
                Destroy(coinItem.Value.gameObject);
                CoinsToRender.Remove(coinItem.Key);
            }

        }
        
    }
    private void InstantiateCoin(Coin coin)
    {
        GameObject coinGameObject = Instantiate(CoinPrefab, CoinsContainer);
        coinGameObject.transform.position = new Vector2(coin.x, coin.y);
        coinGameObject.GetComponent<GameCoin>().Id = coin.Id;

        CoinsToRender[coin.Id] = coinGameObject.transform;
    }
    private void InstantiateProjectile(Vector2 position)
    {
        GameObject projectileGameObject = Instantiate(ProjectilePrefab, CoinsContainer);
        projectileGameObject.transform.position = position;
    }
    public void LaunchProjectile(Vector2 position)
    {
        onProjectileLaunch?.Invoke(position);
    }


    private void UpdateConnectedPlayersUI()
    {
        // Limpiar la lista actual en el ScrollView
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos objetos de texto para cada jugador y agregarlos al ScrollView
        foreach (string playerName in connectedPlayers)
        {
            TMP_Text playerText = Instantiate(playerListPrefab, scrollViewContent);
            playerText.text = playerName;
        }
    }
}

[Serializable]
public class GameStateData
{
    public GameState State;
}
