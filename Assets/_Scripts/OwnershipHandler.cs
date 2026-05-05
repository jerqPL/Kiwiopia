using UnityEngine;
using static Global;
using System.Collections.Generic;

public class OwnershipHandler : MonoBehaviour
{
    [SerializeField] int owershipDistance;
    [SerializeField] float ownershipFallof;

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
    
    void OwnershipChanged(int prev, int curr)
    {
        if (prev == curr)
        {
            return;
        }

        if (prev != -1)
        {
            List<Tile> neighbours = tilesHandler.IndirectTileNeighbours(tilesHandler.GetTileAt(prev), owershipDistance);
            foreach (Tile n in neighbours) 
            {
                n.transform.position += Vector3.up * ownershipFallof;
            }
        }
    }

    // Update is called once per frame
    void Update()
     {
        
    }
}
