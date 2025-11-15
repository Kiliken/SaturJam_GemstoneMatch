using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public GameObject piece;
    public sbyte color = -1;
    public sbyte[] pos = { -1, -1 };
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public bool IntantiatePiece(sbyte x, sbyte y, bool silet = false)
    {
        color = (sbyte)Random.Range(0, 6);
        GetComponent<SpriteRenderer>().material.SetFloat("_CurrentPalette", (float)color);
        //GetComponent<SpriteRenderer>().color = GlobalUtils.pieceColors[color];
        pos[0] = x;
        pos[1] = y;

        if (!silet)
        {
            Debug.LogError("SILENT");
            StartCoroutine(MoveDown(new Vector2(transform.position.x, 6.8f + transform.position.y), transform.position, 50f));
        }

        return false; 
    }

    public bool MovePiece(sbyte x, sbyte y, Vector2 pos)
    {
        StartCoroutine(MoveDown(transform.position,pos, 50f));
        pos[0] = x;
        pos[1] = y;
        return false;
    }

    public bool DeletePiece()
    {
        color = -1;
        //GetComponent<SpriteRenderer>().color = GlobalUtils.pieceColors[3];
        //GetComponent<SpriteRenderer>().enabled = false;
        transform.localScale *= .5f;
        return false;
    }

    public IEnumerator MoveDown(Vector3 startPos,Vector3 endPos, float speed)
    {
        transform.position = startPos;

        while (Vector3.Distance(transform.position, endPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPos,
                speed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = endPos; // Snap to final position
    }
}
