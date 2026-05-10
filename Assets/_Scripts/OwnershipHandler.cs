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
        LogsHandler.Log("Initializing OwnershipHandler");
        foreach (Tile tile in tilesHandler.tiles)
        {
            tileOwners.Add(new List<float>());
            foreach (Player player in playerHandler.players)
            {
                tileOwners[^1].Add(0);
            }
        }
    }

    private float GetOwnershipByDistance(int distance)
    {
        if (distance == 0)
        {
            return 10f;
        }
        if (distance > ownershipDistance)
        {
            return 0;
        }
        //return 1f * Mathf.Pow(1f - ownershipFallof, distance);
        float t = 1f - ((float)distance / (ownershipDistance + 1));
        return t * t;
    }

    void OwnershipChanged(int prev, int curr, int player)
    {
        //LogsHandler.LogError("New ownership change: player " + player + " moved from tile " + prev + " to tile " + curr);
        List<Tile> tilesAffected = new List<Tile>();
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
                    tileOwners[tilesHandler.GetIndexOf(n)][player] -= GetOwnershipByDistance(distance);
                    LogsHandler.Log("removing ownership from tile " + n.name + " for player " + player + ", distance: " + distance + ", removed value: " + 1f * Mathf.Pow(1f - ownershipFallof, distance) + ", final value: " + tileOwners[tilesHandler.GetIndexOf(n)][player]);
                    UpdateTileOwnership(n);
                    tilesAffected.Add(n);
                }
            }
        }
        if (curr != -1) { 
            Tile currTile = tilesHandler.GetTileAt(curr);
            List<Tile> neighbours = tilesHandler.IndirectTileNeighbours(currTile, ownershipDistance);
            LogsHandler.Log("Current tile: " + currTile.name + ", Neighbours: " + neighbours.Count);
            foreach (Tile n in neighbours) 
            {
                int distance = tilesHandler.Distance(currTile, n);
                if (distance <= ownershipDistance)
                {
                    tileOwners[tilesHandler.GetIndexOf(n)][player] += GetOwnershipByDistance(distance);
                    LogsHandler.Log("adding ownership to tile " + n.name + " for player " + player + ", distance: " + distance + ", added value: " + 1f * Mathf.Pow(1f - ownershipFallof, distance) + ", final value: " + tileOwners[tilesHandler.GetIndexOf(n)][player]);
                    UpdateTileOwnership(n);
                    tilesAffected.Add(n);
                }
            }
        }

        List <Tile> tilesAffectedNeighbours = new List<Tile>();

        foreach (Tile tile in tilesAffected)
        {
            foreach (Tile neighbour in tile.neighbours)
            {
                if (neighbour == null) continue;
                if (!tilesAffectedNeighbours.Contains(neighbour) && !tilesAffected.Contains(neighbour))
                {
                    tilesAffectedNeighbours.Add(neighbour);
                }
            }
        }
        tilesAffected.AddRange(tilesAffectedNeighbours);
        foreach (Tile tile in tilesAffected)
        {
            tile.UpdateBorders();
        }
    }

    void UpdateTileOwnership(Tile tile)
    {
        int tileIndex = tilesHandler.GetIndexOf(tile);

        float maxOwnership = float.MinValue;
        float secondMaxOwnership = float.MinValue;
        int owner = -1;
        int ownershipCount = 0;

        for (int i = 0; i < playerHandler.players.Count; i++)
        {
            float value = tileOwners[tileIndex][i];

            ownershipCount++;

            if (value > maxOwnership)
            {
                secondMaxOwnership = maxOwnership;
                maxOwnership = value;
                owner = i;
            }
            else if (value > secondMaxOwnership)
            {
                secondMaxOwnership = value;
            }
        }

        LogsHandler.Log(
            "Tile " + tile.name +
            " ownership values: " + string.Join(", ", tileOwners[tileIndex]) +
            ", max ownership: " + maxOwnership +
            ", second max ownership: " + secondMaxOwnership
        );

        // No meaningful ownership
        if (maxOwnership <= 0)
        {
            tile.UpdateOwner(null);
            LogsHandler.Log("Tile " + tile.name + " has no owner");
            return;
        }

        // If only one player exists, just use maxOwnership threshold
        if (ownershipCount < 2 || secondMaxOwnership == float.MinValue)
        {
            if (maxOwnership < minOwnership)
            {
                tile.UpdateOwner(null);
                LogsHandler.Log("Tile " + tile.name + " has no owner");
                return;
            }

            tile.UpdateOwner(playerHandler.players[owner]);
            return;
        }

        if (maxOwnership - secondMaxOwnership < minOwnership)
        {
            tile.UpdateOwner(null);
            LogsHandler.Log("Tile " + tile.name + " has no owner");
            return;
        }

        tile.UpdateOwner(playerHandler.players[owner]);
    }
    // Update is called once per frame
    void Update()
     {
        
    }
}
