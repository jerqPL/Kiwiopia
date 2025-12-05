using NUnit.Framework;
using System.Collections.Generic;
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

    public static int startingWood; 
    [SerializeField] private int startingWoodReference;

    public static int startingStone;
    [SerializeField] private int startingStoneReference;


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

    public static CityHandler cityHandler;
    [SerializeField] private CityHandler cityHandlerReference;

    public static float lineHegithAboveTiles = 0.05f;


    public static int[,] newCityResourceCost = {
        {20, 5, 5},
        {40, 10, 10},
        {80, 20, 20 },
        {160, 40, 40 },
        {320, 80, 80 },
        {640, 160, 160 },
        {1280, 320, 320 }
    };

    public static int[,] cityUpgradeCostMultiplayer = {
        /*{2, 2, 2},
        {4, 4, 4},
        {8, 8, 8},
        {16, 16, 16},
        {32, 32, 32},
        {64, 64, 64},
        {128, 128, 128}*/
        {0,0,0 },
        {0,0,0 },
        {0,0,0 },
        {0,0,0 },
        {0,0,0 },
        {0,0,0 },
        {0,0,0 },
        {0,0,0 }
    };

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
        startingWood = startingWoodReference;
        startingStone = startingStoneReference;

        timePerCoinPerTile = timePerCoinPerTileReference;
        timePerLogPerForest = timePerLogPerForestReference;
        timePerStonePerMountain = timePerStonePerMountainReference;

        unitTypes = unitTypesReference;

        //SCRIPTS
        tilesHandler = tilesHandlerReference;
        playerHandler = playerHandlerReference;
        terrainGeneration = terrainGenerationReference;
        uIHandler = uIHandlerReference;
        cameraMovement = cameraMovementReference;
        unitsHandler = unitsHandlerReference;
        selectionHandler = selectionHandlerReference;
        cityHandler = cityHandlerReference;
    }

    public static Vector3 ZeroYVector3(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    public static Vector3 AddToYVector3(Vector3 vector, float value) { return new Vector3(vector.x, vector.y + value, vector.z); }
}
