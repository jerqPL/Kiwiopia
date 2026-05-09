using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static Global;

public class TilesHandler : MonoBehaviour
{
    public int gridSize;
    public GameObject hexTile;
    public List<Tile> tiles = new List<Tile>();
    public GameObject centerTile;


    [SerializeField] private TerrainGeneration terrainGeneration;
    [SerializeField] private PlayerHandler playerHandler;


    private Tile getNewTile(Vector2 pos, int i, int x)
    {
        GameObject tileGameObject = Instantiate(hexTile, new Vector3(pos.x, -.5f, pos.y), Quaternion.identity);
        tileGameObject.name = $"Tile_{i}_{x}";
        tileGameObject.transform.parent = transform;
        Tile newTile = tileGameObject.GetComponent<Tile>();
        newTile.position = pos;
        newTile.index = tiles.Count;
        return newTile;
    }


    public void GenerateTiles()
    {
        if (gridSize % 2 == 0)
        {
            gridSize += 1;
        }

        tiles.Clear();

        // Store coordinates for neighbour lookup
        Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();

        Vector2 startingPosition = new Vector2(
            (-Mathf.Sqrt(3) / 2) * (gridSize - 1),
            (-gridSize + 1) * 1.5f
        );

        float deltaX = 0f;

        // TOP HALF + MIDDLE
        for (int i = 0; i < gridSize; i++)
        {
            for (int x = 0; x < gridSize + i; x++)
            {
                Vector2 pos = startingPosition + new Vector2(
                    x * Mathf.Sqrt(3) + deltaX,
                    1.5f * i
                );

                Tile newTile = getNewTile(pos, i, x);

                tiles.Add(newTile);

                tileMap[new Vector2Int(i, x)] = newTile;

                // Ensure exactly 6 neighbour slots
                newTile.neighbours = new Tile[6];

                if (i == gridSize - 1 && x == (gridSize + i - 1) / 2)
                {
                    centerTile = newTile.gameObject;
                }
            }

            deltaX -= Mathf.Sqrt(3) / 2;
        }

        // BOTTOM HALF
        deltaX += Mathf.Sqrt(3);

        for (int i = gridSize - 2; i >= 0; i--)
        {
            for (int x = 0; x < gridSize + i; x++)
            {
                Vector2 pos = startingPosition + new Vector2(
                    x * Mathf.Sqrt(3) + deltaX,
                    1.5f * (gridSize + (gridSize - 2 - i))
                );

                int row = 2 * gridSize - i - 2;

                Tile newTile = getNewTile(pos, row, x);

                tiles.Add(newTile);

                tileMap[new Vector2Int(row, x)] = newTile;

                // Ensure exactly 6 neighbour slots
                newTile.neighbours = new Tile[6];
            }

            deltaX += Mathf.Sqrt(3) / 2;
        }

        // CONNECT NEIGHBOURS
        foreach (var kvp in tileMap)
        {
            Vector2Int coord = kvp.Key;
            Tile tile = kvp.Value;

            int row = coord.x;
            int col = coord.y;

            // Same order for every tile:
            // 0 = Left
            // 1 = Right
            // 2 = Upper Left
            // 3 = Upper Right
            // 4 = Lower Left
            // 5 = Lower Right

            Vector2Int[] offsets;

            bool topHalf = row < gridSize;

            if (topHalf)
            {
                offsets = new Vector2Int[]
                {
                new Vector2Int(0, -1), // Left
                new Vector2Int(0, 1),  // Right
                new Vector2Int(-1, -1),// Upper Left
                new Vector2Int(-1, 0), // Upper Right
                new Vector2Int(1, 0),  // Lower Left
                new Vector2Int(1, 1)   // Lower Right
                };
            }
            else
            {
                offsets = new Vector2Int[]
                {
                new Vector2Int(0, -1), // Left
                new Vector2Int(0, 1),  // Right
                new Vector2Int(-1, 0), // Upper Left
                new Vector2Int(-1, 1), // Upper Right
                new Vector2Int(1, -1), // Lower Left
                new Vector2Int(1, 0)   // Lower Right
                };
            }

            for (int n = 0; n < 6; n++)
            {
                Vector2Int neighbourCoord = coord + offsets[n];

                if (tileMap.TryGetValue(neighbourCoord, out Tile neighbour))
                {
                    tile.neighbours[n] = neighbour;
                }
                else
                {
                    tile.neighbours[n] = null;
                }
            }
        }

        // APPLY TERRAIN
        foreach (Tile tile in tiles)
        {
            tile.ApplyTerrain(
                terrainGeneration.GetTerrainAtPos(
                    tile.transform.position.x,
                    tile.transform.position.z
                )
            );
        }
    }


    /*void AddNeighbourAtIndex(int index, Tile tile)
    {
        if (index >= 0 && index < tiles.Count && !tile.neighbours.Contains(tiles[index]))
        {
            tile.neighbours.Add(tiles[index]);
            tiles[index].neighbours.Add(tile);
        }
        else
        {
            Debug.LogWarning($"Index {index} is out of bounds for tiles list.");
        }
    }*/

