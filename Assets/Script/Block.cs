using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Color defaultColor;
    public Vector2Int size;
    public Vector2Int[] structure;
    public float scaleInSpawner = 0.9f;
    [HideInInspector]
    public BlockState state = BlockState.normal;

    private Vector2 basePosition;
    private float localScale;
    

    public void GetCenter()
    {

    }
    public void GetSize()
    {
        
    }
    public Color GetColor()
    {
        return transform.GetChild(0).GetComponent<SpriteRenderer>().color;
    }

    /*public void ChangeColor()
    {
        SpriteRenderer[] arr = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer spriteRenderer in arr)
        {
            spriteRenderer.material.color = defaultColor;
        }
    }*/
    public void ScaleBlock(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetBlock(Rect parentRect, int pos)
    {
        float x = parentRect.width / Board.BLOCKS_AMOUNT / 2 * (2*pos+1) - parentRect.width/2;
        basePosition = new Vector2(x,0);
        transform.localPosition = basePosition;

        float scale_y = (parentRect.height / GetComponent<BoxCollider2D>().size.y)*scaleInSpawner;
        float scale_x =  (parentRect.width / Board.BLOCKS_AMOUNT / GetComponent<BoxCollider2D>().size.x)*scaleInSpawner;
        localScale = Mathf.Min(scale_x, scale_y);
        ScaleBlock(localScale);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        state = BlockState.isDragged;
        transform.localPosition += new Vector3(eventData.delta.x, eventData.delta.y, 0);
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
    private void Start()
    {
        //ChangeColor();
    }
}
