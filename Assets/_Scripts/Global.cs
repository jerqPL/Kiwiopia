using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static GameObject cityPrefab;
    [SerializeField] private GameObject cityPrefabReference;

    public static GameObject swordmanPrefab;
    [SerializeField] private GameObject swordmanPrefabReference;

    public static string[,] terrainTypes = {
        {"pustynia",     "pustynne zaroœla", "suchy las",    "mokry las",    "las deszczowy"},
        {"pustynia",     "step",             "suchy las",    "mokry las",    "las deszczowy"},
        {"sucha tundra", "mokra tundra",     "mokra tundra", "mokra tundra", "deszczowa tundra" },
        {"lodowiec",     "lodowiec",         "lodowiec",     "lodowiec",     "lodowiec" }
    };

    public static Material[] terrainMaterials;
    [SerializeField] private Material[] terrainMaterialReference;

    void Awake()
    {
        cityPrefab = cityPrefabReference;
        swordmanPrefab = swordmanPrefabReference;
        terrainMaterials = terrainMaterialReference;
    }
}
