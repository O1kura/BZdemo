using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(BoxCollider2D))]
public class BlockV2 : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Vector2Int colliderOffset = new Vector2Int(0,0);
    public Vector2Int[] structure;
    public float scaleInSpawner = 0.5f;
    [SerializeField] private int rotation = 0;
    [HideInInspector]
    public BlockState state = BlockState.normal;

    private bool isMovable = true;
    private float canvasScaleFactor;
    private BlockTileV2 blockTilePrefab;
    private Vector2 basePosition;
    private float baseScale;
    private Vector2Int size;
    private Vector2 center;
    private Vector2Int startBlockPos;

    private Vector2Int RotateVector(Vector2Int point,int rotationIndex)
    {
        Vector2Int ret = new Vector2Int();
        switch (rotationIndex)
        {
            case 1:
                ret.x = point.y;
                ret.y = -point.x;
                break;
            case 2:
                ret.x = -point.x;
                ret.y = -point.y;
                break;
            case 3:
                ret.x = -point.y;
                ret.y = point.x;
                break;
            default:
                ret = point; 
                break;
        }
        return ret;
    }

    private void RotateStructure(int rotationIndex)
    {
        rotation = rotationIndex;
        for (int i = 0; i < structure.Length;i++)
        {            
            structure[i] = RotateVector(structure[i], rotation);
        }
    }
    private Vector3 CoordsToLocalPosition(Vector2Int coords)
    {
        return new Vector3( coords.x - center.x, center.y - coords.y, 0);
    }
    private void CreateBlockTiles()
    {
        foreach (Vector2Int coords in structure)
        {
            Vector3 pos = CoordsToLocalPosition(coords);
            BlockTileV2 b = Instantiate(blockTilePrefab, transform);
            b.transform.localPosition = pos;
        }
    }
    private void SetColliderSize()
    {
        GetComponent<BoxCollider2D>().size = size + colliderOffset;
    }
    private void CalculateBlockInfo()
    {
        Vector2Int min = structure[0];
        Vector2Int max = structure[0];
        for (int i = 1; i < structure.Length; i++)
        {
            if (structure[i].x < min.x)
            {
                min.x = structure[i].x;
            }
            if (structure[i].y < min.y)
            {
                min.y = structure[i].y;
            }
            if (structure[i].x > max.x)
            {
                max.x = structure[i].x;
            }
            if (structure[i].y > max.y)
            {
                max.y = structure[i].y;
            }
        }
        startBlockPos = min;
        center.x = (max.x + min.x)  /2.0f;
        center.y = (max.y + min.y) / 2.0f;
        size = max - min + new Vector2Int(1, 1);
    }
    public void SetMoveable(bool canBeMoved)
    {
        isMovable = canBeMoved;
    }
    public Vector2Int GetCoordsRelativeToStartBlock(int index)
    {
        int x = structure[index].x - startBlockPos.x;
        int y = structure[index].y - startBlockPos.y;

        return new Vector2Int(x, y);
    }

    public Color GetColor()
    {
        return blockTilePrefab.GetComponent<SpriteRenderer>().color;
    }
    public Vector2Int GetSize()
    {
        return size;
    }

    public void ScaleBlock(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetBlock(Rect parentRect, int pos, BlockTileV2 blockTileV2, int rotaionIndex ,float canvasScaleFactor)
    {

        if( rotaionIndex != 0)
        {
            RotateStructure(rotaionIndex);
        }
        CalculateBlockInfo();

        this.blockTilePrefab = blockTileV2;
        this.canvasScaleFactor = canvasScaleFactor;

        CreateBlockTiles();

        SetColliderSize();

        // Get base position in spawner UI
        float x = parentRect.width / BoardV2.BLOCKS_AMOUNT / 2 * (2 * pos + 1) - parentRect.width / 2;
        basePosition = new Vector2(x, 0);
        transform.localPosition = basePosition;
        // Get base scale in spawner UI
        float scale_y = parentRect.height / size.y * scaleInSpawner;
        float scale_x = parentRect.width / BoardV2.BLOCKS_AMOUNT / size.x * scaleInSpawner;
        baseScale = Mathf.Min(scale_x, scale_y);
        ScaleBlock(baseScale);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (isMovable)
        {
            state = BlockState.isDragged;
            transform.localPosition += new Vector3(eventData.delta.x/canvasScaleFactor, eventData.delta.y/canvasScaleFactor, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isMovable)
            state = BlockState.isPlaced;
    }

    public void ResetBlock()
    {
        transform.localPosition = basePosition;
        ScaleBlock(baseScale);
        isMovable = true;
    }

}
