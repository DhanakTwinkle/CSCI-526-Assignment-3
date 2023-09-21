using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Connector : MonoBehaviour
{
    enum type
    {
        immovable_cell,
        movable_cell,
        player_cell,
        invalid
    }

    public static Connector instance = null;
    
    public Vector3 playerPos;
    List<BoxCollider2D> colliders;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            foreach (Transform child in spriteTransform)
            {
                GameObject.Destroy(child.gameObject);
            }

            colliders = new List<BoxCollider2D>();
            BoxCollider2D newcollider = gameObject.AddComponent<BoxCollider2D>();
            newcollider.offset = new Vector2(0.0f, 0.0f);
            newcollider.size = new Vector2(0.95f, 0.95f);
            colliders.Add(newcollider);

            playerCells = new List<GameObject>();

            GameObject newSprite = new GameObject();
            newSprite.transform.parent = spriteTransform;
            newSprite.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            newSprite.AddComponent<SpriteRenderer>().sprite = playerSprite;
            playerCells.Add(newSprite);
        }
        else
            Destroy(this);

    }

    [Header("Indicator")]
    public GameObject indicator;
    public Color immovableColor;
    public Color movableColor;
    public Color deletableColor;

    [Header("Tilemaps")]
    public Tilemap environment;
    public TileBase immovableCells;
    public TileBase movableCells;

    public List<GameObject> deadCells;
    public List<GameObject> playerCells;

    [Header("Player")]
    public Transform spriteTransform;
    public Sprite playerSprite;
    public Transform deadCellContainer;
    public GameObject deadCellPrefab;

    public void setConnections()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0.0f);
        playerPos = transform.position;

        foreach(GameObject dc in deadCells)
        {
            dc.GetComponent<Rigidbody2D>().isKinematic = true;
            dc.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            dc.transform.position = new Vector3(Mathf.RoundToInt(dc.transform.position.x), Mathf.RoundToInt(dc.transform.position.y), 0.0f);
        }

        indicator.transform.position = new Vector3(Grid.instance.gridEntry[0].location.x, Grid.instance.gridEntry[0].location.y, transform.position.z) + playerPos;
        bool deletable;
        Grid.instance.check(Grid.instance.gridEntry[0].location, out deletable);
        
        Color cellColor;
        if (deletable)
            cellColor = deletableColor;
        else
            cellColor = immovableColor;

        indicator.GetComponent<SpriteRenderer>().color = cellColor;
        indicator.SetActive(true);
    }

    public void resetConnections()
    {
        indicator.SetActive(false);
        GetComponent<Rigidbody2D>().isKinematic = false;
        foreach (GameObject dc in deadCells)
        {
            dc.GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }

    void Update()
    {
        if (StateManager.instance.IsConnecting)
        {
            Vector2 position = new Vector2(indicator.transform.position.x, indicator.transform.position.y);

            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                position += new Vector2(0.0f, 1.0f);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                position -= new Vector2(0.0f, 1.0f);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                position -= new Vector2(1.0f, 0.0f);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                position += new Vector2(1.0f, 0.0f);
            }

            type cellType = type.invalid;
            if (validPosition(position, out cellType))
            {
                if (position - new Vector2(indicator.transform.position.x, indicator.transform.position.y) != Vector2.zero)
                {
                    Color cellColor = Color.black;
                    if (cellType == type.immovable_cell)
                        cellColor = immovableColor;
                    else if (cellType == type.movable_cell)
                        cellColor = movableColor;
                    else if (cellType == type.player_cell)
                        cellColor = deletableColor;

                    indicator.GetComponent<SpriteRenderer>().color = cellColor;
                    indicator.transform.position = new Vector3(position.x, position.y, 0.0f);
                }

                else if (Input.GetKeyDown(KeyCode.E))
                {
                    if (cellType == type.movable_cell)
                    {
                        deleteEnv(position);

                        Vector2 gridPos = position - new Vector2(playerPos.x, playerPos.y);
                        Grid.instance.add(gridPos);
                        BoxCollider2D newcollider = gameObject.AddComponent<BoxCollider2D>();
                        newcollider.offset = gridPos;
                        newcollider.size = new Vector2(0.95f, 0.95f);
                        colliders.Add(newcollider);

                        GameObject newSprite = new GameObject();
                        newSprite.transform.parent = spriteTransform;
                        newSprite.transform.localPosition = new Vector3(gridPos.x, gridPos.y, 0.0f);
                        newSprite.AddComponent<SpriteRenderer>().sprite = playerSprite;
                        playerCells.Add(newSprite);
                    }
                    else if(cellType == type.player_cell)
                    {
                        Vector2 gridPos = position - new Vector2(playerPos.x, playerPos.y);
                        foreach (GameObject sprite in playerCells) {
                            if (sprite.transform.localPosition.x == gridPos.x && sprite.transform.localPosition.y == gridPos.y)
                            {
                                playerCells.Remove(sprite);
                                Destroy(sprite.gameObject);
                                break;
                            }
                        }

                        foreach (BoxCollider2D coll in colliders)
                        {
                            if (coll.offset.x == gridPos.x && coll.offset.y == gridPos.y)
                            {
                                colliders.Remove(coll);
                                Destroy(coll);
                                break;
                            }
                        }

                        Grid.instance.remove(gridPos);

                        GameObject DeadCell = Instantiate(deadCellPrefab, deadCellContainer);
                        DeadCell.transform.position = position;
                        deadCells.Add(DeadCell);
                    }

                    StateManager.instance.switchConnectionState();
                }
            }
        }
    }

    void deleteEnv(Vector2 position)
    {
        {
            // Check if position is part of environment

            Vector3Int cellPos = environment.WorldToCell(new Vector3(position.x, position.y, 0.0f));
            TileBase cell = environment.GetTile(cellPos);
            if (cell != null)
            {
                environment.SetTile(cellPos, null);
                return;
            }
        }

        {
            foreach (GameObject dc in deadCells)
            {
                Vector3 pos = new Vector3(position.x, position.y, 0.0f);
                if (Vector3.Distance(dc.transform.position, pos) < 0.0001f)
                {
                    deadCells.Remove(dc);
                    Destroy(dc.gameObject);
                    return;
                }
            }
        }
    }

    bool validPosition(Vector2 position, out type cellType)
    {
        bool checkable = false;
        {
            Vector2 gridPos = position - new Vector2(playerPos.x, playerPos.y);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == y)
                        continue;
                    else if ((x == -1 && y == 1) || (x == 1 && y == -1))
                        continue;
                    gridPos = position - new Vector2(playerPos.x, playerPos.y);
                    gridPos.x += x;
                    gridPos.y += y;
                    if (Grid.instance.check(gridPos))
                    {
                        checkable = true;
                        break;
                    }
                }
                if (checkable)
                    break;
            }
        }
        {
            // Check if position is part of environment
            if (checkable)
            {
                Vector3Int cellPos = environment.WorldToCell(new Vector3(position.x, position.y, 0.0f));
                TileBase cell = environment.GetTile(cellPos);
                if (cell != null)
                {
                    if (cell == immovableCells)
                        cellType = type.immovable_cell;
                    else if (cell == movableCells)
                        cellType = type.movable_cell;
                    else
                        cellType = type.invalid;

                    return true;
                }
            }
        }

        {
            //Check if position is part of deadCells
            if (checkable)
            {
                foreach (GameObject dc in deadCells)
                {
                    Vector3 pos = new Vector3(position.x, position.y, 0.0f);
                    if (Vector3.Distance(dc.transform.position, pos) < 0.0001f)
                    {
                        cellType = type.movable_cell;
                        return true;
                    }
                }
            }
        }

        {
            // Check if position is part of Player
            bool deletable = false;
            Vector2 gridPos = position - new Vector2(playerPos.x, playerPos.y);
            if(Grid.instance.check(gridPos, out deletable))
            {
                if (deletable)
                    cellType = type.player_cell;
                else
                    cellType = type.immovable_cell;

                return true;
            }
        }

        cellType = type.invalid;
        return false;
    }
}
