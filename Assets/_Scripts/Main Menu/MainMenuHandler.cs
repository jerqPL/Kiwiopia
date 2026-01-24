using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using static Global;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Player localPlayer;
    [SerializeField] private Transform cameraPositionMain;
    [SerializeField] private Transform cameraPositionSingleplayer;
    [SerializeField] private Transform cameraPositionMultiplayer;

    private void Awake()
    {
        AfterInitialization += () => {
            Camera.main.transform.position = cameraPositionMain.position;
            tilesHandler.GenerateTiles();
            Tile centerTile = tilesHandler.centerTile.GetComponent<Tile>();
            Player player = Instantiate(localPlayer);
            Unit unit = player.SpawnPlayer(centerTile);
            City city = cityHandler.BuildCityLocally(playerHandler.GetIndexOf(player), tilesHandler.GetIndexOf(centerTile.neighbors[0]));
            unit.RotateTowards(city.tile.transform.position);
            List<Tile> path = new List<Tile>();
            path.Add(tileTranscriptToTile(""));
            path.Add(tileTranscriptToTile("4"));
            path.Add(tileTranscriptToTile("40"));

            Tile multiplayerUnitSpawn = tileTranscriptToTile("40444");
            Unit multiplayerUnit = unitsHandler.RecruitUnit(
                playerHandler.GetIndexOf(player),
                tilesHandler.GetIndexOf(multiplayerUnitSpawn),
                1
            );

            List<Tile> multiplayerUnitPath = new List<Tile>();
            multiplayerUnitPath.Add(tileTranscriptToTile("40444"));
            multiplayerUnitPath.Add(tileTranscriptToTile("4044"));
            multiplayerUnitPath.Add(tileTranscriptToTile("404"));


            Camera.main.transform.position = cameraPositionMain.position;
            Camera.main.transform.rotation = cameraPositionMain.rotation;

            unit.unitMovement.RequestMove(path);
            multiplayerUnit.unitMovement.RequestMove(multiplayerUnitPath);
            StartCoroutine(animateCameraMovement(cameraPositionMultiplayer, (path.Count - 1) * (1/unit.unitType.speed)));
        };
    }

    private IEnumerator animateCameraMovement(Transform target, float duration)
    {
        Transform cam = Camera.main.transform;

        Vector3 startPosition = cam.position;
        Vector3 targetPosition = target.position;

        Quaternion startRotation = cam.rotation;
        Quaternion targetRotation = target.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            cam.position = Vector3.Lerp(startPosition, targetPosition, t);
            cam.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.position = targetPosition;
        cam.rotation = targetRotation;
    }

    Tile tileTranscriptToTile(string transcript)
    {
        Tile current = tilesHandler.centerTile.GetComponent<Tile>();
        int index = 0;
        while(index < transcript.Length)
        {
            current = current.neighbors[int.Parse(transcript[index].ToString())];
            index++;
        }
        return current;
    }

}
