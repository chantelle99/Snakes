﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodML : MonoBehaviour
{
    public GameObject food;

    public Transform topBorder;
    public Transform bottomBorder;
    public Transform leftBorder;
    public Transform rightBorder;

    public BoxCollider2D wall;


    public GameObject SpawnFood(List<Vector2> list)
    {
        Vector2Int element = new Vector2Int();
        int x, y;

        Vector2[] wallpts = CalculateWall();
        bool check = false;

        do
        {
            x = (int)Random.Range(leftBorder.position.x+1, rightBorder.position.x-4);
            y = (int)Random.Range(bottomBorder.position.y+1, topBorder.position.y-4);
            element.x = x;
            element.y = y;

            check = CheckWalls(wallpts, x, y);

        } while (list.IndexOf(element) != -1  | check == true);


        return Instantiate(food, new Vector2(x, y), Quaternion.identity);
    }


    public Vector2[] CalculateWall()
    {
        Vector2 size = wall.bounds.extents;
        Vector2 centre = wall.bounds.center;

        float top = centre.y + size.y;
        float btm = centre.y - size.y;

        float left = centre.x - size.x;
        float right = centre.x + size.x;

        Vector2 topLeft = new Vector2(left, top);
        Vector2 topRight = new Vector2(right, top);
        Vector2 btmLeft = new Vector2(left, btm);
        Vector2 btmRight = new Vector2(right, btm);

        Vector2[] wallpts = { topLeft, topRight, btmLeft, btmRight };

        return wallpts;

    }


    public bool CheckWalls(Vector2[] wallpts, int x, int y)
    {
        bool check = false;
       
        if (y <= wallpts[0].y + 2  & y >= wallpts[2].y - 2)
        {
            if (x >= wallpts[0].x - 2 & x <= wallpts[1].x + 2)
            {
                check = true;
            }            
        }else
        {
            check = false;
        }

        return check;
    }
}

