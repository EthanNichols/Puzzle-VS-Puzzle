using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMover : MonoBehaviour {

    [HideInInspector]
    public Vector2 gridPos;

    public float moveTimer = .1f;
    private float moveTimeReset;

    public IdleMoneyExchangeField field;

    private GameObject coinBase;
    private int holdingCoinValue;
    private int holdingCount;

    public List<Vector2> pullDownCoins = new List<Vector2>();

	// Use this for initialization
	void Start () {
        moveTimeReset = moveTimer;
    }
	
	// Update is called once per frame
	void Update () {

        if (field.gameOver) { return; }

        if (pullDownCoins.Count > 0)
        {
            PullDown();
            return;
        }

        MoveInput();
	}

    /// <summary>
    /// Get player input and run the correct functions
    /// </summary>
    private void MoveInput()
    {
        if (Input.GetKeyDown(KeyCode.C)) { PullCoins(); }
        if (Input.GetKeyDown(KeyCode.V)) { PushCoins(); }

        //Move the player, or reset the moveTimer
        if (Input.GetKey(KeyCode.A)) { Move(Vector2.left); }
        else if (Input.GetKey(KeyCode.D)) { Move(Vector2.right); }
        else { moveTimer = 0; }

        //Create a new row of coins at the top
        if (Input.GetKeyDown(KeyCode.S)) { field.SpawnRow(); }
    }

    /// <summary>
    /// Move the player in the direction passed in
    /// </summary>
    /// <param name="dir">The direction the player will move</param>
    private void Move(Vector2 dir)
    {
        //Start the timer
        moveTimer -= Time.deltaTime;

        //Move the player when the timer is finished
        if (moveTimer <= 0)
        {
            gridPos += dir;

            //Keep the player within the game bounds
            if (gridPos.x < 0) { gridPos.x = 0; }
            if (gridPos.x >= field.width) { gridPos.x = field.width - 1; }

            //Get the new position for the player
            Vector2 newPos = field.tilePositions[gridPos];
            newPos.y = transform.localPosition.y;

            //Set the player's position, and reset the timer
            transform.localPosition = newPos;
            moveTimer = moveTimeReset;
        }
    }

    private void PullCoins()
    {
        for (int y=0; y<field.height; y++)
        {
            Vector2 pos = new Vector2(gridPos.x, y);

            if (field.tileValue[pos] == 0) { continue; }

            if (holdingCoinValue == 0 || field.tileValue[pos] == holdingCoinValue) {
                coinBase = Instantiate(field.coins[pos]);
                coinBase.SetActive(false);
                holdingCoinValue = field.tileValue[pos];
                holdingCount++;

                pullDownCoins.Add(pos);
                field.tileValue[pos] = 0;

            } else
            {
                return;
            }
        }
    }

    private void PullDown()
    {
        for (int i=0; i<pullDownCoins.Count; i++)
        {
            Vector2 curPos = pullDownCoins[i];

            field.coins[curPos].transform.localPosition += Vector3.down * field.tileSize;

            if (field.coins[curPos].transform.localPosition.y <= (-field.height / 2) * field.tileSize)
            {
                Destroy(field.coins[curPos]);

                field.coins[curPos] = null;
                field.tileValue[curPos] = 0;

                pullDownCoins.RemoveAt(i);
            }
        }
    }

    private void PushCoins()
    {
        if (holdingCount > 0)
        {
            if (field.coins[new Vector2(gridPos.x, 0)]) { return; }

            holdingCount--;

            GameObject newCoin = Instantiate(coinBase);
            newCoin.transform.SetParent(field.coinLayer.transform);
            newCoin.transform.localPosition = field.tilePositions[new Vector2(gridPos.x, 0)];
            newCoin.SetActive(true);

            field.coins[new Vector2(gridPos.x, 0)] = newCoin;
            
            if (holdingCount <= 0)
            {
                Destroy(coinBase);

                holdingCoinValue = 0;
                coinBase = null;
            }
        }
    }
}
