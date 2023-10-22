using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(BoxCollider2D))]
public class BlockV2 : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Vector2Int colliderOffset = new Vector2Int(0,0);
    public Vector2Int[] structure;
    public float scaleInSpawner = 0.5f;

    [HideInInspector]
    public BlockState state = BlockState.normal;


    private float canvasScaleFactor;
    private BlockTileV2 blockTilePrefab;
    private Vector2 basePosition;
    private float localScale;
    private Vector2Int size;
    private Vector2 center;
    private Vector2Int startBlockPos;
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

    public void SetBlock(Rect parentRect, int pos, BlockTileV2 blockTileV2, float canvasScaleFactor)
    {
        CalculateBlockInfo();

        this.blockTilePrefab = blockTileV2;
        this.canvasScaleFactor = canvasScaleFactor;

        CreateBlockTiles();

        SetColliderSize();
        float x = parentRect.width / BoardV2.BLOCKS_AMOUNT / 2 * (2 * pos + 1) - parentRect.width / 2;
        basePosition = new Vector2(x, 0);
        transform.localPosition = basePosition;

        float scale_y = parentRect.height / size.y * scaleInSpawner;
        float scale_x = parentRect.width / BoardV2.BLOCKS_AMOUNT / size.x * scaleInSpawner;
        localScale = Mathf.Min(scale_x, scale_y);
        ScaleBlock(localScale);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        state = BlockState.isDragged;
        transform.localPosition += new Vector3(eventData.delta.x/canvasScaleFactor, eventData.delta.y/canvasScaleFactor, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        state = BlockState.isPlaced;
    }

    public void ResetBlock()
    {
        transform.localPosition = basePosition;
        ScaleBlock(localScale);
    }
}
