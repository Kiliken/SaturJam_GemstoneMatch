using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Loading;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;


public class PuzzleController : MonoBehaviour
{
    [SerializeField] public Vector2Int stageSize;
    
    [SerializeField] GameObject piecePrefab;

    private Vector2 _mousePos;
    private Vector2 _mouseWorldPos;
    private bool _mouseDown = false;

    sbyte[] arrayPos = { -1, -1 };
    sbyte[] swapped = { -1, -1, -1, -1, -1 }; // where first byte is status

    PieceArray[,] pieces;
    bool puzzleChecked = false;
    float timerCheck = 1.5f;
    float comboTime = float.MinValue;

    public int comboCount = 0;

    [SerializeField] float pieceCheckTime = .3f;
    [SerializeField] float comboExpire = 5.0f;


    // Start is called before the first frame update
    void Start()
    {
        pieces = new PieceArray[stageSize.x, stageSize.y];
        InitiateStage();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (timerCheck > 0)
        {
            timerCheck -= Time.deltaTime;
        }
        else timerCheck = float.MinValue;

        if (comboTime > 0) 
        {
            //only if fever
            comboTime -= Time.deltaTime;
        }
        else {
            comboTime = float.MinValue;
            comboCount = 0;
        }

        if (!puzzleChecked && timerCheck == float.MinValue)
        {
            UpdatePuzzle();
        }
        Inputs();
        
        
    }

    bool InitiateStage()
    {
        byte pieceSurround = 0b0;
        sbyte k = 10;
        sbyte jicCheck = 0;

        for (sbyte i = 0; i < stageSize.x; i++)
        {
            for (sbyte j = 0; j < stageSize.y; j++)
            {
                pieces[i, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(i, j, stageSize), Quaternion.identity);
                pieces[i, j].obj.transform.SetParent(transform);
                pieces[i, j].script = pieces[i, j].obj.GetComponent<Piece>();
                pieces[i, j].script.IntantiatePiece(i, j, true);
            }
        }


        // check a specific piece

        //return false;
        while (k > 2 || jicCheck < 20) {
            pieceSurround = 0b0;
            k = 0;
            for (sbyte i = 0; i < stageSize.x; i++)
            {
                for (sbyte j = 0; j < stageSize.y; j++)
                {
                    pieceSurround = CheckNearColor(i, j, pieces[i, j].script.color);
                    if ((pieceSurround & 0b0010) != 0)
                    {
                        k = 2;
                        while (k < 5)
                        {
                            if (i + k >= stageSize.x)
                                break;

                            if (pieces[i, j].script.color == pieces[i + k, j].script.color)
                                k++;
                            else break;
                        }
                        IsGoingDownSilent(i, j, k, 0b0010); // but without stuff
                    }
                    if ((pieceSurround & 0b0100) != 0)
                    {
                        k = 2;
                        while (k < 5)
                        {
                            if (j + k >= stageSize.y)
                                break;

                            if (pieces[i, j].script.color == pieces[i, j + k].script.color)
                                k++;
                            else break;
                        }
                        IsGoingDownSilent(i, j, k, 0b0100);
                    }

                }

            }
            jicCheck++;
            if (jicCheck > 99)
            {
                Debug.LogError($"CHECKED A LOT! {jicCheck} times!");
                Debug.Break();
                break;
            }
        }

        //stageSize.y = 7;
        return false;
    }

