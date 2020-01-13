﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnakeQL : MonoBehaviour
{
    private Vector2Int gridPosition;
    private Vector2Int gridDirection;

    public GameObject food;
    public GameObject tailPrefab;

    public Transform topBorder;
    public Transform bottomBorder;
    public Transform leftBorder;
    public Transform rightBorder;

    List<Vector2Int> tail;
    List<int> tailRotation; //Used in congruence to tail (0 - up, 1 - down, 2 - right, 3 - left)
    List<Vector2Int> snakesize;

    private int snakebodysize = 0;
    bool eat;
    bool dead;
    int headRotationCode = 1; // (0 - up, 1 - down, 2 - right, 3 - left)
    int headRotationCode_PREV; // (0 - up, 1 - down, 2 - right, 3 - left)

    private float Timer;
    private float MaxTimer;

    public List<GameObject> bodyParts;

    Dictionary<string, float[]> qtable = new Dictionary<string, float[]>();

    float learning_rate = 0.85f;
    float discount = 0.9f;
    float epilson_rate = 1950;

    int reward;

    string current_state;
    int action;
    string next_state;

    bool canRotate = true;

    private void Awake()
    {
        SnakeInit();
        bodyParts = new List<GameObject>();
    }

    private void SnakeInit()
    {
        int x = (int)Random.Range(leftBorder.position.x + 1, rightBorder.position.x - 1);
        int y = (int)Random.Range(bottomBorder.position.y + 1, topBorder.position.y - 1);

        gridPosition = new Vector2Int(x, y);
        gridDirection = new Vector2Int(0, -1);

        MaxTimer = 0f;
        Timer = MaxTimer;

        tail = new List<Vector2Int>();
        tailRotation = new List<int>();
        snakebodysize = 0;

        snakesize = GetFullSnake();

        GameObject[] fd = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject fb in fd)
            GameObject.Destroy(fb);

        food.GetComponent<FoodQL>().SpawnFood(snakesize);

        canRotate = true;

    }

    void Update()
    {
        
        if (!dead)
        {
            if (canRotate)
            {
                current_state = State();
                action = GetAction(current_state);
                AIInput(action);
            }

            Movement();

            next_state = State();

            UpdateTable(action, current_state, next_state);

            if (gridPosition.x > rightBorder.transform.position.x || gridPosition.x < leftBorder.transform.position.x)
            {
                dead = true;
            }

            if (gridPosition.y > topBorder.transform.position.y || gridPosition.y < bottomBorder.transform.position.y)
            {
                dead = true;
            }


        }

        else if (dead == true)
        {
            SnakeInit();
            GameController.instance.Iteration();
            UpdateEpilson();
            dead = false;
        }
    }

    private float GetAngleFromVector(Vector2Int dir)
    {
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
        {
            n += 360;
        }

        return n;
    }

    public string State()
    {
        float f = GameObject.FindGameObjectWithTag("Food").transform.position.x;
        float s = gridPosition.x;
        float dif, dify;

        Debug.Log("Food: " + GameObject.FindGameObjectWithTag("Food").transform.position.x + "," + GameObject.FindGameObjectWithTag("Food").transform.position.y);
        Debug.Log("Snake: " + gridPosition.x + "," + gridPosition.y);

        if (f < 0 && s < 0)
        {
            dif = f - s;
            if (dif < 0)
            {
                dif = Mathf.Abs(dif);
            }
        }
        else if (f < 0 || s < 0)
        {
            if (f < 0)
            {
                dif = Mathf.Abs(f) + s;
            }
            else
            {
                dif = Mathf.Abs(s) + f;
            }
        }
        else
        {
            dif = f - s;
            if (dif < 0)
            {
                dif = Mathf.Abs(dif);
            }
        }

        if (f < s)
        {
            dif = -dif;
        }

        f = GameObject.FindGameObjectWithTag("Food").transform.position.y;
        s = gridPosition.y;

        if (f < 0 && s < 0)
        {
            dify = f - s;
            if (dify < 0)
            {
                dify = Mathf.Abs(dif);
            }
        }
        else if (f < 0 || s < 0)
        {
            if (f < 0)
            {
                dify = Mathf.Abs(f) + s;
            }
            else
            {
                dify = Mathf.Abs(s) + f;
            }
        }
        else
        {
            dify = f - s;
            if (dify < 0)
            {
                dify = Mathf.Abs(dify);
            }
        }

        if (f < s)
        {
            dify = -dify;
        }

        string state = dif + ", " + dify;

        if (dif == 0 && dify == 0)
        {
            //Reward given on collision
        }
        else
        {
            reward = reward - 5;
        }

        Debug.Log("State: " + state);


        return state;
    }

    public void NewRow(string state)
    {
        if (qtable.ContainsKey(state))
        {
            return;
        }
        else
        {
            float[] actions = { 0, 0, 0, 0 };
            qtable.Add(state, actions);
        }

    }

    public int GetAction(string state)
    {
        System.Random rnd = new System.Random();
        float num = rnd.Next(0, 2001);

        if (num < epilson_rate || !qtable.ContainsKey(state))
        {
            int action = rnd.Next(0, 4);
            return action;
        }
        else
        {
            float max = qtable[state][0];
            int action = 0;

            for (int i = 0; i < 4; i++)
            {
                if (max > qtable[state][i])
                {
                    max = qtable[state][i];
                    action = i;
                }
            }

            if (max == 0)
            {
                action = rnd.Next(0, 4);
            }

            return action;
        }
    }

    public void UpdateEpilson()
    {
        if (epilson_rate > 20)
        {
            epilson_rate = epilson_rate - 1;

        }
    }

    public void UpdateTable(int action, string state, string nextstate)
    {
        NewRow(state);
        NewRow(nextstate);
        float newValue = reward + discount * Mathf.Max(qtable[nextstate][0], qtable[nextstate][1], qtable[nextstate][2], qtable[nextstate][3]) - qtable[state][action];
        qtable[state][action] = qtable[state][action] + learning_rate * newValue;
    }

    private void AIInput(int action)
    {
        if (action == 0)                //Action 0 defined as up
        {
            if (gridDirection.x != 0 && gridDirection.y != -1)
            {
                gridDirection.x = 0;
                gridDirection.y = 1;
            }
            canRotate = false;
        }
        if (action == 1)                //Action 1 defined as down
        {
            if (gridDirection.x != 0 && gridDirection.y != 1)
            {
                gridDirection.x = 0;
                gridDirection.y = -1;
            }

            canRotate = false;
        }
        if (action == 2)                //Action 2 defined as right
        {
            if (gridDirection.x != -1 && gridDirection.y != 0)
            {
                gridDirection.x = 1;
                gridDirection.y = 0;
            }

            canRotate = false;
        }
        if (action == 3)                // Action 3 defined as left
        {
            if (gridDirection.x != 1 && gridDirection.y != 0)
            {
                gridDirection.x = -1;
                gridDirection.y = 0;
            }

            canRotate = false;
        }

    }

    private void Movement()
    {
        Timer = Timer + Time.deltaTime;
        if (Timer >= MaxTimer)
        {
            refreshBody();
            Timer = Timer - MaxTimer;
            tail.Insert(0, gridPosition);

            headRotationCode_PREV = headRotationCode;
            headRotationCode = calcCurrHeadRotationCode();

            if (headRotationCode != headRotationCode_PREV)
            {
                tailRotation.Insert(0, calcTurnTypeCode());
            }
            else
            {
                tailRotation.Insert(0, headRotationCode);
            }

            gridPosition = gridPosition + gridDirection;

            if (eat)
            {
                snakebodysize++;
                eat = false;
            }

            if (tail.Count >= snakebodysize + 1)
            {
                tail.RemoveAt(tail.Count - 1);
                tailRotation.RemoveAt(tailRotation.Count - 1);
            }

            for (int i = 0; i < tail.Count; i++)
            {
                Vector2Int snakePosition = tail[i];
                int rotation = tailRotation[i];
                Vector3 p = new Vector3(snakePosition.x, snakePosition.y);

                GameObject g = (GameObject)Instantiate(tailPrefab, p, Quaternion.identity);
                GameObject childStraight = g.transform.Find("Straight").gameObject;
                GameObject childCorner1 = g.transform.Find("Corner1").gameObject;
                GameObject childCorner2 = g.transform.Find("Corner2").gameObject;
                GameObject childCorner3 = g.transform.Find("Corner3").gameObject;
                GameObject childCorner4 = g.transform.Find("Corner4").gameObject;

                switch (rotation)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        g.GetComponent<Transform>().Rotate(Vector3.forward * 90);
                        break;
                    case 3:
                        g.GetComponent<Transform>().Rotate(Vector3.forward * -90);
                        break;
                    case 4:
                        childStraight.SetActive(false);
                        childCorner3.SetActive(true);
                        break;
                    case 5:
                        childStraight.SetActive(false);
                        childCorner4.SetActive(true);
                        break;
                    case 6:
                        childStraight.SetActive(false);
                        childCorner1.SetActive(true);
                        break;
                    case 7:
                        childStraight.SetActive(false);
                        childCorner2.SetActive(true);
                        break;
                    case 8:
                        childStraight.SetActive(false);
                        childCorner2.SetActive(true);
                        break;
                    case 9:
                        childStraight.SetActive(false);
                        childCorner4.SetActive(true);
                        break;
                    case 10:
                        childStraight.SetActive(false);
                        childCorner1.SetActive(true);
                        break;
                    case 11:
                        childStraight.SetActive(false);
                        childCorner3.SetActive(true);
                        break;
                }

                bodyParts.Add(g); // Fills the array
            }


            transform.position = new Vector3(gridPosition.x, gridPosition.y);
            transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(gridDirection) - 270);

            if (!canRotate) canRotate = true;

        }
    }

    public void refreshBody()
    {
        if (snakebodysize > 0)
        {
            for (int i = bodyParts.Count - 1; i >= 0; i--)
            {
                Destroy(bodyParts[i]);
            }
            bodyParts.Clear();
        }
    }

    int calcCurrHeadRotationCode()
    {
        if (gridDirection.x == 0 && gridDirection.y == 1) return 0;
        if (gridDirection.x == 0 && gridDirection.y == -1) return 1;
        if (gridDirection.x == 1 && gridDirection.y == 0) return 2;
        if (gridDirection.x == -1 && gridDirection.y == 0) return 3;
        return 1;
    }

    int calcTurnTypeCode()
    {
        if (headRotationCode_PREV == 0 && headRotationCode == 2) return 4;
        if (headRotationCode_PREV == 0 && headRotationCode == 3) return 5;
        if (headRotationCode_PREV == 1 && headRotationCode == 2) return 6;
        if (headRotationCode_PREV == 1 && headRotationCode == 3) return 7;
        if (headRotationCode_PREV == 2 && headRotationCode == 0) return 8;
        if (headRotationCode_PREV == 2 && headRotationCode == 1) return 9;
        if (headRotationCode_PREV == 3 && headRotationCode == 0) return 10;
        if (headRotationCode_PREV == 3 && headRotationCode == 1) return 11;
        return 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Food")
        {
            eat = true;
            reward = reward + 25;
            Destroy(collision.gameObject);
            food.GetComponent<FoodQL>().SpawnFood(snakesize);
            GameController.instance.Iteration(); // *Russell put this here* I think this could help Chantelle, lemmie know what you think
            UpdateEpilson(); // *Russell put this here* I think this could help Chantelle, lemmie know what you think
            GameController.instance.SnakeAte();
            GameController.instance.SingleGame(true);
            
        }
        else
        {
            dead = true;
            reward = reward - 25;
            GameController.instance.SingleGame(false);
            refreshBody();
        }
    }

    public List<Vector2Int> GetFullSnake()
    {
        List<Vector2Int> list = new List<Vector2Int>() { gridPosition };
        list.AddRange(tail);
        return list;
    }

}
