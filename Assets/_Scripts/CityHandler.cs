using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CityHandler : MonoBehaviour
{
    [SerializeField] private SelectionHandler selectionHandler;
    [SerializeField] private GameObject cityPrefab;

    public List<City> cities = new List<City>();

    public int numberOfCities = 0;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void BuildCityServerRpc(int ownerIndex, int tileIndex)
    {
        Player owner = Global.playerHandler.GetPlayerAt(ownerIndex);
        Tile tile = Global.tilesHandler.GetTileAt(tileIndex);

        if (owner == null || tile == null)
            return;

        if (tile.city != null)
        {
            Debug.Log("City already built!");
            return;
        }

        tile.owner = owner;

        int money = Global.newCityResourceCost[numberOfCities, 0];
        int wood = Global.newCityResourceCost[numberOfCities, 1];
        int stone = Global.newCityResourceCost[numberOfCities, 2];

        if (!owner.TakeResources(money, wood, stone))
        {
            Debug.Log($"Not enough resources: {money}, {wood}, {stone}");
            return;
        }

        // Spawn the city on the server
        GameObject cityObj = Instantiate(cityPrefab, tile.transform.position, Quaternion.identity);
        NetworkObject netObj = cityObj.GetComponent<NetworkObject>();
        City city = cityObj.GetComponent<City>();
        city.ownerIndex.Value = ownerIndex;
        city.tileIndex.Value = tileIndex;


        netObj.Spawn(); // == replicate to all clients

        
        numberOfCities++;

    }
}
