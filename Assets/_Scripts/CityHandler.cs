using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CityHandler : NetworkBehaviour
{
    [SerializeField] private GameObject cityPrefab;

    public List<City> cities = new List<City>();

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
        

        int money = Global.newCityResourceCost[owner.citys.Count, 0];
        int wood = Global.newCityResourceCost[owner.citys.Count, 1];
        int stone = Global.newCityResourceCost[owner.citys.Count, 2];


        if (NetworkManager.Singleton.IsServer) Debug.Log("building city on server");
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
        city.ChangeSizeServerRpc(1);
        netObj.ChangeOwnership(Global.playerHandler.players[ownerIndex].OwnerClientId);
    }
}
