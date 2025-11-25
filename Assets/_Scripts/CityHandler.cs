using UnityEngine;

public class CityHandler : MonoBehaviour
{
    [SerializeField] private SelectionHandler selectionHandler;
    [SerializeField] private GameObject cityPrefab;

    public void BuildCity(Tile tile)
    {
        if (tile.city != null)
        {
            Debug.Log("city alr built");
            return;
        }

        GameObject cityObj = Instantiate(cityPrefab, tile.transform.position, Quaternion.identity);
        City city = cityObj.AddComponent<City>();
        tile.city = city;
        cityObj.transform.SetParent(tile.transform);

    }
}
