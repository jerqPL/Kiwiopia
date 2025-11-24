using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public List<Tile> neighbors = new List<Tile>();

    public GameObject contents;
    public GameObject unit;

    public bool hasCity = false;
    public bool occupied = false;

    public float humidity;
    public float temperature;

    public string terrain;

    public Vector2 position;

    public bool inAnimation = false;
    public bool animate = false;

    public bool addCity = false;
    public bool recruitSwordman = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            foreach (Tile neighbor in neighbors)
            {
                if (!neighbor.inAnimation)
                {
                    neighbor.StartCoroutine(neighbor.AnimateTileClicked());
                }
            }
            animate = false;
        }

        if (addCity && !hasCity)
        {
            GameObject city = Instantiate(Global.cityPrefab, transform.position, Quaternion.identity);
            city.transform.parent = this.transform;
            contents = city;
            addCity = false;
            hasCity = true;
        }

        if (recruitSwordman && !occupied && hasCity)
        {
            GameObject swordman = Instantiate(Global.swordmanPrefab, transform.position, Quaternion.identity);
            swordman.transform.parent = this.transform;
            unit = swordman;
            recruitSwordman = false;
            occupied = true;

        }
    }

    private IEnumerator AnimateTileClicked()
    {
        inAnimation = true;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 4f;
        float animationDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        inAnimation = false;

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