    void Inputs()
    {
        float y;
        float x;

        if (Input.GetMouseButtonDown(0) && puzzleChecked)
        {
            //test
            Debug.Log("clicked");

            _mousePos = Input.mousePosition;
            _mouseDown = true;
            _mouseWorldPos = Camera.main.ScreenToWorldPoint(_mousePos);
            arrayPos = GlobalUtils.GetIndexByPos(_mouseWorldPos, stageSize);
            
            Debug.Log($"pos= {_mouseWorldPos},i= {arrayPos[0]}, j= {arrayPos[1]}");
        }

        if (_mouseDown)
        {
            y = Input.mousePosition.y - _mousePos.y;
            x = Input.mousePosition.x - _mousePos.x;
            if (Mathf.Abs(y) + 100 < Mathf.Abs(x * 2))
            {
                if (x > 0)
                {
                    SwitchPieces(0b0001, arrayPos); // right
                }
                else
                {
                    SwitchPieces(0b0100, arrayPos); // left
                }
                _mousePos = Input.mousePosition;
            }
            if (Mathf.Abs(x) + 100 < Mathf.Abs(y * 2))
            {
                if (y > 0)
                {
                    SwitchPieces(0b1000, arrayPos); // up
                }
                else
                {
                    SwitchPieces(0b0010, arrayPos); // down
                }
                _mousePos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _mouseDown = false;
            y = Input.mousePosition.y - _mousePos.y;
            x = Input.mousePosition.x - _mousePos.x;
        }

    }

    void SwitchPieces(byte side, sbyte[] piece)
    {
        if (piece[0] == -1 || piece[1] == -1)
            return;

        PieceArray swapper = new PieceArray();

        switch (side)
        {
            case 0b0001:
                Debug.Log("Right");
                if ((sbyte)(piece[0] + 1) >= stageSize.x)
                    return;

                swapped[0] = 1;
                swapped[1] = piece[0];
                swapped[2] = piece[1];
                swapped[3] = (sbyte)(piece[0] + 1);
                swapped[4] = piece[1];

                break;
            case 0b0010:
                Debug.Log("Down");
                if ((sbyte)(piece[1] + 1) >= stageSize.y)
                    return;

                swapped[0] = 1;
                swapped[1] = piece[0];
                swapped[2] = piece[1];
                swapped[3] = piece[0];
                swapped[4] = (sbyte)(piece[1] + 1);
                break;
            case 0b0100:
                Debug.Log("Left");
                if ((sbyte)(piece[0] - 1) < 0)
                    return;

                swapped[0] = 1;
                swapped[1] = piece[0];
                swapped[2] = piece[1];
                swapped[3] = (sbyte)(piece[0] - 1);
                swapped[4] = piece[1];

                break;
            case 0b1000:
                Debug.Log("Up");
                if ((sbyte)(piece[1] - 1) < 0)
                    return;

                swapped[0] = 1;
                swapped[1] = piece[0];
                swapped[2] = piece[1];
                swapped[3] = piece[0];
                swapped[4] = (sbyte)(piece[1] - 1);
                break;
        }

        pieces[swapped[1], swapped[2]].script.MovePiece(swapped[3], swapped[4], GlobalUtils.GetPosByIndex(swapped[3], swapped[4], stageSize));
        pieces[swapped[3], swapped[4]].script.MovePiece(swapped[1], swapped[2], GlobalUtils.GetPosByIndex(swapped[1], swapped[2], stageSize));

        swapper = pieces[swapped[1], swapped[2]];
        pieces[swapped[1], swapped[2]] = pieces[swapped[3], swapped[4]];
        pieces[swapped[3], swapped[4]] = swapper;

        arrayPos[0] = -1;
        arrayPos[1] = -1;
        timerCheck = 1f;
        puzzleChecked = false;
    }

    void UpdatePuzzle()
    {
        byte pieceSurround = 0b0;
        sbyte k = 0;
        sbyte longest = 0;
        sbyte[] longPos = { -1,-1};
        byte longesDir = 0b0;

        // check a specific piece
        
        for (sbyte i = 0; i < stageSize.x ; i++)
        {
            for (sbyte j = 0; j < stageSize.y ; j++)
            {
                pieceSurround = CheckNearColor(i, j, pieces[i, j].script.color);
                if ((pieceSurround & 0b0001) != 0)
                {
                   

                    //return if tris or more
                }
                if ((pieceSurround & 0b0010) != 0)
                {
                    k = 2;
                    while (k < 5)
                    {
                        if (i + k >= stageSize.x)
                            break;

                        if (pieces[i, j].script.color == pieces[i+ k, j].script.color)
                            k++;
                        else break;
                    }
                    //Debug.Log($"Combo size: {k}");
                    //Debug.Log($"Currently at {i},{j}");
                    if (k > longest)
                    {
                        longest = k;
                        longPos[0] = i;
                        longPos[1] = j;
                        longesDir = 0b0010;
                    }
                }
                if ((pieceSurround & 0b0100) != 0)
                {
                    k = 2;
                    while (k < 5)
                    {
                        if (j + k >= stageSize.y)
                            break;

                        if (pieces[i, j].script.color == pieces[i, j + k].script.color)
                            k++;
                        else break;
                    }
                    //Debug.Log($"Combo size: {k}");
                    //Debug.Log($"Currently at {i},{j}");
                    if(k > longest)
                    {
                        longest = k;
                        longPos[0] = i;
                        longPos[1] = j;
                        longesDir = 0b0100;
                    }

                    

                    //return if tris or more
                }
                if ((pieceSurround & 0b1000) != 0)
                {
                    //return if tris or more
                }
                //Debug.Log(Convert.ToString(pieceSurround, 2).PadLeft(4, '0'));
            }

        }

        if (longest > 2)
        {
            IsGoingDown(longPos[0], longPos[1], longest, longesDir);
            //IsGoingDown(longPos[0], longPos[1], longest, longesDir);
            timerCheck = pieceCheckTime; //small pause between blocks
            

            swapped[0] = -1; //reset piece to swap
            return;
        }

        // put the piece back
        if (swapped[0] != -1)
        {
            PieceArray swapper = new PieceArray();

            pieces[swapped[1], swapped[2]].script.MovePiece(swapped[3], swapped[4], GlobalUtils.GetPosByIndex(swapped[3], swapped[4], stageSize));
            pieces[swapped[3], swapped[4]].script.MovePiece(swapped[1], swapped[2], GlobalUtils.GetPosByIndex(swapped[1], swapped[2], stageSize));

            swapper = pieces[swapped[1], swapped[2]];
            pieces[swapped[1], swapped[2]] = pieces[swapped[3], swapped[4]];
            pieces[swapped[3], swapped[4]] = swapper;
        }

        puzzleChecked = true;
    }

    byte CheckNearColor(sbyte x, sbyte y, sbyte color)
    {
        if (color == -1) //already in a tris
            return 0b0;

        byte buffer = 0b0;
        //if (color == pieces[x, Math.Max(y - 1,0)].script.color && y > 0) buffer += 0b0001; // up
        if (color == pieces[Math.Min(x + 1, stageSize.x-1), y ].script.color && x < stageSize.x - 1) buffer += 0b0010; // right
        if (color == pieces[x, Math.Min(y + 1, stageSize.y-1)].script.color && y < stageSize.y - 1) buffer += 0b0100; // down
        //if (color == pieces[Math.Max(x - 1, 0), y ].script.color && x > 0) buffer += 0b1000; // left
        return buffer;
    }

    void IsGoingDown(sbyte x, sbyte y, sbyte size, byte side)
    {
        sbyte k = size;
        byte toDeleteCount = 0;

        comboCount++;
        comboTime = comboExpire;

        Debug.Log($"Size: {size} | Combo: {comboCount}");
        // catch if GameManager exists
        //GlobalUtils.AssertDeclaration(FindFirstObjectByType<Beam>()).BeamRange(size, GlobalUtils.GetPosByIndex(x, y, stageSize));
        //Debug.Log();// beam stuff

        PieceArray[] toDelete = new PieceArray[100];

        if ((side & 0b0010) != 0) //left
        {
            bool skipStep = true;
            sbyte[] r = { 0, 0, 0, 0, 0 };

            Debug.LogWarning($"R size: {r.Length}");

            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();
                toDelete[toDeleteCount] = pieces[x + k, y ];
                toDeleteCount++;
            }


            switch (size)
            {
                case 3:
                    break;
                case 4:
                    r[0] = 25;
                    skipStep = false;
                    break;
                case 5:
                    r[0] = 25;
                    r[1] = (sbyte)(y - 1 < 0 ? 2 : -1);
                    r[2] = (sbyte)(y + 1 >= stageSize.y ? -2 : 1);

                    skipStep = false;
                    break;
            }





            for (int i = 0; i < r.Length; i++)
            {
                if (r[i] == 0)
                    continue;

                if (r[i] == 25)
                    r[i] = 0;

                for (int j = stageSize.x - 1; j >= 0; j--)
                {
                    toDelete[toDeleteCount] = pieces[j, y + r[i]];
                    toDeleteCount++;
                }

                if (r[i] == 0)
                    r[i] = 25;
            }

            
            if (size == 3)
            {
                k = size;
                while (k > 0)
                {
                    k--;

                    for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                    {
                        pieces[x + k, i].script.MovePiece((sbyte)(x + k), (sbyte)(i + 1), GlobalUtils.GetPosByIndex((sbyte)(x + k), (sbyte)(i + 1), stageSize));
                        pieces[x + k, i + 1] = pieces[x + k, i];
                    }
                }

                k = size;
                while (k > 0)
                {
                    k--;
                    pieces[x + k, 0].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(x + k), 0, stageSize), Quaternion.identity);
                    pieces[x + k, 0].obj.transform.SetParent(transform);
                    pieces[x + k, 0].script = pieces[x + k, 0].obj.GetComponent<Piece>();
                    pieces[x + k, 0].script.IntantiatePiece((sbyte)(x + k), 0);
                }
            }
            else
            {
                if(size == 4)
                {
                    for (sbyte j = (sbyte)(stageSize.x - 1); j >= 0; j--)
                    {
                        for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                        {
                            pieces[j, i].script.MovePiece(j, (sbyte)(i + 1), GlobalUtils.GetPosByIndex(j, (sbyte)(i + 1), stageSize));
                            pieces[j, i + 1] = pieces[j, i];
                        }

                        pieces[j, 0].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(j, 0, stageSize), Quaternion.identity);
                        pieces[j, 0].obj.transform.SetParent(transform);
                        pieces[j, 0].script = pieces[j, 0].obj.GetComponent<Piece>();
                        pieces[j, 0].script.IntantiatePiece(j, 0);
                    }
                }

