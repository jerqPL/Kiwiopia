using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField] private int humidityXOffset;
    [SerializeField] private int humidityZOffset;

    [SerializeField] private int temperatureXOffset;
    [SerializeField] private int temperatureZOffset;

    [SerializeField] private float humidityXScale;
    [SerializeField] private float humidityZScale;

    [SerializeField] private float temperatureXScale;
    [SerializeField] private float temperatureZScale;
    public void GenerateOffset()
    {
        humidityXOffset = Random.Range(100, 1000);
        humidityZOffset = Random.Range(100, 1000);
        temperatureXOffset = Random.Range(100, 1000);
        temperatureZOffset = Random.Range(100, 1000);
    }

    public Vector2 GetTerrainAtPos(float posX, float posZ)
    {
        
        return new Vector2( Mathf.PerlinNoise(humidityXScale * (humidityXOffset + posX), humidityZScale * (humidityZOffset + posZ)),
                            Mathf.PerlinNoise(temperatureXScale * (temperatureXOffset + posX), temperatureZScale * (temperatureZOffset + posZ)));
    }
}
