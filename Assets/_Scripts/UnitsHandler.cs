using UnityEngine;

public class UnitsHandler : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    
    public void RecruitUnit(Tile tile)
    {
        GameObject unitGameObj = Instantiate(unitPrefab, tile.transform.position, Quaternion.identity);
        Unit unit = unitGameObj.AddComponent<Unit>();
        unit.name = "speraman";
        tile.unit = unit;
        unit.transform.parent = tile.transform;
    }

    public void DestroyUnit(Tile tile)
    {
        if (tile.unit != null)
        {
            Destroy(tile.unit.gameObject);
            Destroy(tile.unit);
            tile.unit = null;
        }
    }
}
