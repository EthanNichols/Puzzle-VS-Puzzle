using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IdleMoneyExchangeField : PlayField {

    public List<Sprite> tileSprites = new List<Sprite>();
    public Sprite backgroundSprite;

    public float newRowTimer = 4;
    private float newRowReset;

    public Sprite coinMoverSprite;
    private GameObject coinMover;

    public bool gameOver = false;

    public GameObject coinLayer;
    public Dictionary<Vector2, GameObject> coins = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, int> tileValue = new Dictionary<Vector2, int>();

    // Use this for initialization
    void Start () {
        //Temp
        width = 7;
        height = 12;

        //Set the timer reset
        newRowReset = newRowTimer;

        //Load sprites and assets for the puzzle
        SetTileSprites();
        CalcTileSize(3);
        LoadResources();

        //Create the field and the buffer
        CreateField(width, height, backgroundSprite);
        CreateTileBuffer(width, height);

        //Create an empty objects for the coins
        coinLayer = new GameObject();
        coinLayer.name = "Coin Layer";
        coinLayer.transform.SetParent(transform);
        coinLayer.transform.localPosition = Vector2.zero;

        //Set all of the values and objects to 0/null
        foreach (Vector2 pos in tilePositions.Keys)
        {
            tileValue[pos] = 0;
            coins[pos] = null;
        }

        //Create the mover
        CreateMover();

        SpawnRow();
        SpawnRow();
    }
	
	// Update is called once per frame
	void Update () {

        if (gameOver) { return; }

		if (newRowTimer <= 0 && coinMover.GetComponent<CoinMover>().pullDownCoins.Count == 0)
        {
            SpawnRow();
            newRowTimer = newRowReset;
        }

        newRowTimer -= Time.deltaTime;

        Ascend();
    }

    /// <summary>
    /// Creates/load the sprites that will be used for the play area and pieces
    /// </summary>
    public void SetTileSprites()
    {
        int darkness = 1;
        string tile = "Coin";

        tileSprites.Add(SpriteSheetManager.GetSprite(tile + "1", "Tarmac", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile + "5", "Cheese", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile + "10", "Mint", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile + "50", "Spinach", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile + "100", "Water", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile + "500", "Cheese", darkness));

        backgroundSprite = SpriteSheetManager.GetSprite("Blank", "Lilac", darkness + 3);
        coinMoverSprite = SpriteSheetManager.GetSprite("Blank", "Tomato", darkness + 4);
    }

    /// <summary>
    /// Create the coin mover for the play field
    /// </summary>
    private void CreateMover()
    {
        //Get the base object
        GameObject localObj = Resources.Load("Temp Background Tile") as GameObject;

        //Instantiate the mover
        coinMover = Instantiate(localObj);
        coinMover.name = "Coin Mover";

        //Set the size and sprite of the object
        coinMover.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
        coinMover.GetComponent<Image>().sprite = coinMoverSprite;

        //Set the parent and local position of the mover
        coinMover.transform.SetParent(transform);
        coinMover.transform.localPosition = tilePositions[new Vector2(width / 2, 0)] + Vector2.down * (tileSize / 2);

        //Add the mover component, and get the component on the object
        coinMover.AddComponent<CoinMover>();
        CoinMover coinMoverComp = coinMover.GetComponent<CoinMover>();

        //Set information about the mover
        coinMoverComp.gridPos = new Vector3(width / 2, 0);
        coinMoverComp.field = this;
    }

    /// <summary>
    /// Spawn a row of coins at the top of the play field
    /// </summary>
    public void SpawnRow()
    {
        //Reset the timer
        newRowTimer = newRowReset;

        //Move all coins down if the coins can't spawn
        while(tileValue[new Vector2(0, height)] != 0)
        {
            PushRowsDown();
        }

        //Loop through the width of the play field
        for (int x=0; x<width; x++)
        {
            //Get a random coin
            int randCoin = Random.Range(0, 6);

            //Set the value of the coin
            int coinValue = 0;
            switch(randCoin)
            {
                case 0:
                    coinValue = 1;
                    break;
                case 1:
                    coinValue = 5;
                    break;
                case 2:
                    coinValue = 10;
                    break;
                case 3:
                    coinValue = 50;
                    break;
                case 4:
                    coinValue = 100;
                    break;
                case 5:
                    coinValue = 500;
                    break;
            }

            //Get the spawn position of the coin
            Vector2 spawnPos = new Vector2(x, height);

            //Set the value of the coin value dictionary
            tileValue[spawnPos] = coinValue;

            //Spawn the coin in the right position in the play field
            GameObject spawnCoin = Instantiate((GameObject)Resources.Load("Temp Background Tile"));
            spawnCoin.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
            spawnCoin.GetComponent<Image>().sprite = tileSprites[randCoin];
            spawnCoin.transform.SetParent(coinLayer.transform);

            //Add the coin to the dictionary of coin objects
            coins[spawnPos] = spawnCoin;
        }
    }

    /// <summary>
    /// Push all of the coins down a tile
    /// </summary>
    public void PushRowsDown()
    {
        //Loop through the play area and the buffer
        for (int x=0; x<width; x++)
        {
            for (int y=0; y<(height * 2) - 1; y++)
            {
                //Get the position being tested
                Vector2 pos = new Vector2(x, y);

                if (y == 0 && coins[pos]) {
                    coins[pos].transform.position += Vector3.down * tileSize;
                    gameOver = true;
                }

                //Set the new value and coin object in the play area
                tileValue[pos] = tileValue[pos + Vector2.up];
                coins[pos] = coins[pos + Vector2.up];

                tileValue[pos + Vector2.up] = 0;
                coins[pos + Vector2.up] = null;

                //Set the position of the object if it is on the field
                if (coins[pos]) {
                    coins[pos].transform.localPosition = tilePositions[pos];
                }
            }
        }
    }

    public void Ascend()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);

                if (!coins[pos]) { continue; }

                if (coins[pos + Vector2.up]) { continue; }

                tileValue[pos + Vector2.up] = tileValue[pos];
                coins[pos + Vector2.up] = coins[pos];

                coins[pos + Vector2.up].transform.localPosition = tilePositions[pos + Vector2.up];

                coins[pos] = null;
                tileValue[pos] = 0;
            }
        }
    }
}
