using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class LobbyHandler : NetworkBehaviour
{
    public int maxPlayers = 4;

    // list of connected players
    private List<ulong> connectedClients = new List<ulong>();

    public bool gameStarted = false;
    public List<Player> playersPlaying = new List<Player>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        connectedClients.Add(clientId);
        Debug.Log($"Client connected: {clientId}");

        if (connectedClients.Count == maxPlayers)
        {
            StartGame();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        connectedClients.Remove(clientId);
        Debug.Log($"Client disconnected: {clientId}");
    }

    public void StartGame()
    {
        if (!IsServer) return;
        Global.uIHandler.HideGameMenuClientRpc();
        gameStarted = true;
        Debug.Log("All players connected! Generating map...");
        // call your TerrainGeneration / TilesHandler generation
        Global.terrainGeneration.SpawnMapServerRpc();
        Global.playerHandler.SpawnPlayers();
        playersPlaying = new List<Player>(Global.playerHandler.players);
    }

    private void Update()
    {
        if (!IsServer) return;
        List<Player> lost = new List<Player>();
        foreach(Player player in playersPlaying)
        {
            if (player.units.Count == 0 && player.citys.Count == 0)
            {
                player.LostClientRpc();
                lost.Add(player);
            }
        }
        foreach (Player player in lost)
        {
            playersPlaying.Remove(player);
        }
        if (playersPlaying.Count == 1)
        {
            playersPlaying[0].WonClientRpc();
        }
    }
}
