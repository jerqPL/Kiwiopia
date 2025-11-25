
using UnityEngine;
using System.Collections.Generic;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private CityHandler cityHandler;
    [SerializeField] private UnitsHandler unitsHandler;
    [SerializeField] private SelectionHandler selectionHandler;

    [SerializeField] private RectTransform tileMenu;
    [SerializeField] private RectTransform cityMenu;
    [SerializeField] private RectTransform unitMenu;


    private List<RectTransform> possible_menus = new List<RectTransform>();

    private void Start()
    {
        DisableAll();
    }
    public void ClickedTile(Tile tile, int num_of_times)
    {
        DisableAll();

        possible_menus.Clear();
        possible_menus.Add(tileMenu);
        if (tile.city != null) possible_menus.Add(cityMenu);
        if (tile.unit != null) possible_menus.Add(unitMenu);

        ActivateMenu(num_of_times);
        //Debug.Log($"Clicked tile: {tile.transform.name}, {num_of_times} times");
    }

    private void DisableAll()
    {
        cityMenu.gameObject.SetActive(false);
        unitMenu.gameObject.SetActive(false);
        tileMenu.gameObject.SetActive(false);
    }

    private void ActivateMenu(int num_of_times)
    {
        int menu_index = num_of_times % (possible_menus.Count + 1);
        if (menu_index < possible_menus.Count) {
            possible_menus[menu_index].gameObject.SetActive(true);
        }
    }


    public void BuildCity()
    {
        cityHandler.BuildCity(selectionHandler.lastClickedTile);
    }

    public void RecruitUnit()
    {
        unitsHandler.RecruitUnit(selectionHandler.lastClickedTile);
    }

    public void DestroyUnit()
    {
        unitsHandler.DestroyUnit(selectionHandler.lastClickedTile);
    }
}
