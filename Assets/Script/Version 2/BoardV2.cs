using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardV2 : MonoBehaviour
{
    public const int BOARD_SIZE = 8;
    public const int BLOCKS_AMOUNT = 3;
    public GameObject boardTilePrefab;
    public GameObject spawner;


    [HideInInspector]
    public SpriteRenderer[,] boardTiles = new SpriteRenderer[BOARD_SIZE, BOARD_SIZE];
    [HideInInspector]
    public int[,] boardBlocks = new int[BOARD_SIZE, BOARD_SIZE];
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
                boardBlocks[i, k] = 0;
            }
        }
    }
    private BlockV2 SpawnBlock(int pos, int blockTileIndex, int blockIndex)
    {
        Rect rect = spawner.GetComponent<RectTransform>().rect;
        BlockV2 block = Instantiate(blockPrefabs[blockIndex], spawner.transform).GetComponent<BlockV2>();
        block.SetBlock(rect, pos, blockTileV2s[blockTileIndex], canvasScaleFactor);
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
                blocks[i] = SpawnBlock(i, Random.Range(0, blockTileV2s.Length),Random.Range(0,blockPrefabs.Length));
                if (CanPlaceBlock(blocks[i])) break;
                else { Destroy(blocks[i].gameObject); }
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

    private Vector2Int PositionToBoardTiles(Vector2 pos, BlockV2 block)
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
    private void HighlightBlocks()
    {
        foreach (Vector2Int coords in highlightedBlocksPosList)
        {
            boardTiles[coords.x, coords.y].color = Color.red;
        }
    }

    private void RemoveHighlightedBlocks()
    {
        foreach (Vector2Int coords in highlightedBlocksPosList)
        {
            boardTiles[coords.x, coords.y].color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
        }
        highlightedBlocksPosList.Clear();
    }
    private bool CanPlaceBlockTile(Vector2Int coords)
    {
        return boardBlocks[coords.x, coords.y] == 0;
    }
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
            int sum = 0;
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                sum += boardBlocks[i, j];
            }
            if (sum == BOARD_SIZE)
            {
                list_col.Add(i);
                num++;
            }
        }

        // Check horizontal lines
        for (int i = coords.y; i < coords.y + blockSize.y; i++)
        {
            int sum = 0;
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                sum += boardBlocks[j, i];
            }
            if (sum == BOARD_SIZE)
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
                boardBlocks[i, j] = 0;
                boardTiles[i, j].color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
            }
        }

        foreach (int j in col)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                boardBlocks[j, i] = 0;
                boardTiles[j, i].color = boardTilePrefab.GetComponent<SpriteRenderer>().color;
            }
        }

    }

    private bool CanPlaceBlock(BlockV2 block)
    {
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
                if (canPlace) return true;

            }
        }
        return false;
    }
    private void PlaceBlock(BlockV2 block)
    {
        Color color = block.GetColor();
        foreach (Vector2Int coords in highlightedBlocksPosList)
        {
            boardTiles[coords.x, coords.y].color = color;
            boardBlocks[coords.x, coords.y] = 1;
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
                if (CanPlaceBlock(block))
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
        if (GameController.instance.isGameOver == true) return;
        if (IsGameOver()) GameController.instance.SetGameOver();
        int blocksNum = BLOCKS_AMOUNT;
        for (int i = 0; i < BLOCKS_AMOUNT; i++)
            if (blocks[i] == null)
            {
                blocksNum--;
                continue;
            }
            else
            {
                Vector2 pos = blocks[i].transform.position;
                Vector2Int blockPos = PositionToBoardTiles(pos, blocks[i]);

              
                if (blocks[i].state == BlockState.isDragged)
                {
                    blocks[i].ScaleBlock(boardScale);

                    RemoveHighlightedBlocks();

                    if (CanPlaceBlock(blocks[i], blockPos))
                    {
                        HighlightBlocks();
                    }

                }
                else if (blocks[i].state == BlockState.isPlaced)
                {
                    if (highlightedBlocksPosList.Count > 0)
                    {
                        PlaceBlock(blocks[i]);

                        int score = CheckLines(blocks[i],blockPos);
                        if (score > 0)
                        {
                            GameController.instance.UpdateScore(score, blocks[i]);
                        }

                        highlightedBlocksPosList.Clear();
                        Destroy(blocks[i].gameObject);
                        blocks[i] = null;


                    }
                    else
                    {
                        blocks[i].state = BlockState.normal;
                        blocks[i].ResetBlock();
                        RemoveHighlightedBlocks();
                    }
                }
            }

        if (blocksNum == 0)
        {
            CreateBlocks();
            return;
        }
    }
}
