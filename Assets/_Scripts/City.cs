using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements.Experimental;

public class City : NetworkBehaviour
{
    public List<Tile> cityTiles = new List<Tile>();
    public NetworkVariable<int> tileIndex = new NetworkVariable<int>();
    public Tile tile => Global.tilesHandler.GetTileAt(tileIndex.Value);
    public NetworkVariable<int> ownerIndex = new NetworkVariable<int>();
    public Player owner => Global.playerHandler.GetPlayerAt(ownerIndex.Value);

    public int orderNumber = 0;
    public int size = 0;

    public int money = 0;
    public int wood = 0;
    public int stone = 0;

    public override void OnNetworkSpawn()
    {
        transform.position = tile.transform.position;
        owner.citys.Add(this);
        tile.city = this;
        Global.cityHandler.cities.Add(this);
        ChangeSize(1);
    }

    public void ChangeSize(int newSize)
    {
        newSize -= 2;
        if (newSize >= 0)
        {
            int moneyToTake = Global.newCityResourceCost[orderNumber, 0] * Global.cityUpgradeCostMultiplayer[newSize, 0];
            int woodToTake = Global.newCityResourceCost[orderNumber, 1] * Global.cityUpgradeCostMultiplayer[newSize, 1];
            int stoneToTake = Global.newCityResourceCost[orderNumber, 2] * Global.cityUpgradeCostMultiplayer[newSize, 2];

            if (!owner.TakeResources(moneyToTake, woodToTake, stoneToTake))
            {
                return;
            }
        }
        size = newSize + 2;   
        foreach (Tile cityTile in cityTiles)
        {
            cityTile.underCity = null;
            cityTile.owner = null;
        }
        cityTiles.Clear();
        foreach(Tile neighbour in tile.neighbors)
        {
            if (neighbour.underCity == null && (neighbour.owner == tile.owner || neighbour.owner == null))
            {
                neighbour.underCity = this;
                cityTiles.Add(neighbour);
                neighbour.owner = tile.owner;
            }
        }
        for (int i = 0; i < size-1; i++)
        {
            int cityTilesCount = cityTiles.Count;
            for( int j = 0; j < cityTilesCount; j++) 
            {
                Tile cityTile = cityTiles[j];
                foreach (Tile neighbour in cityTile.neighbors)
                {
                    if (!cityTiles.Contains(neighbour) && neighbour.underCity == null && (neighbour.owner == tile.owner || neighbour.owner == null) && neighbour != tile)
                    {
                        neighbour.underCity = this;
                        cityTiles.Add(neighbour);
                        neighbour.owner = tile.owner;
                    }
                }
            }
        }
        foreach(Tile cityTile in cityTiles)
        {
            cityTile.transform.position = new Vector3(cityTile.transform.position.x, .15f, cityTile.transform.position.z);
        }
    }

    private void Update()
    {
        SendResourcesToPlayer();
    }

    void SendResourcesToPlayer()
    {
        if (money == 0 && wood == 0 && stone == 0)
        {
            return;
        }
        owner.RecieveResources(money, wood, stone);
        money = 0;
        wood = 0;
        stone = 0;
    }

    public void RecieveResources(int rMoney, int rWood, int rStone)
    {
        money += rMoney;
        wood += rWood;
        stone += rStone;
    }
}