    /*public List<Tile> shortestPath(Tile source, Tile end)
    {
        Unit unit = source.unit;
        if (source == null || end == null || unit == null || unit.unitMovement.isMoving.Value)
            return new List<Tile>();

        // BFS kolejka
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(source);

        // S³ownik przechowuj¹cy poprzednika ka¿dego odwiedzonego wêz³a
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        cameFrom[source] = null;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            if (current == end)
            {
                break; // znaleziono cel
            }

            List<Tile> neighbors = new List<Tile>(current.neighbors);
            Global.Shuffle(neighbors);
            foreach (Tile neighbor in neighbors)
            {
                if (!cameFrom.ContainsKey(neighbor) && (!neighbor.hasMountains || unit.unitType.canClimb) && (neighbor == end || neighbor.unit == null || neighbor.unit.owner != unit.owner || (neighbor.unit.owner == unit.owner && neighbor.unit.unitMovement.isMoving.Value)))
                {
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Odtworzenie œcie¿ki od end do source
        List<Tile> path = new List<Tile>();
        Tile temp = end;
        while (temp != null)
        {
            path.Add(temp);
            cameFrom.TryGetValue(temp, out temp);
        }

        path.Reverse(); // od source do end
        if (path.Count > 0 && path[0] != source)
        {
            // brak po³¹czenia miêdzy source a end
            return new List<Tile>();
        }

        return path;
    }*/

    public Tile RandomTile()
    {
        return tiles[Random.Range(0, tiles.Count)];
    }

    public void SetVisibility(List<Tile> visibleTiles)
    {
        foreach (Tile tile in tiles)
        {
            if (visibleTiles.Contains(tile)){
                tile.SetVisibility(true);
            }
            else
            {
                tile.SetVisibility(false);
            }
        }
    }

    public Tile GetTileAt(int index)
    {
        return tiles[index];
    }

    public int GetIndexOf(Tile tile)
    {
        return tiles.IndexOf(tile);
    }

    public bool CanGetThrough(Unit unit, Tile tile)
    {
        if (!unit.owner.seenTiles.Contains(Global.tilesHandler.GetIndexOf(tile)) || ((!tile.hasMountains || unit.unitType.canClimb) && (tile.unit == null || tile.unit.owner != unit.owner || (tile.unit.owner == unit.owner && tile.unit.unitMovement.isMoving.Value))))
        {
            return true;
        }
        return true;
        return false;
    }

    public List<Tile> shortestPathSeeingVisible(Tile source, Tile end)
    {
        Unit unit = source.unit;
        if (source == null || end == null || unit == null || unit.unitMovement.isMoving.Value)
        {
            Debug.Log("Brak Ÿród³a, celu lub jednostki w shortestPathSeeingVisible");
            return new List<Tile>();
        }

        // BFS kolejka
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(source);

        // S³ownik przechowuj¹cy poprzednika ka¿dego odwiedzonego wêz³a
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        cameFrom[source] = null;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            if (current == end)
            {
                break; // znaleziono cel
            }

            List<Tile> neighbors = new List<Tile>(current.neighbours);
            Global.Shuffle(neighbors);
            foreach (Tile neighbour in neighbors)
            {
                if (neighbour == null) continue;
                if (!cameFrom.ContainsKey(neighbour))
                {
                    if (CanGetThrough(unit, neighbour))
                    {
                        cameFrom[neighbour] = current;
                        queue.Enqueue(neighbour);
                    }
                }
            }
        }

        // Odtworzenie œcie¿ki od end do source
        List<Tile> path = new List<Tile>();
        Tile temp = end;
        while (temp != null)
        {
            path.Add(temp);
            cameFrom.TryGetValue(temp, out temp);
        }

        path.Reverse(); // od source do end
        if (path.Count > 0 && path[0] != source)
        {
            Debug.Log("Brak po³¹czenia miêdzy Ÿród³em a celem w shortestPathSeeingVisible");
            return new List<Tile>();
        }

        return path;
    }

    public List<Tile> IndirectTileNeighbours(Tile tile, int range)
    {
        List<Tile> visibleTiles = new List<Tile> { tile };
        for (int i = 0; i < range; i++)
        {
            int visTiles = visibleTiles.Count;
            for (int j = 0; j < visTiles; j++)
            {
                Tile tilee = visibleTiles[j];
                foreach (Tile neighbour in tilee.neighbours)
                {
                    if (neighbour == null) continue;
                    if (!visibleTiles.Contains(neighbour))
                        visibleTiles.Add(neighbour);
                }
            }
        }
        return visibleTiles;
    }

    public int Distance(Tile tile1, Tile tile2)
    {
        if (tile1 == tile2)
        {
            Debug.Log("Distance calculation starts and ends, tiles are the same, distance is 0");
            return 0;
        }
        Debug.Log($"Distance calculation starts from {tile1.name} to {tile2.name}");
        List<Tile> visibleTiles = new List<Tile> { tile1 };
        for (int o = 1; o < gridSize; o++)
        {
            for (int i = 0; i < o; i++)
            {
                int visTiles = visibleTiles.Count;
                for (int j = 0; j < visTiles; j++)
                {
                    Tile tilee = visibleTiles[j];
                    foreach (Tile neighbour in tilee.neighbours)
                    {
                        if (neighbour == null) continue;
                        if (!visibleTiles.Contains(neighbour))
                        {
                            if (neighbour == tile2)
                            {
                                Debug.Log($"Distance calculation ends, distance is {o}");
                                return o;
                            }
                            visibleTiles.Add(neighbour);
                        }
                    }
                }
            }
        }
        Debug.Log("Distance calculation ends, no path found");
        return -1;
    }
}
