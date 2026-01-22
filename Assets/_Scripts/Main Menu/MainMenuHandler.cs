using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using static Global;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Player localPlayer;

    private void Awake()
    {
        AfterInitialization += () => {
            tilesHandler.GenerateTiles();
            Tile centerTile = tilesHandler.centerTile.GetComponent<Tile>();
            Player player = Instantiate(localPlayer);
            player.SpawnPlayer(centerTile);
            cityHandler.BuildCityLocally(playerHandler.GetIndexOf(player), tilesHandler.GetIndexOf(centerTile.neighbors[0]));
        };
    }
}
