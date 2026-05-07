using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Global;

public class OwnershipHandler : MonoBehaviour
{
    [SerializeField] int ownershipDistance;
    [SerializeField] float ownershipFallof;
    [SerializeField] float minOwnership; 

    List<List<float>> tileOwners = new List<List<float>>();

    void Awake()
    {
        unitsHandler.AfterUnitMoved += OwnershipChanged;
        
    }

    public void GenerateOwnership() 
    { 
        Debug.Log("Initializing OwnershipHandler");
        foreach (Tile tile in tilesHandler.tiles)
        {
            tileOwners.Add(new List<float>());
            foreach (Player player in playerHandler.players)
            {
                tileOwners[^1].Add(0);
            }
        }
    }
    
    void OwnershipChanged(int prev, int curr, int player)
    {
        //Debug.LogError("New ownership change: player " + player + " moved from tile " + prev + " to tile " + curr);
        if (prev == curr)
        {
            return;
        }

        if (prev != -1)
        {
            Tile prevTile = tilesHandler.GetTileAt(prev);
            List<Tile> neighbours = tilesHandler.IndirectTileNeighbours(prevTile, ownershipDistance);
            foreach (Tile n in neighbours) 
            {
                int distance = tilesHandler.Distance(prevTile, n);
                if (distance <= ownershipDistance)
                {
                    tileOwners[tilesHandler.GetIndexOf(n)][player] -= 1f * Mathf.Pow(1f - ownershipFallof, distance);
                    Debug.Log("removing ownership from tile " + n.name + " for player " + player + ", distance: " + distance + ", removed value: " + 1f * Mathf.Pow(1f - ownershipFallof, distance) + ", final value: " + tileOwners[tilesHandler.GetIndexOf(n)][player]);
                    UpdateTileOwnership(n);
                }
            }
        }
        if (curr != -1) { 
            Tile currTile = tilesHandler.GetTileAt(curr);
            List<Tile> neighbours = tilesHandler.IndirectTileNeighbours(currTile, ownershipDistance);
            Debug.Log("Current tile: " + currTile.name + ", Neighbours: " + neighbours.Count);
            foreach (Tile n in neighbours) 
            {
                int distance = tilesHandler.Distance(currTile, n);
                if (distance <= ownershipDistance)
                {
                    tileOwners[tilesHandler.GetIndexOf(n)][player] += 1f * Mathf.Pow(1f - ownershipFallof, distance);
                    Debug.Log("adding ownership to tile " + n.name + " for player " + player + ", distance: " + distance + ", added value: " + 1f * Mathf.Pow(1f - ownershipFallof, distance) + ", final value: " + tileOwners[tilesHandler.GetIndexOf(n)][player]);
                    UpdateTileOwnership(n);
                }
            }
        }
    }

    void UpdateTileOwnership(Tile tile)
    {
        int tileIndex = tilesHandler.GetIndexOf(tile);
        float maxOwnership = 0;
        float secondMaxOwnership = 0;
        int owner = -1;
        for (int i = 0; i < playerHandler.players.Count; i++)
        {
            if (tileOwners[tileIndex][i] > maxOwnership)
            {
                secondMaxOwnership = maxOwnership;
                maxOwnership = tileOwners[tileIndex][i];
                owner = i;
            }
        }
        tile.transform.position = new Vector3(tile.transform.position.x, maxOwnership - 0.5f, tile.transform.position.z);
        if (owner == -1 || maxOwnership - secondMaxOwnership > minOwnership)
        {
            tile.owner = null;
            Debug.Log("Tile " + tile.name + " has no owner");
            
            return;
        }
        tile.owner = playerHandler.players[owner];
        
    }

    // Update is called once per frame
    void Update()
     {
        
    }
}
