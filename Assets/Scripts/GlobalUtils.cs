using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class GlobalUtils
{
    public static float stagePadding = 1.1f;
    public static UnityEngine.Color[] pieceColors = { 
        new Color32(35,181,116, 255),
        new Color32(41,170,225, 255),
        new Color32(248,147,31, 255),
        new Color32(163, 41, 181, 255),   
        new Color32(255, 66, 93, 255),    
        new Color32(255, 241, 89, 255),
    };
    public static readonly float[] stdOffsets = { -3.3f, 6.5f };
    public static float pieceRadius = 1;

    //add a fuinction to get puzzle size on vect2dInt

    public static Vector2 GetPosByIndex(sbyte x, sbyte y, Vector2Int size)
    {
        float[] offsets = (float[])stdOffsets.Clone();
        for (sbyte i = 0; i < size.x; i++)
        {
            for (sbyte j = 0; j < size.y; j++)
            {
                if (i == x && j == y) return new Vector2(offsets[0], offsets[1]);
                offsets[1] -= stagePadding;
            }
            offsets[0] += stagePadding;
            offsets[1] = stdOffsets[1];
        }
        return Vector2.zero; // Function failed
    }


    public static sbyte[] GetIndexByPos(Vector2 pos, Vector2Int size)
    {
        float[] offsets = (float[])stdOffsets.Clone();
        for (sbyte i = 0; i < size.x; i++)
        {
            for (sbyte j = 0; j < size.y; j++)
            {
                if (IsClicked(pos, offsets[0], offsets[1],pieceRadius)) return new sbyte[] { i, j }; ;
                offsets[1] -= stagePadding;
            }
            offsets[0] += stagePadding;
            offsets[1] = stdOffsets[1];
        }
        return new sbyte[]{ -1,-1}; // Function failed
    }

    public static Vector2 GetMatchCenter(sbyte x, sbyte y, Vector2Int size, sbyte count, byte direction)
    {
        switch (direction)
        {
            case 0b0010: // right
                float[] offsets = (float[])stdOffsets.Clone();
                for (sbyte i = 0; i < size.x; i++)
                {
                    for (sbyte j = 0; j < size.y; j++)
                    {
                        if (i == x && j == y)
                        {
                            offsets[0] += (count / 2) * stagePadding;
                            return new Vector2(offsets[0], offsets[1]);
                        }
                            
                            
                         // start pos + those
                        offsets[1] -= stagePadding;
                    }
                    offsets[0] += stagePadding;
                    offsets[1] = stdOffsets[1];
                }


                return Vector2.zero;
                break;

            case 0b0100: // down

                return Vector2.zero;
                break;
            default:
                Debug.LogError($"NO SUCH LINE!");
                return Vector2.zero;
                break;

        }

        // just in case
        return Vector2.zero;
    }

    public static bool IsClicked(Vector2 point, float posX, float posY, float size)
    {
        return point.x >= posX - size / 2 &&
               point.x <= posX + size / 2 &&
               point.y >= posY - size / 2 &&
               point.y <= posY + size / 2;
    }

    public static T AssertDeclaration<T>(T toDeclare) where T : MonoBehaviour
    {
        if (toDeclare == null)
        {
            Debug.LogError($"NullReferenceException: Missing {typeof(T).Name}");
            Debug.Break();
            return null;
        }

        return toDeclare;
    }

}

public struct PieceArray
{
    public GameObject obj { get; set; }
    public Piece script { get; set; }
}