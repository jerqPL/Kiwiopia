using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public List<Tile> neighbors = new List<Tile>();

    public bool hasCity = false;
    public bool occupied = false;

    public float humidity;
    public float temperature;

    public string terrain;

    public Vector2 position;

    public Unit unit;
    public City city;

    void Start()
    {

    }

    public void ApplyTerrain(Vector2 newTerrain)
    {
        humidity = newTerrain.x;
        temperature = newTerrain.y;

        int temperatureInt = Mathf.FloorToInt(temperature * 4f - 0.001f -0.3f);
        int humidityInt = Mathf.FloorToInt(humidity * 5f - 0.001f);
        if (temperatureInt < 0)
        {
            temperatureInt = 0;
        }
        if (humidityInt < 0){ 
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

        foreach(Material mat in Global.terrainMaterials)
        {
            if (mat.name == terrain)
            {
                GetComponent<Renderer>().material = mat;
                break;
            }
        }

        /*terrainType = (int)Mathf.Floor(terrain * 4f);
        GetComponent<Renderer>().material = Global.terrainMaterials[terrainType];*/
    }
}
