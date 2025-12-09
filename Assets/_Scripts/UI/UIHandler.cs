
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

public class UIHandler : NetworkBehaviour
{
    [SerializeField] private RectTransform tileMenu;
    [SerializeField] private RectTransform cityMenu;
    [SerializeField] private RectTransform cityMenuUnits;

    [SerializeField] private RectTransform unitMenu;


    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    [SerializeField] private RectTransform wonScreen;
    [SerializeField] private RectTransform lostScreen;

    [SerializeField] private RectTransform gameMenu;
    [SerializeField] private RectTransform mainMenu;
    [SerializeField] private RectTransform lobbyMenu;
    [SerializeField] private RectTransform gameUI;


    [SerializeField] private RectTransform JoinStatusConnecting;
    [SerializeField] private RectTransform JoinStatusError;
    [SerializeField] private RectTransform JoinStatusSuccess;

    [SerializeField] private RectTransform CreateStatusConnecting;
    [SerializeField] private RectTransform CreateStatusError;
    [SerializeField] private RectTransform CreateStatusSuccess;

    [SerializeField] private RectTransform startButton;

    [SerializeField] private TMP_Text errorText;

    [SerializeField] private RectTransform uIPlayers;
    [SerializeField] private GameObject uiPlayerPrefab;

    private List<KeyValuePair<int, RectTransform>> menus = new List<KeyValuePair<int, RectTransform>>();

    private Unit clickedUnit;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            startButton.gameObject.SetActive(false);
        }
        if (IsServer)
        {
            startButton.gameObject.SetActive(true);
        }
    }

    public GameObject AddPlayer(string playerID, Color color)
    {
        GameObject newPlayer = Instantiate(uiPlayerPrefab);
        newPlayer.GetComponent<PlayerUI>().playerID.text = playerID;
        newPlayer.GetComponent<PlayerUI>().background.color = color;
        newPlayer.transform.SetParent(uIPlayers);
        return newPlayer;
    }


    [ClientRpc]
    public void HideGameMenuClientRpc()
    {
        gameMenu.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
    }

    public void SwichToLobbyMenu()
    {
        mainMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(true);
    }

    private void Start()
    {
        DisableAll();
        DisableJoinStatus();
    }

    public void SetErrorText(string text)
    {
        errorText.text = text;
    }

    public void DisableJoinStatus()
    {
        JoinStatusConnecting.gameObject.SetActive(false);
        JoinStatusError.gameObject.SetActive(false);
        JoinStatusSuccess.gameObject.SetActive(false);
    }

    public void EnableJoinStatusConnecting()
    {
        DisableJoinStatus();
        JoinStatusConnecting.gameObject.SetActive(true);
    }

    public void EnableJoinStatusError()
    {
        DisableJoinStatus();
        JoinStatusError.gameObject.SetActive(true);
    }

    public void EnableJoinStatusSuccess()
    {
        DisableJoinStatus();
        JoinStatusSuccess.gameObject.SetActive(true);
    }

    public void DisableCreateStatus()
    {
        CreateStatusConnecting.gameObject.SetActive(false);
        CreateStatusError.gameObject.SetActive(false);
        CreateStatusSuccess.gameObject.SetActive(false);
    }

    public void EnableCreateStatusConnecting()
    {
        DisableCreateStatus();
        CreateStatusConnecting.gameObject.SetActive(true);
    }

    public void EnableCreateStatusError()
    {
        DisableCreateStatus();
        CreateStatusError.gameObject.SetActive(true);
    }

    public void EnableCreateStatusSuccess()
    {
        DisableCreateStatus();
        CreateStatusSuccess.gameObject.SetActive(true);
    }

    public void ClickedTile(Tile tile, int num_of_times)
    {
        if (tile.unit != null)
        {
            cityMenuUnits.gameObject.SetActive(false);
            clickedUnit = tile.unit;
        }
        else
        {
            cityMenuUnits.gameObject.SetActive(true);
            clickedUnit = null;
        }

        if (tile == null) return;
        DisableAll();

        menus.Clear();
        if ((tile.unit != null && tile.unit.isDead == false) && tile.unit.owner == Global.playerHandler.GetLocalPlayer() && tile.unit.isMoving.Value) menus.Add(new KeyValuePair<int, RectTransform>(3, unitMenu));
        if ((tile.unit != null && tile.unit.isDead == false) && tile.unit.owner == Global.playerHandler.GetLocalPlayer()) menus.Add(new KeyValuePair<int, RectTransform>(1 ,tileMenu));
        if (tile.city != null && tile.city.owner == Global.playerHandler.GetLocalPlayer()) menus.Add(new KeyValuePair<int, RectTransform>(2, cityMenu));
        if ((tile.unit != null && tile.unit.isDead == false) && tile.unit.owner == Global.playerHandler.GetLocalPlayer() && !tile.unit.isMoving.Value) menus.Add(new KeyValuePair<int, RectTransform>(3, unitMenu));

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
        int menu_index = num_of_times % (menus.Count + 1);
        if (menu_index < menus.Count) {
            menus[menu_index].Value.gameObject.SetActive(true);
            Global.selectionHandler.state = menus[menu_index].Key;
        }
        else
        {
            Global.selectionHandler.state = 0;
        }
    }


    public void BuildCity()
    {
        Global.cityHandler.BuildCityServerRpc(Global.playerHandler.GetLocalPlayerIndex(), Global.tilesHandler.GetIndexOf(Global.selectionHandler.lastClickedTile));
    }

    public void RecruitUnit(int type)
    {
        Global.unitsHandler.StartRecruitingServerRpc(Global.playerHandler.GetLocalPlayerIndex(), Global.tilesHandler.GetIndexOf(Global.selectionHandler.lastClickedTile), type);
        cityMenuUnits.gameObject.SetActive(false);
    }

    public void DestroyUnit()
    {
        Global.unitsHandler.KillUnitServerRpc(Global.unitsHandler.GetIndexOf(clickedUnit));
        Global.selectionHandler.state = 0;

    }

    public void StopUnit()
    {
        clickedUnit.CancelMovementServerRpc(Global.unitsHandler.GetIndexOf(clickedUnit));
    }

    public void UpgradeCity()
    {
        Global.selectionHandler.lastClickedTile.city.ChangeSizeServerRpc(Global.selectionHandler.lastClickedTile.city.size.Value + 1);
    }

    public void UpdateMoneyText(int value)
    {
        moneyText.text = value.ToString();
    }

    public void UpdateWoodText(int value)
    {
        woodText.text = value.ToString();
    }

    public void UpdateStoneText(int value)
    {
        stoneText.text = value.ToString();
    }

    public void Lost()
    {
        DisableAll();
        lostScreen.gameObject.SetActive(true);
    }

    public void Won()
    {
        DisableAll();
        wonScreen.gameObject.SetActive(true);
    }
}
