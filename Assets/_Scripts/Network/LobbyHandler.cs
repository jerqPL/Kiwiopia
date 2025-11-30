using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class LobbyHandler : NetworkBehaviour
{
    public int maxPlayers = 4;

    // list of connected players
    private List<ulong> connectedClients = new List<ulong>();

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
        Debug.Log("All players connected! Generating map...");
        // call your TerrainGeneration / TilesHandler generation
        Global.terrainGeneration.SpawnMapServerRpc();
        Global.playerHandler.SpawnPlayers();
    }
}
