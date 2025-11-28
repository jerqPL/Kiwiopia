using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CityHandler : MonoBehaviour
{
    [SerializeField] private SelectionHandler selectionHandler;
    [SerializeField] private GameObject cityPrefab;

    public int numberOfCities = 0;

    public void BuildCity(Player owner, Tile tile)
    {
        if (tile.city != null)
        {
            Debug.Log("city alr built");
            return;
        }

        tile.owner = owner;

        int moneyToTake = Global.newCityResourceCost[numberOfCities, 0];
        int woodToTake = Global.newCityResourceCost[numberOfCities, 1];
        int stoneToTake = Global.newCityResourceCost[numberOfCities, 2];

        if (!owner.TakeResources(moneyToTake, woodToTake, stoneToTake))
        {
            Debug.Log("not enough resources");
            return;
        }

        GameObject cityObj = Instantiate(cityPrefab, tile.transform.position, Quaternion.identity);
        City city = cityObj.AddComponent<City>();
        city.owner = owner;
        city.tile = tile;
        tile.city = city;
        cityObj.transform.SetParent(tile.transform);
        city.ChangeSize(1);
        owner.citys.Add(city);
    }
}
