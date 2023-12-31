using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardV2 : MonoBehaviour
{
    public const int BOARD_SIZE = 8;
    public const int BLOCKS_AMOUNT = 3;
    public GameObject boardTilePrefab;
    public GameObject spawner;

    [HideInInspector]
    public bool isGameOver;
    [HideInInspector]
    public SpriteRenderer[,] boardTiles = new SpriteRenderer[BOARD_SIZE, BOARD_SIZE];
    [HideInInspector]
    public bool[,] boardBlocks = new bool[BOARD_SIZE, BOARD_SIZE];
    public BlockV2[] blocks = new BlockV2[BLOCKS_AMOUNT];

    private BlockV2[] blockPrefabs;
    private BlockTileV2[] blockTileV2s;
    private float boardToScreenHeight = 0.8f;
    private Rect screenRect;
    private float boardScale;
    private float canvasScaleFactor;
    private List<Vector2Int> highlightedBlocksPosList = new List<Vector2Int>();

    public void ResetBoard()
    {
        isGameOver = false;
        ResetBoardTiles();
        ResetBoardBlocks();
        CreateBlocks();
        GameController.instance.NewGame();
    }
    private void ResetBoardTiles()
    {
        foreach (SpriteRenderer spriteRenderer in boardTiles)
        {
            spriteRenderer.color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
        }
    }
    private void ResetBoardBlocks()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int k = 0; k < BOARD_SIZE; k++)
            {
                boardBlocks[i, k] = false;
            }
        }
    }
    private BlockV2 SpawnBlock(int pos, int blockTileIndex, int blockIndex, int rotationIndex)
    {
        Rect rect = spawner.GetComponent<RectTransform>().rect;
        BlockV2 block = Instantiate(blockPrefabs[blockIndex], spawner.transform).GetComponent<BlockV2>();
        block.SetBlock(rect, pos, blockTileV2s[blockTileIndex], rotationIndex, canvasScaleFactor);
        return block;
    }
    private void CreateBoard()
    {
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                Transform t = Instantiate(boardTilePrefab, transform).transform;
                t.localPosition = new Vector3Int(x, -y, 0);
                boardTiles[x, y] = t.GetComponent<SpriteRenderer>();
            }
        }
    }
    // Create new blocks
    private void CreateBlocks()
    {
        for (int i = 0; i < BLOCKS_AMOUNT; i++)
        {
            if (blocks[i] == null) continue;
            else
            {
                Destroy(blocks[i].gameObject);
            }
        }

        for (int i = 0; i < BLOCKS_AMOUNT; i++)
        {
            while (true)
            {
                blocks[i] = SpawnBlock(i, Random.Range(0, blockTileV2s.Length)
                    , Random.Range(0, blockPrefabs.Length), Random.Range(0, 4));

                if (CanPlaceBlock(blocks[i]).Count > 0)  break; 
                else Destroy(blocks[i].gameObject);
            }

        }
    }
    private void ScaleBoard()
    {
        float boardWidth = boardTilePrefab.GetComponent<SpriteRenderer>().size.x * BOARD_SIZE;
        boardScale = screenRect.width / boardWidth;
        transform.localScale = new Vector3(boardScale, boardScale, boardScale);
        transform.localPosition = new Vector3(-(boardWidth - 1) * boardScale / 2, screenRect.height * (boardToScreenHeight - 0.5f), 0);
    }

    private bool IsInBoardRange(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < BOARD_SIZE &&
               coords.y >= 0 && coords.y < BOARD_SIZE;
    }
    // Transfer world position to board coordinates
    private Vector2Int PositionToBoardCoords(Vector2 pos, BlockV2 block)
    {
        Vector2 boardStart = boardTiles[0, 0].transform.position;
        Vector2 boardEnd = boardTiles[BOARD_SIZE - 1, BOARD_SIZE - 1].transform.position;
        Vector2 relPos = (pos - boardStart) / (boardEnd - boardStart) * (BOARD_SIZE   - 1);

        Vector2Int blockSize = block.GetSize();
        relPos.x = relPos.x - (blockSize.x - 1) / 2.0f;
        relPos.y = relPos.y - (blockSize.y - 1) / 2.0f;
        Vector2Int coords = new Vector2Int();
        coords.x = Mathf.RoundToInt(relPos.x);
        coords.y = Mathf.RoundToInt(relPos.y);
        return coords;
    }
    private void HighlightBoardTiles()
    {
        foreach (Vector2Int coords in highlightedBlocksPosList)
        {
            boardTiles[coords.x, coords.y].color = Color.red;
        }
    }

    private void RemoveHighlightedBoardTiles()
    {
        foreach (Vector2Int coords in highlightedBlocksPosList)
        {
            boardTiles[coords.x, coords.y].color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
        }
        highlightedBlocksPosList.Clear();
    }
    private bool CanPlaceBlockTile(Vector2Int coords)
    {
        return !boardBlocks[coords.x, coords.y];
    }
    // Return if a block can be placed at a given position
    private bool CanPlaceBlock(BlockV2 block, Vector2Int pos)
    {

        for (int j = 0; j < block.structure.Length; j++)
        {
            Vector2Int coords = pos + block.GetCoordsRelativeToStartBlock(j);

            if (IsInBoardRange(coords) && CanPlaceBlockTile(coords))
            {
                highlightedBlocksPosList.Add(coords);
            }
            else
            {
                highlightedBlocksPosList.Clear();
                return false;
            }
        }
        return true;
    }
    private int CheckLines(BlockV2 block, Vector2Int coords)
    {
        int num = 0;
        Vector2Int blockSize = block.GetSize();
        List<int> list_col = new List<int>();
        List<int> list_row = new List<int>();
        // Check vertical lines
        for (int i = coords.x; i < coords.x + blockSize.x; i++)
        {
            bool ret = true;
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (!boardBlocks[i, j]) 
                { 
                    ret = false;
                    break;
                }
            }
            if (ret)
            {
                list_col.Add(i);
                num++;
            }
        }

        // Check horizontal lines
        for (int i = coords.y; i < coords.y + blockSize.y; i++)
        {
            bool ret = true;
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (!boardBlocks[j, i])
                {
                    ret = false;
                    break;
                }
            }
            if (ret)
            {
                list_row.Add(i);
                num++;
            }
        }

        RemoveLines(list_row, list_col);
        return num;
    }

    private void RemoveLines(List<int> row, List<int> col)
    {

        foreach (int j in row)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                boardBlocks[i, j] = false;
                boardTiles[i, j].color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
            }
        }

        foreach (int j in col)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                boardBlocks[j, i] = false;
                boardTiles[j, i].color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
            }
        }

    }
    // Return a list of coordinates if a block can be place anywhere on the board
    private List<Vector2Int> CanPlaceBlock(BlockV2 block)
    {
        List<Vector2Int> positionList = new List<Vector2Int>();
        for (int i = 0; i < BOARD_SIZE - block.GetSize().x + 1; i++)
        {
            for (int j = 0; j < BOARD_SIZE - block.GetSize().y + 1; j++)
            {
                bool canPlace = true;
                for (int k = 0; k < block.structure.Length; k++)
                {
                    Vector2Int coords = new Vector2Int(i,j) + block.GetCoordsRelativeToStartBlock(k);
                    if (!IsInBoardRange(coords) || !CanPlaceBlockTile(coords))
                    {
                        canPlace = false;
                        break;
                    }
                }
                if (canPlace) positionList.Add(new Vector2Int(i,j));
                if(positionList.Count >= 2) return positionList;

            }
        }
        return positionList;
    }
    private void PlaceBlock(BlockV2 block)
    {
        Color color = block.GetColor();
        foreach (Vector2Int coords in highlightedBlocksPosList)
        {
            boardTiles[coords.x, coords.y].color = color;
            boardBlocks[coords.x, coords.y] = true;
        }
    }
    private bool IsGameOver()
    {
        bool ret = false;
        foreach (BlockV2 block in blocks)
        {
            if (block == null)
            {
                continue;
            }
            else
            {
                ret = true;
                if (CanPlaceBlock(block).Count!=0)
                {
                    return false;
                }
            }
        }
        return ret;
    }
    private void Awake()
    {
        blockTileV2s = Resources.LoadAll<BlockTileV2>("BlockTile");
        blockPrefabs = Resources.LoadAll<BlockV2>("Block");
    }

    // Start is called before the first frame update
    private void Start()
    {
        screenRect = GetComponentInParent<RectTransform>().rect;
        canvasScaleFactor = GetComponentInParent<Canvas>().scaleFactor;
        CreateBoard();
        ScaleBoard();
        CreateBlocks();
    }

    private void Update()
    {
        if (isGameOver == true) return;
        else
        {
            // Check through conditions of all blocks
            int blocksNum = BLOCKS_AMOUNT;
            for (int i = 0; i < BLOCKS_AMOUNT; i++)
                if (blocks[i] == null)
                {
                    blocksNum--;
                    continue;
                }
                else
                {
                    List<Vector2Int> posPlaces = CanPlaceBlock(blocks[i]);
                    int numOfPlaces = posPlaces.Count;

                    // Cannot be placed anywhere
                    if (numOfPlaces == 0)
                    {
                        blocks[i].SetMoveable(false);
                    }
                    else
                    {
                        blocks[i].SetMoveable(true);
                        Vector2 pos = blocks[i].transform.position;
                        Vector2Int blockPos = PositionToBoardCoords(pos, blocks[i]);

                        if (blocks[i].state == BlockState.isDragged)
                        {
                            blocks[i].ScaleBlock(boardScale);

                            RemoveHighlightedBoardTiles();

                            // there only one position that this block can be placed
                            if (numOfPlaces == 1)
                            {
                                CanPlaceBlock(blocks[i], posPlaces.ElementAt(0));
                                HighlightBoardTiles();
                            }
                            else if (CanPlaceBlock(blocks[i], blockPos))
                            {
                                HighlightBoardTiles();
                            }

                        }
                        else if (blocks[i].state == BlockState.isPlaced)
                        {
                            if ((numOfPlaces == 1 && blockPos == posPlaces.ElementAt(0))
                                || (numOfPlaces >= 2 && highlightedBlocksPosList.Count > 0))
                            {
                                PlaceBlock(blocks[i]);

                                int score = CheckLines(blocks[i], blockPos);
                                GameController.instance.UpdateScore(score, blocks[i]);

                                highlightedBlocksPosList.Clear();
                                Destroy(blocks[i].gameObject);
                                blocks[i] = null;
                            }
                            else
                            {
                                blocks[i].state = BlockState.normal;
                                blocks[i].ResetBlock();
                                RemoveHighlightedBoardTiles();
                            }
                        }
                    }
                }
            // Create new blocks if theres no more blocks
            if (blocksNum == 0)
            {
                CreateBlocks();
                return;
            }
        }
        if (IsGameOver()) 
        {
            isGameOver = true;
            GameController.instance.SetGameOver(); 
        }
    }
}
