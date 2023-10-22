using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;
    public const int BLOCKS_AMOUNT = 3;
    public GameObject boardTilePrefab;
    public GameObject spawner;
    public GameObject[] blockPrefabs;


    [HideInInspector]
    public SpriteRenderer[,] boardTiles = new SpriteRenderer[BOARD_SIZE, BOARD_SIZE];
    [HideInInspector]
    public int[,] boardBlocks = new int[BOARD_SIZE, BOARD_SIZE];
    public Block[] blocks = new Block[BLOCKS_AMOUNT];


    private float boardToScreenHeight = 0.8f;
    private Rect screenRect;
    private float boardScale;
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
    private Block SpawnBlock(int pos,int blockIndex)
    {
        Rect rect = spawner.GetComponent<RectTransform>().rect;
        Block block = Instantiate(blockPrefabs[blockIndex],spawner.transform).GetComponent<Block>();
        block.SetBlock(rect,pos);
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
            while (true) { 
                blocks[i] = SpawnBlock(i, Random.Range(0, blockPrefabs.Length));
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
        transform.localPosition = new Vector3( - (boardWidth -1) * boardScale / 2, screenRect.height * (boardToScreenHeight-0.5f),0);
    }

    private bool IsInBoardRange(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < BOARD_SIZE &&
               coords.y >= 0 && coords.y < BOARD_SIZE;
    }

    private Vector2Int PositionToBoardTiles(Vector2 pos)
    {

        Vector2 boardStart = boardTiles[0,0].transform.position;
        Vector2 boardEnd = boardTiles[BOARD_SIZE - 1, BOARD_SIZE - 1].transform.position;
        Vector2 relPos = (pos - boardStart) / (boardEnd - boardStart) * (BOARD_SIZE-1);

        int x = Mathf.RoundToInt(relPos.x);
        int y = Mathf.RoundToInt(relPos.y);

        return new Vector2Int(x, y);
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
    private bool CanPlaceBlock(Block block, Vector2Int pos)
    {

        for (int j = 0; j < block.structure.Length; j++)
        {
            Vector2Int coords = block.structure[j] + pos;
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
    private int CheckLines(Block block)
    {
        int num = 0;
        Vector2Int blockSize = block.size;
        Vector2Int coords = highlightedBlocksPosList.ElementAt(0) - block.structure[0];
        List<int> list_col = new List<int>();
        List<int> list_row = new List<int>();
        // Check vertical lines
        for (int i = coords.x; i < coords.x+blockSize.x ; i++)
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

    private bool CanPlaceBlock(Block block)
    {
        for(int i = 0; i < BOARD_SIZE - block.size.x + 1; i++)
        {
            for(int j = 0; j < BOARD_SIZE - block.size.y + 1; j++)
            {
                bool canPlace = true;
                for (int k = 0; k < block.structure.Length; k++)
                {
                    Vector2Int coords = block.structure[k] + new Vector2Int(i, j);
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
    private void PlaceBlock(Block block)
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
        foreach(Block block in blocks)
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
        screenRect = GetComponentInParent<RectTransform>().rect;
    }

    // Start is called before the first frame update
    private void Start()
    {
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

                if (blocks[i].state == BlockState.isDragged)
                {
                    blocks[i].ScaleBlock(boardScale);
                    Vector2 pos = blocks[i].transform.GetChild(0).position;
                    Vector2Int firstBlockPos = PositionToBoardTiles(pos);
                    Vector2Int boardPos = firstBlockPos - blocks[i].structure[0];

                    RemoveHighlightedBlocks();

                    if (CanPlaceBlock(blocks[i], boardPos))
                    {
                        HighlightBlocks();
                    }

                }
                else if (blocks[i].state == BlockState.isPlaced)
                {
                    if (highlightedBlocksPosList.Count > 0)
                    {
                        PlaceBlock(blocks[i]);

                        int score = CheckLines(blocks[i]) ;
                        if(score > 0)
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
