﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnakeAIController : MonoBehaviour
{
    private Vector2Int gridPosition;
    private Vector2Int gridDirection;

    public GameObject food;
    public GameObject tailPrefab;

    List<Vector2Int> tail;
    List<Vector2Int> snakesize;
    
    private int snakebodysize = 0;
    bool eat;
    bool dead;

    private float Timer;
    private float MaxTimer;
    
    public bool isUp = false;
    public bool isDown = true;
    public bool isLeft = false;
    public bool isRight = false;

    private void Awake()
    {

        gridPosition = new Vector2Int(0, 0);
        gridDirection = new Vector2Int(0, -1);
        MaxTimer = 0.1f;
        Timer = MaxTimer;

        tail = new List<Vector2Int>();
        snakebodysize = 0;

        snakesize = GetFullSnake();
        food.GetComponent<Food>().SpawnFood(snakesize);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!dead)
        {
            UserInput();
            Movement();
        }
    }

    private void UserInput()
    {
        
        //isUp = animator.GetBool("isUp");
        //isDown = animator.GetBool("isDown");
        //isRight = animator.GetBool("isRight");
        //isLeft = animator.GetBool("isLeft");
        
        if (isUp)
        {
            if (gridDirection.x != 0 && gridDirection.y != -1)
            {
                SpriteRotation(0);
                gridDirection.x = 0;
                gridDirection.y = 1;
            }
        }
        if (isDown)
        {
            if (gridDirection.x != 0 && gridDirection.y != 1)
            {
                SpriteRotation(1);
                gridDirection.x = 0;
                gridDirection.y = -1;
            }
        }
        if (isRight)
        {
            if (gridDirection.x != -1 && gridDirection.y != 0)
            {
                SpriteRotation(2);
                gridDirection.x = 1;
                gridDirection.y = 0;
            }
        }
        if (isLeft)
        {
            if (gridDirection.x != 1 && gridDirection.y != 0)
            {
                SpriteRotation(3);
                gridDirection.x = -1;
                gridDirection.y = 0;
            }
        }
    }

    private void SpriteRotation(int r)
    {
        if(r == 0)
        {
            if(gridDirection.x == 1)
            {
                RotateRight();
            }else if(gridDirection.x == -1)
            {
                RotateLeft();
            }
        }else if (r == 1)
        {
            if (gridDirection.x == 1)
            {
                RotateLeft();
            }
            else if (gridDirection.x == -1)
            {
                RotateRight();
            }
        }else if (r == 2)
        {
            if (gridDirection.y == 1)
            {
                RotateLeft();
            }
            else if (gridDirection.y == -1)
            {
                RotateRight();
            }
        }else
        {
            if (gridDirection.y == 1)
            {
                RotateRight();
            }
            else if (gridDirection.y == -1)
            {
                RotateLeft();
            }
        }
    }

    private void RotateLeft()
    {
        transform.Rotate(Vector3.forward * -90);
    }

    private void RotateRight()
    {
        transform.Rotate(Vector3.forward * 90);
    }

    private void Movement()
    {
        Timer = Timer + Time.deltaTime;

        if (Timer >= MaxTimer)
        {
            Timer = Timer - MaxTimer;
            tail.Insert(0, gridPosition);

            gridPosition = gridPosition + gridDirection;

            if (eat)
            {
                snakebodysize++;
                eat = false;
            }

            if (tail.Count >= snakebodysize + 1)
            {
                tail.RemoveAt(tail.Count - 1);
            }

            for(int i = 0; i < tail.Count; i ++)
            {
                Vector2Int snakePosition = tail[i];
                Vector3 p = new Vector3(snakePosition.x, snakePosition.y);

                GameObject g = (GameObject)Instantiate(tailPrefab, p , Quaternion.identity);
                Object.Destroy(g, MaxTimer);
            }

            transform.position = new Vector3(gridPosition.x, gridPosition.y);
            AstarPath.active.Scan(); //Update scan
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.StartsWith("Apple"))
        {
            eat = true;
            Destroy(collision.gameObject);
            GameController.instance.SnakeAte();

            snakesize = GetFullSnake();
            food.GetComponent<Food>().SpawnFood(snakesize);

        }
        else
        {
            dead = true;
            GameController.instance.GameEnd();
        }        
    }

    public List<Vector2Int> GetFullSnake()
    {
        List<Vector2Int> list = new List<Vector2Int>() { gridPosition };
        list.AddRange(tail);
        return list;
    }
  
}