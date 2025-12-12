
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class UIHandler : NetworkBehaviour
{
    [Header("Resources UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    [Header("City UI")]
    [SerializeField] private TMP_Text cityTier;
    [SerializeField] private Image cityOwner;
    [SerializeField] private TMP_Text cityMoneyYield;
    [SerializeField] private TMP_Text cityWoodYield;
    [SerializeField] private TMP_Text cityStoneYield;
    [SerializeField] private RectTransform recruitmentQueueLayoutGroup;
    [SerializeField] private List<RectTransform> unitPrefabs;
    private List<RectTransform> unitsInQueue = new List<RectTransform>();

    [Header("Tile UI")]
    [SerializeField] private TMP_Text tileType;
    [SerializeField] private Image tileOwner;
    [SerializeField] private TMP_Text unknownTileOwner;
    [SerializeField] private TMP_Text tileMoneyYield;
    [SerializeField] private TMP_Text tileWoodYield;
    [SerializeField] private TMP_Text tileStoneYield;

    [Header("Unit UI")]
    [SerializeField] private TMP_Text unitType;
    [SerializeField] private Image unitOwner;
    [SerializeField] private TMP_Text unitHealth;
    [SerializeField] private TMP_Text unitAttackCooldown;
    [SerializeField] private TMP_Text unitRange;
    [SerializeField] private TMP_Text unitDamage;
    [SerializeField] private TMP_Text unitResistance;
    [SerializeField] private TMP_Text unitSpeed;
    [SerializeField] private TMP_Text unitCanClimb;
    [SerializeField] private TMP_Text unitUpkeapCost;
    [SerializeField] private TMP_Text unitScoutDistance;
    [SerializeField] private TMP_Text unitDescription;

    [Header("Menus")]
    [SerializeField] private RectTransform tileMenu;
    [SerializeField] private RectTransform cityMenu;
    [SerializeField] private RectTransform unitMenu;

    [Header("Endgame Screens")]
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
        DisableAllMenus();
        DisableJoinStatus();
        gameMenu.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
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

    private void DisableAllMenus()
    {
        cityMenu.gameObject.SetActive(false);
        unitMenu.gameObject.SetActive(false);
        tileMenu.gameObject.SetActive(false);
    }
    public void ClickedTile(Tile tile, int num_of_times)
    {
        DisableAllMenus();
        if (tile != null)
        {
            tileMenu.gameObject.SetActive(true);
            UpdateTileMenu(tile);
            if (tile.unit != null)
            {
                unitMenu.gameObject.SetActive(true);
                clickedUnit = tile.unit;
                UpdateUnitMenu(tile.unit);
            }
            if (tile.city != null)
            {
                cityMenu.gameObject.SetActive(true);
                UpdateCityMenu(tile.city);
            }
        }
    }

    private void UpdateTileMenu(Tile tile)
    {
        if (!tile.localPlayerHasSeen)
        {
            tileType.text = "Unknown";
        }
        else {
            if (tile.hasForest && tile.hasMountains)
            {
                tileType.text = "Forest Mountains";
            }
            else if (tile.hasMountains)
            {
                tileType.text = "Mountains";
            }
            else if (tile.hasForest)
            {
                tileType.text = "Forest";
            }
            else
            {
                tileType.text = "Field";
            }
        }

        if (!tile.localPlayerHasSeen)
        {
            tileMoneyYield.text = "?";
            tileWoodYield.text = "?";
            tileStoneYield.text = "?";
        }
        else
        {
            if (tile.hasForest && tile.hasMountains)
            {
                tileMoneyYield.text = "0";
                tileWoodYield.text = ((int)(1f / Global.timePerLogPerForest * 10) / 10f).ToString();
                tileStoneYield.text = ((int)(1f / Global.timePerStonePerMountain * 10) / 10f).ToString();
            }
            else if (tile.hasMountains)
            {
                tileMoneyYield.text = "0";
                tileWoodYield.text = "0";
                tileStoneYield.text = ((int)(1f / Global.timePerStonePerMountain * 10) / 10f).ToString();
            }
            else if (tile.hasForest)
            {
                tileMoneyYield.text = "0";
                tileWoodYield.text = ((int)(1f / Global.timePerLogPerForest * 10) / 10f).ToString();
                tileStoneYield.text = "0";
            }
            else
            {
                tileMoneyYield.text = ((int)(1f / Global.timePerCoinPerTile * 10) / 10f).ToString();
                tileWoodYield.text = "0";
                tileStoneYield.text = "0";
            }
        }

        if (tile.owner == null)
        {
            tileOwner.gameObject.SetActive(false);
            unknownTileOwner.gameObject.SetActive(true);
        }
        else
        {
            tileOwner.gameObject.SetActive(true);
            unknownTileOwner.gameObject.SetActive(false);
            tileOwner.color = Global.playerHandler.GetPlayerColor(Global.playerHandler.GetIndexOf(tile.owner));
        }  
    }

    private void UpdateUnitMenu(Unit unit)
    {

        unitType.text = unit.unitType.name;
        unitOwner.color = Global.playerHandler.GetPlayerColor(Global.playerHandler.GetIndexOf(unit.owner));
        unitHealth.text = unit.health.Value.ToString() + "/" + unit.unitType.health.ToString();
        unitAttackCooldown.text = ((int)(unit.attackCooldown * 10)/10f).ToString() + "/" + ((int)(unit.unitType.attackCooldown * 10) / 10f).ToString();
        unitRange.text = unit.unitType.range.ToString();
        unitDamage.text = unit.unitType.damage.ToString();
        unitResistance.text = unit.unitType.resistance.ToString();
        unitSpeed.text = ((int)(1/unit.unitType.speed * 10)/10f).ToString();
        unitCanClimb.text = unit.unitType.canClimb ? "yes" : "no";
        unitUpkeapCost.text = ((int)(1 / unit.unitType.timePerCoin * 10) / 10f).ToString();
        unitScoutDistance.text = unit.unitType.scoutDistance.ToString();
        unitDescription.text = unit.unitType.description;
    }

    public void UpdateCityMenu(City city)
    {
        cityTier.text = "Tier: " + city.size.Value.ToString();
        cityOwner.color = Global.playerHandler.GetPlayerColor(Global.playerHandler.GetIndexOf(city.owner));

        float moneyYield = 0f;
        float woodYield = 0f;
        float stoneYield = 0f;

        foreach(Tile tile in city.cityTiles)
        {
            if (tile.hasForest && tile.hasMountains)
            {
                woodYield += 1f / Global.timePerLogPerForest;
                stoneYield += 1f / Global.timePerStonePerMountain;
            }
            else if (tile.hasMountains)
            {
                stoneYield += 1f / Global.timePerStonePerMountain;
            }
            else if (tile.hasForest)
            {
                woodYield += 1f / Global.timePerLogPerForest;
            }
            else
            {
                moneyYield += 1f / Global.timePerCoinPerTile;
            }
        }

        cityMoneyYield.text = ((int)(moneyYield * 10) / 10f).ToString();
        cityWoodYield.text = ((int)(woodYield * 10) / 10f).ToString();
        cityStoneYield.text = ((int)(stoneYield * 10) / 10f).ToString();

        foreach(RectTransform unitQ in unitsInQueue)
        {
            Destroy(unitQ.gameObject);
        }
        unitsInQueue.Clear();

        for(int i = 0; i < city.recruitmentQueue.Count; i++)
        {
            int unitQ = city.recruitmentQueue[i];
            RectTransform newUnitQ = Instantiate(unitPrefabs[unitQ]);
            newUnitQ.SetParent(recruitmentQueueLayoutGroup);
            unitsInQueue.Add(newUnitQ);

            int unitIndex = i;
            newUnitQ.GetComponent<Button>().onClick.AddListener(() => 
            {   
                Global.unitsHandler.RemoveFromRecruitmentQueueServerRpc(
                    Global.playerHandler.GetLocalPlayerIndex(),
                    Global.tilesHandler.GetIndexOf(Global.selectionHandler.lastClickedTile),
                    unitsInQueue.IndexOf(newUnitQ));
                unitsInQueue.Remove(newUnitQ);
                Destroy(newUnitQ.gameObject);
            });
        }
    }


    public void BuildCity()
    {
        if (clickedUnit != null)
        {
            Global.cityHandler.BuildCityServerRpc(Global.playerHandler.GetLocalPlayerIndex(), clickedUnit.tileIndex.Value);
        }
        else
        {
            Global.cityHandler.BuildCityServerRpc(Global.playerHandler.GetLocalPlayerIndex(), Global.tilesHandler.GetIndexOf(Global.selectionHandler.lastClickedTile));
        }
        ClickedTile(Global.selectionHandler.lastClickedTile, 0);
    }

    public void AddUnitToQueue(int type)
    {
        Global.unitsHandler.AddToRecruitmentQueueServerRpc(Global.playerHandler.GetLocalPlayerIndex(), Global.tilesHandler.GetIndexOf(Global.selectionHandler.lastClickedTile), type);
        UpdateCityMenu(Global.selectionHandler.lastClickedTile.city);
    }

    public void RemoveUnitFromQueue(int index)
    {
        Global.unitsHandler.RemoveFromRecruitmentQueueServerRpc(Global.playerHandler.GetLocalPlayerIndex(), Global.tilesHandler.GetIndexOf(Global.selectionHandler.lastClickedTile), index);
        UpdateCityMenu(Global.selectionHandler.lastClickedTile.city);
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
        DisableAllMenus();
        lostScreen.gameObject.SetActive(true);
    }

    public void Won()
    {
        DisableAllMenus();
        wonScreen.gameObject.SetActive(true);
    }

    public void FocusOnTile()
    {
        Global.cameraMovement.focousOnTile(Global.selectionHandler.lastClickedTile);
    }

    public void MoveUnit()
    {
        Global.selectionHandler.state = 3;
    }
}
