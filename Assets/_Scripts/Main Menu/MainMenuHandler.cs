using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Android.Gradle;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.VisualScripting;
using UnityEngine;
using static Global;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Player localPlayer;
    [SerializeField] private Transform cameraPositionMain;
    [SerializeField] private Transform cameraPositionSingleplayer;
    [SerializeField] private Transform cameraPositionMultiplayer;

    private string mainUnitPositionTranscript = "";
    private string multiplayerUnitPositionTranscript = "";
    private string multiplayerUnitBasePositionTranscript = "404444";
    private string mainUnitBasePositionTranscript = "";
    private string mainUnitMultiplayerPositionTranscript = "40";

    private Unit mainUnit;
    private Unit multiplayerUnit;

    private void Awake()
    {
        AfterInitialization += () =>
        {
            Camera.main.transform.position = cameraPositionMain.position;
            tilesHandler.GenerateTiles();
            Tile centerTile = tilesHandler.centerTile.GetComponent<Tile>();
            Player player = Instantiate(localPlayer);
            
            mainUnit = player.SpawnPlayer(centerTile);
            mainUnitPositionTranscript = mainUnitBasePositionTranscript;

            City city = cityHandler.BuildCityLocally(playerHandler.GetIndexOf(player), tilesHandler.GetIndexOf(centerTile.neighbors[0]));
            mainUnit.RotateTowards(city.tile.transform.position);

            Tile multiplayerUnitSpawn = tileTranscriptToTile(multiplayerUnitBasePositionTranscript);
            multiplayerUnit = unitsHandler.RecruitUnit(
                playerHandler.GetIndexOf(player),
                tilesHandler.GetIndexOf(multiplayerUnitSpawn),
                1
            );
            multiplayerUnitPositionTranscript = multiplayerUnitBasePositionTranscript;
            Camera.main.transform.position = cameraPositionMain.position;
            Camera.main.transform.rotation = cameraPositionMain.rotation;
        };
    }

    public void moveUnitsToMultiplayer()
    {
        List<Tile> mainUnitPathToBasePosition = transcriptToList(mainUnitPositionTranscript, true);
        List<Tile> mainUnitPathToMultiplayerPosition = transcriptToList(mainUnitMultiplayerPositionTranscript, false);
        List<Tile> mainUnitPath = mainUnitPathToBasePosition.Concat(mainUnitPathToMultiplayerPosition).ToList();

        mainUnit.unitMovement.RequestMove(mainUnitPath);
        
        
        List<Tile> multiplayerUnitPath = transcriptToList(multiplayerUnitPositionTranscript, true, 4);

        multiplayerUnit.unitMovement.RequestMove(multiplayerUnitPath);
        StartCoroutine(animateCameraMovement(cameraPositionMultiplayer, (mainUnitPath.Count - 1) * (1/mainUnit.unitType.speed)));   
    }

    public void moveUnitsToMain()
    {
        List<Tile> multiplayerUnitPath = transcriptToList(multiplayerUnitPositionTranscript, false, 3);
        Debug.Log("main");
        List<Tile> mainUnitPathToMain = transcriptToList(mainUnitPositionTranscript, true);

        mainUnit.unitMovement.RequestMove(mainUnitPathToMain);
        multiplayerUnit.unitMovement.RequestMove(multiplayerUnitPath);
        StartCoroutine(animateCameraMovement(cameraPositionMain, (mainUnitPathToMain.Count - 1) * (1/mainUnit.unitType.speed)));
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
        while (index < transcript.Length)
        {
            current = current.neighbors[int.Parse(transcript[index].ToString())];
            index++;
        }
        return current;
    }

    public List<Tile> transcriptToList(string transcript, bool fromEnd, int length = -1)
    {
        Debug.Log("Transcript: " + transcript + ", fromEnd: " + fromEnd);
        List<Tile> result = new List<Tile>();
        Tile current = tilesHandler.centerTile.GetComponent<Tile>();
        if (!fromEnd)
        {
            Debug.Log("Processing from start");
            int index = length == -1 ? 0 : length;
            while (index < transcript.Length+1)
            {
                current = tileTranscriptToTile(transcript.Substring(0, index));
                Debug.Log(transcript.Substring(0, index));
                result.Add(current);
                index++;
            
            }
            
            Debug.Log("Result: " + string.Join(", ", result.Select(t => t)));
            return result;
        }
        else
        {
            int index = transcript.Length - 1;
            while (index >= 0 && (length == -1 || transcript.Length - 1 - index + 1 < length))
            {
                current = tileTranscriptToTile(transcript.Substring(0, index));
                Debug.Log(tileTranscriptToTile(transcript.Substring(0, index)));
                result.Add(current);
                index--;
            }
            return result;
        }
    }
}
