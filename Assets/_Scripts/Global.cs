using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static string[,] terrainTypes = {
        {"lodowiec",     "lodowiec",         "lodowiec",     "lodowiec",     "lodowiec" },
        {"sucha tundra", "mokra tundra",     "mokra tundra", "mokra tundra", "deszczowa tundra" },
        {"pustynia",     "step",             "suchy las",    "mokry las",    "las deszczowy"},
        {"pustynia",     "pustynne zaroœla", "suchy las",    "mokry las",    "las deszczowy"}
    };

    public static Material[] terrainMaterials;
    [SerializeField] private Material[] terrainMaterialReference;

    public static LineRenderer lineRendererPrefab;
    [SerializeField] private LineRenderer lineRendererPrefabReference;

    public static float heightToMountain;
    [SerializeField] private float heightToMoutainReference;

    public static GameObject mountainPrefab;
    [SerializeField] private GameObject mountainPrefabReference;

    public static float hotWetnessToForest;
    [SerializeField] private float hotWetnessToForestReference;

    public static GameObject forestPrefab;
    [SerializeField] private GameObject forestPrefabReference;



    public static Material completedLineMaterial;
    [SerializeField] private Material completedLineMaterialReference;

    public static Material inProgressLineMaterial;
    [SerializeField] private Material inProgressLineMaterialReference;

    public static Material notScoutedTileMaterial;
    [SerializeField] private Material notScoutedTileMaterialReference;


    public static int startingMoney; //money, wood, stone;
    [SerializeField] private int startingMoneyReference;


    public static float timePerCoinPerTile;
    [SerializeField] private float timePerCoinPerTileReference;

    public static float timePerLogPerForest;
    [SerializeField] private float timePerLogPerForestReference;

    public static float timePerStonePerMountain;
    [SerializeField] private float timePerStonePerMountainReference;




    public static UnitType[] unitTypes;
    [SerializeField] private UnitType[] unitTypesReference;


    //SCRIPTS
    public static TilesHandler tilesHandler;
    [SerializeField] private TilesHandler tilesHandlerReference;

    public static PlayerHandler playerHandler;
    [SerializeField] private PlayerHandler playerHandlerReference;

    public static TerrainGeneration terrainGeneration;
    [SerializeField] private TerrainGeneration terrainGenerationReference;

    public static UIHandler uIHandler;
    [SerializeField] private UIHandler uIHandlerReference;

    public static CameraMovement cameraMovement;
    [SerializeField] private CameraMovement cameraMovementReference;

    public static UnitsHandler unitsHandler;
    [SerializeField] private UnitsHandler unitsHandlerReference;

    public static SelectionHandler selectionHandler;
    [SerializeField] private SelectionHandler selectionHandlerReference;
    public enum SelectionHandlerState { None, UnitMoving}

    public static CityHandler cityHandler;
    [SerializeField] private CityHandler cityHandlerReference;

    public static float lineHegithAboveTiles = 0.05f;

    public static float timeToCaptureCity = 20f;

    public static SessionHandler sessionHandler;
    [SerializeField] private SessionHandler sessionHandlerReference;

    public static NetworkUI networkUI;
    [SerializeField] private NetworkUI networkUIReference;

    public static Color localPlayerColor;
    [SerializeField] private Color localPlayerColorReference;


    public static int newCityResourceCost = 10;

    public static OwnershipHandler ownershipHandler;
    [SerializeField] private OwnershipHandler ownershipHandlerReference;

    public static event System.Action AfterInitialization;

    void Awake()
    {
        terrainMaterials = terrainMaterialReference;
        lineRendererPrefab = lineRendererPrefabReference;
        heightToMountain = heightToMoutainReference;
        mountainPrefab = mountainPrefabReference;
        forestPrefab = forestPrefabReference;
        hotWetnessToForest = hotWetnessToForestReference;
        completedLineMaterial = completedLineMaterialReference;
        inProgressLineMaterial = inProgressLineMaterialReference;
        notScoutedTileMaterial = notScoutedTileMaterialReference;

        startingMoney = startingMoneyReference;

        timePerCoinPerTile = timePerCoinPerTileReference;
        timePerLogPerForest = timePerLogPerForestReference;
        timePerStonePerMountain = timePerStonePerMountainReference;
        networkUI = networkUIReference;

        unitTypes = unitTypesReference;
        localPlayerColor = localPlayerColorReference;

        //SCRIPTS
        tilesHandler = tilesHandlerReference;
        playerHandler = playerHandlerReference;
        terrainGeneration = terrainGenerationReference;
        uIHandler = uIHandlerReference;
        cameraMovement = cameraMovementReference;
        unitsHandler = unitsHandlerReference;
        selectionHandler = selectionHandlerReference;
        cityHandler = cityHandlerReference;
        sessionHandler = sessionHandlerReference;
        ownershipHandler = ownershipHandlerReference;

        AfterInitialization?.Invoke();
    }

    public static Vector3 ZeroYVector3(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    public static Vector3 AddToYVector3(Vector3 vector, float value) { return new Vector3(vector.x, vector.y + value, vector.z); }

    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    public static bool isLocal()
    {
        return NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;
    }
}