                if (size == 5)
                {
                    for (sbyte j = (sbyte)(stageSize.x - 1); j >= 0; j--)
                    {
                        for (sbyte i = (sbyte)(y - 2); i >= 0; i--)
                        {
                            pieces[j, i].script.MovePiece(j, (sbyte)(i + 3), GlobalUtils.GetPosByIndex(j, (sbyte)(i + 3), stageSize));
                            pieces[j, i + 3] = pieces[j, i];
                        }
                    }

                    for (sbyte j = (sbyte)(stageSize.x - 1); j >= 0; j--)
                    {
                        for (sbyte i = 0; i < 3; i++)
                        {
                            pieces[j, i].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(j, i, stageSize), Quaternion.identity);
                            pieces[j, i].obj.transform.SetParent(transform);
                            pieces[j, i].script = pieces[j, i].obj.GetComponent<Piece>();
                            pieces[j, i].script.IntantiatePiece(j, i);
                        }
                    }
                }

            }

            while (toDeleteCount > 0)
            {
                toDeleteCount--;
                //toDelete[k].script.DeletePiece();
                Destroy(toDelete[toDeleteCount].obj);
            }
        }

        if ((side & 0b0100) != 0) //down
        {
            bool skipStep = true;
            sbyte[] r = { 0, 0, 0, 0, 0 };

            Debug.LogWarning($"R size: {r.Length}");

            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();
                toDelete[toDeleteCount] = pieces[x, y + k];
                toDeleteCount++;
            }

            
                switch (size)
                {
                    case 3:
                        break;
                    case 4: 
                        r[0] = 25;
                        skipStep = false;
                        break;
                    case 5:
                        r[0] = 25;
                        r[1] = (sbyte)(x - 1 < 0 ? 2 : -1);
                        r[2] = (sbyte)(x + 1 >= stageSize.x ? -2 : 1);

                        skipStep = false;
                        break;
                }

            
           

            
                for (int i = 0; i < r.Length; i++)
                {
                    if (r[i] == 0)
                        continue;

                    if (r[i] == 25)
                        r[i] = 0;

                    for (int j = stageSize.y - 1; j >= 0; j--)
                    {
                        toDelete[toDeleteCount] = pieces[x + r[i], j];
                        toDeleteCount++;
                    }

                    if (r[i] == 0)
                        r[i] = 25;
                }

            
                if (skipStep)
                {
                    if (y != 0)
                        for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                        {
                            Debug.Log($"x: {x}; y: {i}");
                            //pieces[x, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, j, stageSize), Quaternion.identity);
                            //pieces[x, j].obj.transform.SetParent(transform);

                            pieces[x, i].script.MovePiece(x, (sbyte)(i + size), GlobalUtils.GetPosByIndex(x, (sbyte)(i + size), stageSize));
                            pieces[x, i + size] = pieces[x, i];
                        }

                    for (sbyte i = 0; i < size; i++)
                    {
                        pieces[x, i].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, i, stageSize), Quaternion.identity);
                        pieces[x, i].obj.transform.SetParent(transform);
                        pieces[x, i].script = pieces[x, i].obj.GetComponent<Piece>();
                        pieces[x, i].script.IntantiatePiece(x, i);
                    }
                }
                else
                {
                    for (int i = 0; i < r.Length; i++)
                    {
                        if (r[i] == 0)
                            continue;

                        if (r[i] == 25)
                            r[i] = 0;


                        for (int j = stageSize.y - 1; j >= 0; j--)
                        {
                            pieces[x + r[i], j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(x + r[i]), (sbyte)j, stageSize), Quaternion.identity);
                            pieces[x + r[i], j].obj.transform.SetParent(transform);
                            pieces[x + r[i], j].script = pieces[x + r[i], j].obj.GetComponent<Piece>();
                            pieces[x + r[i], j].script.IntantiatePiece((sbyte)(x + r[i]), (sbyte)j);
                        }
                    }
                }

            

            while (toDeleteCount > 0)
            {
                toDeleteCount--;
                //toDelete[k].script.DeletePiece();
                Destroy(toDelete[toDeleteCount].obj);
            }
        }
    }

    void IsGoingDownSilent(sbyte x, sbyte y, sbyte size, byte side)
    {
        sbyte k = size;

        PieceArray[] toDelete = new PieceArray[5];

        if ((side & 0b0010) != 0)
        {
            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();
                toDelete[k] = pieces[x + k, y];
            }

           


            k = size;
            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();

                for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                {
                    Debug.Log($"x: {x}; y: {i}");
                    //pieces[x, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, j, stageSize), Quaternion.identity);
                    //pieces[x, j].obj.transform.SetParent(transform);

                    pieces[x + k, i].obj.transform.position = GlobalUtils.GetPosByIndex((sbyte)(x + k), (sbyte)(i + 1), stageSize);
                    pieces[x + k, i + 1] = pieces[x + k, i];
                }
            }

            k = size;
            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();

                pieces[x + k, 0].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(x + k), 0, stageSize), Quaternion.identity);
                pieces[x + k, 0].obj.transform.SetParent(transform);
                pieces[x + k, 0].script = pieces[x + k, 0].obj.GetComponent<Piece>();
                pieces[x + k, 0].script.IntantiatePiece((sbyte)(x + k), 0, true);
            }

            /*for (sbyte i = 0; i < size; i++)
            {
                pieces[i, y].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(i, y, stageSize), Quaternion.identity);
                pieces[i, y].obj.transform.SetParent(transform);
                pieces[i, y].script = pieces[i, y].obj.GetComponent<Piece>();
                pieces[i, y].script.IntantiatePiece(i, y);
            }*/

            k = size;
            while (k > 0)
            {
                k--;
                //toDelete[k].script.DeletePiece();
                Destroy(toDelete[k].obj);
            }
        }

        if ((side & 0b0100) != 0)
        {
            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();
                toDelete[k] = pieces[x, y + k];
            }

            if (y != 0)
                for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                {
                    Debug.Log($"x: {x}; y: {i}");
                    //pieces[x, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, j, stageSize), Quaternion.identity);
                    //pieces[x, j].obj.transform.SetParent(transform);

                    pieces[x, i].obj.transform.position = GlobalUtils.GetPosByIndex(x, (sbyte)(i + size), stageSize);
                    pieces[x, i + size] = pieces[x, i];
                }

            for (sbyte i = 0; i < size; i++)
            {
                pieces[x, i].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, i, stageSize), Quaternion.identity);
                pieces[x, i].obj.transform.SetParent(transform);
                pieces[x, i].script = pieces[x, i].obj.GetComponent<Piece>();
                pieces[x, i].script.IntantiatePiece(x, i, true);
            }

            k = size;
            while (k > 0)
            {
                k--;
                //toDelete[k].script.DeletePiece();
                Destroy(toDelete[k].obj);
            }
        }
    }

    void IsGoingDownBeam(sbyte x, sbyte y, sbyte size, byte side, bool beam = false)
    {
        sbyte k = size;
        byte toDeleteCount = 0;
        
        comboCount++;
        comboTime = comboExpire;

        Debug.Log($"Size: {size} | Combo: {comboCount}");
        // catch if GameManager exists
        //GlobalUtils.AssertDeclaration(FindFirstObjectByType<Beam>()).BeamRange(size, GlobalUtils.GetPosByIndex(x, y, stageSize));
        //Debug.Log();// beam stuff

        PieceArray[] toDelete = new PieceArray[100];

        if ((side & 0b0010) != 0) //left
        {
            bool skipStep = true;
            bool deleteAll = false;
            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();
                toDelete[toDeleteCount] = pieces[x + k, y];
                toDeleteCount++;

                if (!beam)
                {
                    switch (size)
                    {
                        case 3:
                            continue;
                        case 4:
                            if (k != 1)
                                continue;
                            break;
                        case 5:
                            continue;
                    }
                    
                } else

                    switch (size)
                    {
                        case 3:
                            if (k != 1)
                                continue;
                            break;
                        case 4:
                            if (k == 0)
                                continue;
                            break;
                        case 5:
                            deleteAll = true;
                            continue;
                            
                    }


                

                for (int j = stageSize.y - 1; j >= 0; j--)
                {
                    toDelete[toDeleteCount] = pieces[x + k, j];
                    toDeleteCount++;
                }

            }


            // (k == 0 || k == size - 1)
            // k > 0 && k < size - 1

            k = size;
            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();

                if (!beam)
                {
                    switch (size)
                    {
                        case 3:
                            break;
                        case 4:
                            if (k == 1)
                                continue;
                            break;
                        case 5:
                            if (k != 0 || k != size - 1)
                                continue;
                            break;
                    }
                } else

                switch (size)
                {
                    case 3:
                        if (k == 1)
                            continue;
                        break;
                    case 4:
                        if (k != 0)
                            continue;
                        break;
                    case 5:
                            continue;
                    }

                for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                {
                    Debug.Log($"x: {x}; y: {i}");
                    //pieces[x, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, j, stageSize), Quaternion.identity);
                    //pieces[x, j].obj.transform.SetParent(transform);

                    pieces[x + k, i].script.MovePiece((sbyte)(x + k), (sbyte)(i + 1), GlobalUtils.GetPosByIndex((sbyte)(x + k), (sbyte)(i + 1), stageSize));
                    pieces[x + k, i + 1] = pieces[x + k, i];
                }
            }

            k = size;
            while (k > 0)
            {
                k--;
                skipStep = true;
                //pieces[x, y + k].script.DeletePiece();

                if (!beam)
                {
                    switch (size)
                    {
                        case 3:
                            break;
                        case 4:
                            if (k == 1)
                                skipStep = false;
                            break;
                        case 5:
                            if (k != 0 || k != size - 1)
                                skipStep = false;
                            break;
                    }
                } else

                switch (size)
                {
                    case 3:
                        if (k == 1)
                            skipStep = false;
                        break;
                    case 4:
                        if (k != 0)
                            skipStep = false;
                        break;
                    case 5:
                        continue;
                    }

                if (skipStep)
                {
                    pieces[x + k, 0].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(x + k), 0, stageSize), Quaternion.identity);
                    pieces[x + k, 0].obj.transform.SetParent(transform);
                    pieces[x + k, 0].script = pieces[x + k, 0].obj.GetComponent<Piece>();
                    pieces[x + k, 0].script.IntantiatePiece((sbyte)(x + k), 0);
                }
                else
                {
                    for (int j = stageSize.y - 1; j >= 0; j--)
                    {
                        pieces[x + k, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(x + k), (sbyte)j, stageSize), Quaternion.identity);
                        pieces[x + k, j].obj.transform.SetParent(transform);
                        pieces[x + k, j].script = pieces[x + k, j].obj.GetComponent<Piece>();
                        pieces[x + k, j].script.IntantiatePiece((sbyte)(x + k), (sbyte)j);
                    }
                }


                    
            }

            if(deleteAll)
                for (int j = stageSize.y - 1; j >= 0; j--)
                {
                    for (int h = stageSize.x - 1; h >= 0; h--)
                    {
                        toDelete[toDeleteCount] = pieces[h, j];
                        toDeleteCount++;


                        pieces[h, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(h), (sbyte)j, stageSize), Quaternion.identity);
                        pieces[h, j].obj.transform.SetParent(transform);
                        pieces[h, j].script = pieces[h, j].obj.GetComponent<Piece>();
                        pieces[h, j].script.IntantiatePiece((sbyte)(h), (sbyte)j);
                    }
                }

            /*for (sbyte i = 0; i < size; i++)
            {
                pieces[i, y].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(i, y, stageSize), Quaternion.identity);
                pieces[i, y].obj.transform.SetParent(transform);
                pieces[i, y].script = pieces[i, y].obj.GetComponent<Piece>();
                pieces[i, y].script.IntantiatePiece(i, y);
            }*/

            while (toDeleteCount > 0)
            {
                toDeleteCount--;
                //toDelete[k].script.DeletePiece();
                Destroy(toDelete[toDeleteCount].obj);
            }
        }

        if ((side & 0b0100) != 0) //down
        {
            bool skipStep = true;
            bool deleteAll = false;
            sbyte[] r = { 0,0,0,0,0 };

            Debug.LogWarning($"R size: {r.Length}");

            while (k > 0)
            {
                k--;
                //pieces[x, y + k].script.DeletePiece();
                toDelete[toDeleteCount] = pieces[x, y + k];
                toDeleteCount++;
            }

            if (!beam)
            {
                switch (size)
                {
                    case 3:
                        break;
                    case 4:
                        r[0] = 25;
                        skipStep = false;
                        break;
                    case 5:
                        r[0] = 25;
                        r[1] = (sbyte)(x - 1 < 0 ? 2 : -1);
                        r[2] = (sbyte)(x + 1 >= stageSize.x ? -2 : 1);

                        skipStep = false;
                        break;
                }

            }
            else

                switch (size)
                {
                    case 3:
                        r[0] = 25;
                        skipStep = false;
                        break;
                    case 4:
                        r[0] = 25;
                        r[1] = (sbyte)(x - 1 < 0 ? 2 : -1);
                        r[2] = (sbyte)(x + 1 >= stageSize.x ? -2 : 1);

                        skipStep = false;
                        break;
                    case 5:
                        deleteAll = true;
                        break;

                }

            if (!deleteAll)
            for (int i = 0; i < r.Length; i++)
            {
                if (r[i] == 0)
                    continue;

                if (r[i] == 25)
                    r[i] = 0;

                for (int j = stageSize.y - 1; j >= 0; j--)
                {
                    toDelete[toDeleteCount] = pieces[x + r[i], j];
                    toDeleteCount++;
                }

                if (r[i] == 0)
                    r[i] = 25;
            }
            
            if(!deleteAll)
            if (skipStep)
            {
                if (y != 0)
                    for (sbyte i = (sbyte)(y - 1); i >= 0; i--)
                    {
                        Debug.Log($"x: {x}; y: {i}");
                        //pieces[x, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, j, stageSize), Quaternion.identity);
                        //pieces[x, j].obj.transform.SetParent(transform);

                        pieces[x, i].script.MovePiece(x, (sbyte)(i + size), GlobalUtils.GetPosByIndex(x, (sbyte)(i + size), stageSize));
                        pieces[x, i + size] = pieces[x, i];
                    }

                for (sbyte i = 0; i < size; i++)
                {
                    pieces[x, i].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex(x, i, stageSize), Quaternion.identity);
                    pieces[x, i].obj.transform.SetParent(transform);
                    pieces[x, i].script = pieces[x, i].obj.GetComponent<Piece>();
                    pieces[x, i].script.IntantiatePiece(x, i);
                }
            }
            else
            {
                for (int i = 0; i < r.Length; i++)
                {
                    if (r[i] == 0)
                        continue;

                    if (r[i] == 25)
                        r[i] = 0;

                    
                    for (int j = stageSize.y - 1; j >= 0; j--)
                    {
                        pieces[x + r[i], j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(x + r[i]), (sbyte)j, stageSize), Quaternion.identity);
                        pieces[x + r[i], j].obj.transform.SetParent(transform);
                        pieces[x + r[i], j].script = pieces[x + r[i], j].obj.GetComponent<Piece>();
                        pieces[x + r[i], j].script.IntantiatePiece((sbyte)(x + r[i]), (sbyte)j);
                    }
                }
            }

            if (deleteAll)
                for (int j = stageSize.y - 1; j >= 0; j--)
                {
                    for (int h = stageSize.x - 1; h >= 0; h--)
                    {
                        toDelete[toDeleteCount] = pieces[h, j];
                        toDeleteCount++;


                        pieces[h, j].obj = Instantiate(piecePrefab, GlobalUtils.GetPosByIndex((sbyte)(h), (sbyte)j, stageSize), Quaternion.identity);
                        pieces[h, j].obj.transform.SetParent(transform);
                        pieces[h, j].script = pieces[h, j].obj.GetComponent<Piece>();
                        pieces[h, j].script.IntantiatePiece((sbyte)(h), (sbyte)j);
                    }
                }

            while (toDeleteCount > 0)
            {
                toDeleteCount--;
                //toDelete[k].script.DeletePiece();
                Destroy(toDelete[toDeleteCount].obj);
            }
        }
    }


#if false

    private void OnDrawGizmos()
    {


        float[] offsets = (float[])GlobalUtils.stdOffsets.Clone();
        for (sbyte i = 0; i < stageSize.x; i++)
        {
            for (sbyte j = 0; j < stageSize.y; j++)
            {
                Gizmos.DrawCube(new Vector3(offsets[0] - 0.5f, offsets[1] + 0.5f, 0), Vector3.one / 2);
                offsets[1] -= GlobalUtils.stagePadding;
            }
            offsets[0] += GlobalUtils.stagePadding;
            offsets[1] = GlobalUtils.stdOffsets[1];
        }
    }
#endif

}





