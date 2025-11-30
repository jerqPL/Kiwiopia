using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public List<Player> players = new List<Player>();
    
    public Player GetPlayerAt(int index)
    {
        return players[index]; 
    }

    public int GetIndexOf(Player player)
    {
        return players.IndexOf(player); 
    }

    public Player GetLocalPlayer()
    {
        foreach (Player player in players)
        {
            if (player.GetComponent<NetworkObject>().IsLocalPlayer) return player;
        }
        return null;
    }

    public int GetLocalPlayerIndex()
    {
        return GetIndexOf(GetLocalPlayer());
    }

    public void SpawnPlayers()
    {
        foreach(Player player in players)
        {
            player.SpawnPlayer();
        }
    }
}
