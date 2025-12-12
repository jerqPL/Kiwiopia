using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public List<Player> players = new List<Player>();
    public List<Color> playerColors = new List<Color>();
    public Color localPlayerColor;
    public Color defaultColor;
    
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

    public Color GetPlayerColor(int index)
    {
        if (GetLocalPlayerIndex() == index)
        {
            return localPlayerColor;
        }
        if (index < playerColors.Count && index >= 0)
        {
            return playerColors[index];
        }
        return defaultColor;
    }
}
