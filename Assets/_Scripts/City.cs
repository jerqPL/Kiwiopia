using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class City : NetworkBehaviour
{
    public List<Tile> cityTiles = new List<Tile>();
    [SerializeField] private Slider recruitProgressBar;
    [SerializeField] private LineRenderer lineRenderer;
    public NetworkVariable<int> tileIndex = new NetworkVariable<int>();
    public Tile tile => Global.tilesHandler.GetTileAt(tileIndex.Value);
    public NetworkVariable<int> ownerIndex = new NetworkVariable<int>();
    public Player owner => Global.playerHandler.GetPlayerAt(ownerIndex.Value);

    public int orderNumber = 0;
    public NetworkVariable<int> size = new NetworkVariable<int>(0,
                                           NetworkVariableReadPermission.Owner,
                                           NetworkVariableWritePermission.Server);

    public int money = 0;
    public int wood = 0;
    public int stone = 0;

    public bool isRecruiting = false;

    private int recruitedUnitType;
    private float recruitTimeLeft;

    private LineRenderer cityBorder;

    public override void OnNetworkSpawn()
    {
        transform.position = tile.transform.position;
        Debug.Log($"tile index: {tileIndex.Value}");
        owner.citys.Add(this);
        tile.city = this;
        tile.underCity = this;
        Global.cityHandler.cities.Add(this);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void ChangeSizeServerRpc(int newSize)
    {
        newSize -= 2;
        if (newSize >= 0)
        {
            int moneyToTake = Global.newCityResourceCost[orderNumber, 0] * Global.cityUpgradeCostMultiplayer[newSize, 0];
            int woodToTake = Global.newCityResourceCost[orderNumber, 1] * Global.cityUpgradeCostMultiplayer[newSize, 1];
            int stoneToTake = Global.newCityResourceCost[orderNumber, 2] * Global.cityUpgradeCostMultiplayer[newSize, 2];

            if (!owner.TakeResources(moneyToTake, woodToTake, stoneToTake))
            {
                return;
            }
        }
        
        ChangeSizeClientRpc(newSize);
        size.Value = newSize + 2;
    }

    [ClientRpc]
    public void ChangeSizeClientRpc(int newSize)
    {
        foreach (Tile cityTile in cityTiles)
        {
            cityTile.underCity = null;
            cityTile.owner = null;
        }
        cityTiles.Clear();
        cityTiles.Add(tile);
        tile.underCity = this;
        tile.city = this;
        tile.GetDefaultOwner();
        foreach (Tile neighbour in tile.neighbors)
        {
            if (neighbour.underCity == null && (neighbour.owner == tile.owner || neighbour.owner == null))
            {
                neighbour.underCity = this;
                cityTiles.Add(neighbour);
                neighbour.owner = tile.owner;
            }
        }
        for (int i = 0; i < (newSize + 2) - 1; i++)
        {
            int cityTilesCount = cityTiles.Count;
            for (int j = 0; j < cityTilesCount; j++)
            {
                Tile cityTile = cityTiles[j];
                foreach (Tile neighbour in cityTile.neighbors)
                {
                    if (!cityTiles.Contains(neighbour) && neighbour.underCity == null && (neighbour.owner == tile.owner || neighbour.owner == null))
                    {
                        neighbour.underCity = this;
                        cityTiles.Add(neighbour);
                        neighbour.owner = tile.owner;
                    }
                    
                }
            }
        }
        if (owner == Global.playerHandler.GetLocalPlayer())
        {
            Global.playerHandler.GetLocalPlayer().UpdateVisibleTiles();
        }
        CreateBorder();
    }



    private void CreateBorder()
    {
        // store all edges, remove duplicates
        Dictionary<Edge, int> edgeCount = new Dictionary<Edge, int>();

        foreach (var tile in cityTiles)
        {
            Vector3[] corners = tile.GetCorners(); // returns 6 positions
            
            
            for (int i = 0; i < 6; i++)
            {
                Vector3 a = Global.AddToYVector3(corners[i], Global.lineHegithAboveTiles);
                Vector3 b = Global.AddToYVector3(corners[(i + 1) % 6], Global.lineHegithAboveTiles);

                

                Edge e = new Edge(a, b);

                if (edgeCount.ContainsKey(e))
                    edgeCount[e]++;
                else
                    edgeCount[e] = 1;
            }
            
        }
        

        // collect only outer edges (appear once)
        List<Edge> borderEdges = new List<Edge>();
        foreach (var p in edgeCount)
        {
            if (p.Value == 1)
            {
                borderEdges.Add(p.Key);
                Debug.DrawRay(p.Key.a, Vector3.up * 100f, Color.purple, 4f, false);
            }
            else
            {
                Debug.DrawRay(p.Key.a, Vector3.up * 100f, Color.yellow, 4f, false);
            }
        }
        
        // now sort edges into a continuous loop
        List<Vector3> borderPoints = BuildContinuousLoop(borderEdges);

        // assign to line renderer
        Destroy(cityBorder);
        (cityBorder = Instantiate(lineRenderer, Vector3.zero, Quaternion.Euler(90, 0, 0))).transform.SetParent(transform);
        
        cityBorder.positionCount = borderPoints.Count;
        cityBorder.SetPositions(borderPoints.ToArray());
    }

    List<Vector3> BuildContinuousLoop(List<Edge> edges)
    {
        List<Vector3> result = new List<Vector3>();

        Edge start = edges[0];
        edges.RemoveAt(0);

        result.Add(start.a);
        result.Add(start.b);

        Vector3 current = start.b;
        int safe = 1000;
        while (edges.Count > 0 && safe > 0)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                Edge e = edges[i];

                if (Vector3.Distance(current, e.a) < 0.1f)
                {
                    result.Add(e.b);
                    current = e.b;
                    edges.RemoveAt(i);
                    break;
                }
                if (Vector3.Distance(current, e.b) < 0.1f)
                {
                    result.Add(e.a);
                    current = e.a;
                    edges.RemoveAt(i);
                    break;
                }
            }
            safe--;
        }
        if (safe < 10)
        {
            Debug.LogError("Safe trigerred");
        }

        return result;
    }

    struct Edge : System.IEquatable<Edge>
    {
        public Vector3 a;
        public Vector3 b;
        private const float EPSILON = 0.1f;

        public Edge(Vector3 a, Vector3 b)
        {
            // sort to make undirected comparison
            if (a.x < b.x || (a.x == b.x && a.z < b.z))
            {
                this.a = a;
                this.b = b;
            }
            else
            {
                this.a = b;
                this.b = a;
            }
        }

        public bool Equals(Edge other)
        {
            return (Vector3.Distance(a, other.a) < EPSILON &&
                    Vector3.Distance(b, other.b) < EPSILON);
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            // Round positions to reduce floating point errors
            int hashA = Mathf.RoundToInt(a.x * 1000f) ^ Mathf.RoundToInt(a.y * 1000f) ^ Mathf.RoundToInt(a.z * 1000f);
            int hashB = Mathf.RoundToInt(b.x * 1000f) ^ Mathf.RoundToInt(b.y * 1000f) ^ Mathf.RoundToInt(b.z * 1000f);
            return hashA ^ hashB;
        }
    }

    private void Update()
    {
        if (isRecruiting) RecruitingProgress();
        if (!IsServer) return;
        SendResourcesToPlayer();
    }

    void SendResourcesToPlayer()
    {
        if (money == 0 && wood == 0 && stone == 0)
        {
            return;
        }
        owner.RecieveResources(money, wood, stone);
        money = 0;
        wood = 0;
        stone = 0;
    }

    public void RecieveResources(int rMoney, int rWood, int rStone)
    {
        money += rMoney;
        wood += rWood;
        stone += rStone;
    }

    private void RecruitingProgress()
    {
        recruitTimeLeft -= Time.deltaTime;
        recruitProgressBar.value = (Global.unitTypes[recruitedUnitType].recruitTime - recruitTimeLeft) / Global.unitTypes[recruitedUnitType].recruitTime;
        if (recruitTimeLeft < 0)
        {
            isRecruiting = false;
            recruitProgressBar.gameObject.SetActive(false);
            if (IsServer)
            {
                Global.unitsHandler.RecruitUnit(ownerIndex.Value, tileIndex.Value, recruitedUnitType);
            }
        }
    }

    public void StartRecruiting(int unitType)
    {
        isRecruiting = true;
        recruitedUnitType = unitType;
        recruitTimeLeft = Global.unitTypes[unitType].recruitTime;
        recruitProgressBar.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        UpdateRecruitProgressBarRotation();
    }

    private void UpdateRecruitProgressBarRotation()
    {
        float camY = Camera.main.transform.eulerAngles.y;

        // rotation around local X only
        Quaternion localRot = Quaternion.Euler(camY + 90f, 0f, 0f);

        recruitProgressBar.transform.localRotation = localRot;
    }
}
