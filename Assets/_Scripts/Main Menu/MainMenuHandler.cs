using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Global;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Player localPlayer;
    [SerializeField] private Transform cameraPositionMain;
    [SerializeField] private Transform cameraPositionSingleplayer;
    [SerializeField] private Transform cameraPositionMultiplayer;

    [SerializeField] private MainMenuUI mainMenuUI;

    public enum UnitPosition
    {
        MAIN,
        MULTIPLAYER,
        SINGLEPLAYER
    }

    private string multiplayerUnitBasePosition = "40444";

    private List<string> mainToMultiplayerPath = new List<string> {
        "",
        "4",
        "40"
    };

    private List<string> mainToSingleplayerPath = new List<string>
    {
        "",
        "1",
        "10",
    };

    private List<string> multiplayerToMainPath = new List<string>
    {
        "40",
        "4",
        ""
    };

    private List<string> singleplayerToMainPath = new List<string>
    {
        "10",
        "1",
        ""
    };

    private List<string> multiplayerUnitToBasePath = new List<string>
    {
        "404",
        "4044",
        "40444",
    };

    private List<string> multiplayerUnitToMultiplayerPath = new List<string>
    {
        "40444",
        "4044",
        "404",
    };
    private List<string> mainUnitgoFromMultiplayer = new List<string> {
        "40",
        "405",
        "4055",
        "40555",
        "405555"
    };
    private List<string> multiplayerUnitGoFromMultiplayer = new List<string> {
        "404",
        "4045",
        "40455",
        "404555",
        "4045555"
    };
    private List<string> mainUnitGoFromSingleplayer = new List<string>
    {
        "4012",
        "40122",
        "401222",
        "4012222",
        "40122222",
    };


    public UnitPosition currentUnitPosition = UnitPosition.MAIN;

    private Unit mainUnit;
    private Unit multiplayerUnit;

    public System.Action AfterCameraMoved;


    public void goFromMultiplayer()
    {
        if (currentUnitPosition != UnitPosition.MULTIPLAYER)
        {
            return;
        }
        List<Tile> path = mainUnitgoFromMultiplayer
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
        mainUnit.unitMovement.RequestMove(path);
        List<Tile> multiplayerUnitPath = multiplayerUnitGoFromMultiplayer
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
        mainUnit.unitMovement.RequestMove(path);
        multiplayerUnit.unitMovement.RequestMove(multiplayerUnitPath);
    }

    public void goFromSingleplayer()
    {
        if (currentUnitPosition != UnitPosition.SINGLEPLAYER)
        {
            return;
        }
        List<Tile> path = mainUnitGoFromSingleplayer
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
        mainUnit.unitMovement.RequestMove(path);
    }

    public void moveUnitsToMultiplayer()
    {
        if (currentUnitPosition == UnitPosition.MULTIPLAYER || currentUnitPosition == UnitPosition.SINGLEPLAYER)
        {
            return;
        }
        if (currentUnitPosition == UnitPosition.MAIN)
        {
            currentUnitPosition = UnitPosition.MULTIPLAYER;
            List<Tile> path = mainToMultiplayerPath
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
            mainUnit.unitMovement.RequestMove(path);
            StartCoroutine(animateCameraMovement(cameraPositionMultiplayer, (path.Count - 1) * (1 / mainUnit.unitType.speed)));
        }   
        List<Tile> multiplayerUnitPath = multiplayerUnitToMultiplayerPath
            .Select(transcript => tileTranscriptToTile(transcript))
            .ToList(); 
        multiplayerUnit.unitMovement.RequestMove(multiplayerUnitPath);
    }

    public void moveUnitsToSingleplayer()
    {
        if (currentUnitPosition == UnitPosition.SINGLEPLAYER || currentUnitPosition == UnitPosition.MULTIPLAYER)
        {
            return;
        }
        if (currentUnitPosition == UnitPosition.MAIN)
        {
            currentUnitPosition = UnitPosition.SINGLEPLAYER;
            List<Tile> path = mainToSingleplayerPath
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
            mainUnit.unitMovement.RequestMove(path);
            StartCoroutine(animateCameraMovement(cameraPositionSingleplayer, (path.Count - 1) * (1 / mainUnit.unitType.speed)));
        }
    }

    public void moveUnitsToMain()
    {
        if (currentUnitPosition == UnitPosition.MAIN)
        {
            return;
        }
        if (currentUnitPosition == UnitPosition.MULTIPLAYER)
        {
            currentUnitPosition = UnitPosition.MAIN;
            List<Tile> path = multiplayerToMainPath
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
            mainUnit.unitMovement.RequestMove(path);
            StartCoroutine(animateCameraMovement(cameraPositionMain, (path.Count - 1) * (1 / mainUnit.unitType.speed)));

            List<Tile> multiplayerUnitPath = multiplayerUnitToBasePath
            .Select(transcript => tileTranscriptToTile(transcript))
            .ToList();
            multiplayerUnit.unitMovement.RequestMove(multiplayerUnitPath);
        }
        if (currentUnitPosition == UnitPosition.SINGLEPLAYER)
        {
            currentUnitPosition = UnitPosition.MAIN;
            List<Tile> pathToMain = singleplayerToMainPath
                .Select(transcript => tileTranscriptToTile(transcript))
                .ToList();
            mainUnit.unitMovement.RequestMove(pathToMain);
            StartCoroutine(animateCameraMovement(cameraPositionMain, (pathToMain.Count - 1) * (1 / mainUnit.unitType.speed)));
        }
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
        AfterCameraMoved?.Invoke();
    }

    Tile tileTranscriptToTile(string transcript)
    {
        Tile current = tilesHandler.centerTile.GetComponent<Tile>();
        int index = 0;
        while (index < transcript.Length)
        {
            current = current.neighbours[int.Parse(transcript[index].ToString())];
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

    public void Quit()
    {
        mainUnit.health.health.Value = 0;
    }
}
