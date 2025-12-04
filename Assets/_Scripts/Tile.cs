using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Tile : MonoBehaviour
{
    public List<Tile> neighbors = new List<Tile>();

    public bool hasCity = false;
    public bool hasMountains = false;
    public bool hasForest = false;

    public float humidity;
    public float temperature;
    public float height;

    public string terrain;

    public Vector2 position;
    public Player owner;
    public Unit unit;
    public City city;
    public City underCity;

    public float generationTimer = 0;
    public float generationTimer1 = 0;

    public bool localPlayerHasSeen = false;

    private GameObject mountainGameObject;
    private GameObject forestGameObject;
    private Material material;

    private Material seenNotVisible;


    void Awake()
    {
        generationTimer = Random.Range(0, 100) * 0.01f;
        generationTimer1 = Random.Range(0, 100) * 0.01f;
    }

    void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (underCity != null)
        {
            generationTimer += Time.deltaTime;
            generationTimer1 += Time.deltaTime;
            if (!hasMountains && !hasForest)
            {
                int generated = (int)Mathf.Floor(generationTimer / Global.timePerCoinPerTile);

                generationTimer -= generated * Global.timePerCoinPerTile;
                SendToCity(generated, 0, 0);
            }
            else if (hasMountains && hasForest)
            {
                int generatedWood = (int)Mathf.Floor(generationTimer / Global.timePerLogPerForest);
                generationTimer -= generatedWood * Global.timePerLogPerForest;
                int generatedStone = (int)Mathf.Floor(generationTimer1 / Global.timePerStonePerMountain);
                generationTimer1 -= generatedStone * Global.timePerStonePerMountain;
                SendToCity(0, generatedWood, generatedStone);
            }
            else if (hasForest)
            {
                int generated = (int)Mathf.Floor(generationTimer / Global.timePerLogPerForest);
                generationTimer -= generated * Global.timePerLogPerForest;
                SendToCity(0, generated, 0);
            }
            else if (hasMountains)
            {
                int generated = (int)Mathf.Floor(generationTimer / Global.timePerStonePerMountain);
                generationTimer -= generated * Global.timePerStonePerMountain;
                SendToCity(0, 0, generated);
            }
        }
        if (underCity == null && unit != null)
        {
            generationTimer += Time.deltaTime;
            generationTimer1 += Time.deltaTime;
            if (!hasMountains && !hasForest)
            {
                int generated = (int)Mathf.Floor(generationTimer / Global.timePerCoinPerTile);

                generationTimer -= generated * Global.timePerCoinPerTile;
                SendToUnit(generated, 0, 0);
            }
            else if (hasMountains && hasForest)
            {
                int generatedWood = (int)Mathf.Floor(generationTimer / Global.timePerLogPerForest);
                generationTimer -= generatedWood * Global.timePerLogPerForest;
                int generatedStone = (int)Mathf.Floor(generationTimer1 / Global.timePerStonePerMountain);
                generationTimer1 -= generatedStone * Global.timePerStonePerMountain;
                SendToUnit(0, generatedWood, generatedStone);
            }
            else if (hasForest)
            {
                int generated = (int)Mathf.Floor(generationTimer / Global.timePerLogPerForest);
                generationTimer -= generated * Global.timePerLogPerForest;
                SendToUnit(0, generated, 0);
            }
            else if (hasMountains)
            {
                int generated = (int)Mathf.Floor(generationTimer / Global.timePerStonePerMountain);
                generationTimer -= generated * Global.timePerStonePerMountain;
                SendToUnit(0, 0, generated);
            }
        }
    }

    public void ApplyTerrain(Vector3 newTerrain)
    {
        humidity = newTerrain.x;
        temperature = newTerrain.y;
        height = newTerrain.z;

        GameObject mountain = null;
        if (Mathf.Pow(height * height * (1 - temperature), 0.33333f) > Global.heightToMountain)
        {
            mountain = Instantiate(Global.mountainPrefab, transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            mountain.transform.SetParent(transform);
            hasMountains = true;
        }
        mountainGameObject = mountain;

        GameObject forest = null;
        if (Mathf.Pow(humidity * humidity * temperature, 0.33333f) > Global.hotWetnessToForest)
        {
            forest = Instantiate(Global.forestPrefab, transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            forest.transform.SetParent(transform);
            hasForest = true;
        }
        forestGameObject = forest;

        int temperatureInt = Mathf.FloorToInt(temperature * 4f - 0.001f + 0.3f);
        int humidityInt = Mathf.FloorToInt(humidity * 5f - 0.001f);
        if (temperatureInt < 0)
        {
            temperatureInt = 0;
        }
        if (humidityInt < 0)
        {
            humidityInt = 0;
        }
        if (temperatureInt > 3)
        {
            temperatureInt = 3;
        }
        if (humidityInt > 4)
        {
            humidityInt = 4;
        }

        terrain = Global.terrainTypes[temperatureInt, humidityInt];


        foreach (Material mat in Global.terrainMaterials)
        {
            if (mat.name == terrain)
            {
                material = mat;
                GetComponent<Renderer>().material = mat;
                if (mountain != null)
                {
                    mountain.GetComponent<Renderer>().material = mat;
                }
                if (forest != null)
                {
                    Renderer rend = forest.GetComponent<Renderer>();
                    rend.material = mat;
                }
                break;
            }
        }
        Color seenNotVisibleColor = Color.Lerp(Global.notScoutedTileMaterial.color, material.color, 0.15f);
        seenNotVisible = new Material(material);
        seenNotVisible.color = seenNotVisibleColor;

        /*terrainType = (int)Mathf.Floor(terrain * 4f);
        GetComponent<Renderer>().material = Global.terrainMaterials[terrainType];*/
    }

    void SendToCity(int money, int wood, int stone)
    {
        underCity.RecieveResources(money, wood, stone);
    }

    void SendToUnit(int money, int wood, int stone)
    {
        unit.owner.RecieveResources(money, wood, stone);
    }

    public void SetVisibility(bool visible)
    {
        
        if (visible) localPlayerHasSeen = true;
        if (mountainGameObject != null)
        {
            mountainGameObject.SetActive(visible || localPlayerHasSeen);
            mountainGameObject.GetComponent<Renderer>().material = visible ? material : (localPlayerHasSeen ? seenNotVisible : Global.notScoutedTileMaterial);
        }
        if (forestGameObject != null)
        {
            forestGameObject.SetActive(visible || localPlayerHasSeen);
            forestGameObject.GetComponent<Renderer>().material = visible ? material : (localPlayerHasSeen ? seenNotVisible : Global.notScoutedTileMaterial);
        }
        GetComponent<Renderer>().material = visible ? material : (localPlayerHasSeen ? seenNotVisible : Global.notScoutedTileMaterial);
    }

    public void SetUnit(Unit uni)
    {
        unit = uni;
    }

    public Vector3[] GetCorners()
    {
        Vector3[] corners = new Vector3[6];
        float r = 1;
        Vector3 c = transform.position;

        // 0°, 60°, 120°, 180°, 240°, 300°
        for (int i = 0; i < 6; i++)
        {
            float angle = 60f * i;
            float rad = angle * Mathf.Deg2Rad;

            corners[i] = new Vector3(
                Mathf.Round((c.x + r * Mathf.Cos(rad))*1000)/1000,
                c.y,
                Mathf.Round((c.z + r * Mathf.Sin(rad))*1000)/1000
            );
        }

        return corners;
    }
}
