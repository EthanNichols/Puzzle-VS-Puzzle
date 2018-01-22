using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisField : PlayField
{

    public List<Sprite> tileSprites = new List<Sprite>();
    public Sprite backgroundSprite;

    [HideInInspector]
    public int nextPieceCount = 5;
    public Queue<int> pieceOrder = new Queue<int>();
    public List<GameObject> nextPieces = new List<GameObject>();
    public GameObject tetrisPiece;

    public int width = 10;
    public int height = 20;

    float nextAreaSize = 1.75f;
    float blockCount = 4f;

    public bool gameOver = false;

    private List<int> linesToClear = new List<int>();
    private float clearLineTimer;

    private float fallingTimer = 2;
    private GameObject tetrisPieceObj;
    private List<Vector2> spawnLocations = new List<Vector2>();

    private Dictionary<Vector2, bool> tileOccupancy = new Dictionary<Vector2, bool>();


    // Use this for initialization
    void Start()
    {
        SetTileSprites();

        //Calculate the sice of the tiles
        CalcTileSize();

        transform.localPosition -= new Vector3((tileSize / nextAreaSize) * (blockCount / 2), 0);

        //Load the resources for the play field
        tetrisPieceObj = Resources.Load("TetrisPiece") as GameObject;
        LoadResources();

        //Create the visible and hidden play field
        CreateField(width, height, backgroundSprite);
        CreateTileBuffer(width, height);
        CreateNextPieceArea();
        CreateBorder();

        foreach (Vector2 pos in tilePositions.Keys)
        {
            tileOccupancy[pos] = false;
        }

        //Set the spawn locations for tetris pieces
        spawnLocations.Add(new Vector2(width / 2, height));
        if (width % 2 == 0) { spawnLocations.Add(new Vector2(width / 2 - 1, height)); }

        //TEMPERARY
        SpawnPiece();
    }

    // Update is called once per frame
    void Update()
    {

        //If the player got a game over don't update
        if (gameOver) { return; }

        //If there is no tetris piece to move, spawn one
        if (tetrisPiece == null &&
            linesToClear.Count == 0)
        {
            SpawnPiece();
            UpdateNextPieces();
        }

        if (clearLineTimer > 0) { clearLineTimer -= Time.deltaTime; }
        if (clearLineTimer <= 0 && linesToClear.Count > 0) { ClearLines(); }
    }

    public void SetTileSprites()
    {
        int darkness = 3;
        string tile = "Blank";

        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Tomato", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Cheese", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Lilac", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Spinach", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Water", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Cherry", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Chocolate", darkness));

        backgroundSprite = SpriteSheetManager.GetSprite(tile, "Tarmac", darkness + 2);
    }

    private void CreateBorder()
    {
        Sprite borderEdge = SpriteSheetManager.GetSprite("Border", "Tarmac", 2);
        Sprite borderCorner = SpriteSheetManager.GetSprite("BorderCorner", "Tarmac", 2);

        GameObject border = new GameObject();
        border.name = "Border";
        border.transform.SetParent(transform);
        border.transform.localPosition = Vector2.zero;

        GameObject tile = Resources.Load("Temp Background Tile") as GameObject;

        //Create the left border
        GameObject leftBorder = Instantiate(tile);
        leftBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize / 2, height * tileSize);
        leftBorder.GetComponent<Image>().sprite = borderEdge;
        leftBorder.transform.SetParent(border.transform);
        leftBorder.transform.localPosition = new Vector2((-width / 2) * tileSize, 0);

        //Create the right border
        GameObject rightBorder = Instantiate(leftBorder);
        rightBorder.transform.SetParent(border.transform);
        rightBorder.transform.localPosition = new Vector2((width / 2f) * tileSize, 0);

        //Create the top border
        GameObject topBorder = Instantiate(leftBorder);
        topBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, width * tileSize);
        topBorder.transform.SetParent(border.transform);
        topBorder.transform.localRotation = Quaternion.Euler(0, 0, -90);
        topBorder.transform.localPosition = new Vector2(0, (height / 2) * tileSize);

        //Create the bottom border
        GameObject bottomBorder = Instantiate(topBorder);
        bottomBorder.transform.SetParent(border.transform);
        bottomBorder.transform.localRotation = Quaternion.Euler(0, 0, 90);
        bottomBorder.transform.localPosition = new Vector2(0, (-height / 2) * tileSize);

        /*
        GameObject corner1 = Instantiate(tile);
        corner1.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
        corner1.GetComponent<Image>().sprite = borderCorner;
        corner1.transform.SetParent(border.transform);
        corner1.transform.localRotation = Quaternion.Euler(0, 0, 180);
        corner1.transform.localPosition = new Vector2(((-width - 1) / 2f) * tileSize, ((-height - 1) / 2f) * tileSize);

        GameObject corner2 = Instantiate(corner1);
        corner2.transform.SetParent(border.transform);
        corner2.transform.localRotation = Quaternion.Euler(0, 0, 90);
        corner2.transform.localPosition = new Vector2(((-width - 1) / 2f) * tileSize, ((height + 1) / 2f) * tileSize);
        */
    }

    private void CreateNextPieceArea()
    {
        GameObject nextPiecesObj = new GameObject();

        GameObject nextPieceBackground = Resources.Load("Temp Background Tile") as GameObject;

        nextPiecesObj.name = "Next Pieces";
        nextPiecesObj.transform.SetParent(transform);
        nextPiecesObj.transform.localPosition = Vector2.zero;

        for (int i=0; i<nextPieceCount; i++)
        {
            int randPiece = Random.Range(0, 7);
            pieceOrder.Enqueue(randPiece);

            Vector2 pos = new Vector2(width / 2 * tileSize + (tileSize / nextAreaSize) * 2, height / 2 * tileSize - (tileSize / nextAreaSize) * 2 - (tileSize / nextAreaSize) * blockCount * i);

            GameObject newBackground = Instantiate(nextPieceBackground);
            newBackground.GetComponent<RectTransform>().sizeDelta = new Vector2((tileSize / nextAreaSize) * blockCount, (tileSize / nextAreaSize) * blockCount);
            newBackground.transform.SetParent(nextPiecesObj.transform);
            newBackground.transform.localPosition = pos;

            string color = backgroundSprite.name.Split('.')[1];
            int darkness = int.Parse(backgroundSprite.name.Split('.')[2].ToString());

            newBackground.GetComponent<Image>().sprite = SpriteSheetManager.GetSprite("Color", color, darkness);

            GameObject newNextPiece = Instantiate(tetrisPieceObj);
            newNextPiece.transform.SetParent(nextPiecesObj.transform);
            newNextPiece.transform.localPosition = pos;

            newNextPiece.GetComponent<TetrisPiece>().fallingTimer = int.MaxValue;
            newNextPiece.GetComponent<TetrisPiece>().tileSize = tileSize / 2f;
            newNextPiece.GetComponent<TetrisPiece>().field = this;
            newNextPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[randPiece];
            newNextPiece.GetComponent<TetrisPiece>().display = true;

            newNextPiece.GetComponent<TetrisPiece>().SetFormation(newNextPiece.transform.localPosition, GetFormation(randPiece));

            nextPieces.Add(newNextPiece);
        }
    }

    private void UpdateNextPieces()
    {
        while (pieceOrder.Count < nextPieceCount)
        {
            int randPiece = Random.Range(0, 7);
            pieceOrder.Enqueue(randPiece);
        }

        Queue<int> localQueue = new Queue<int>(pieceOrder);

        foreach(GameObject nextPiece in nextPieces)
        {
            nextPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[localQueue.Peek()];
            nextPiece.GetComponent<TetrisPiece>().SetFormation(nextPiece.transform.localPosition, GetFormation(localQueue.Dequeue()));
        }
    }

    /// <summary>
    /// Calculate the size that the tiles will be in the playfield
    /// </summary>
    private void CalcTileSize()
    {
        //Padding between the edge of the possible area and the play area
        int padding = 3;

        //Calculate the largest tile size possible, then set the tilesize
        if ((Screen.width / players) / width > Screen.height / height)
        {
            tileSize = Screen.height / (height + padding);
        }
        else
        {
            tileSize = (Screen.width / players) / (width + padding);
        }
    }

    /// <summary>
    /// Spawn a random piece on the play field
    /// </summary>
    private void SpawnPiece()
    {
        int pieceNum = pieceOrder.Dequeue();

        Queue<Vector2> pieceFormation = GetFormation(pieceNum);
        char shape = ' ';

        //Set the shape of the tetris piece
        switch (pieceNum)
        {
            case 0: shape = 'i'; break;
            case 1: shape = 'o'; break;
            case 2: shape = 't'; break;
            case 3: shape = 'j'; break;
            case 4: shape = 'l'; break;
            case 5: shape = 's'; break;
            case 6: shape = 'z'; break;
        }

        //Set the spawn location for the piece
        Vector2 spawnPos = spawnLocations[0];
        if (pieceNum <= 1) { spawnPos = spawnLocations[spawnLocations.Count - 1]; }

        //Create a new piece
        tetrisPiece = Instantiate(tetrisPieceObj, transform);

        //Set information about the piece
        tetrisPiece.GetComponent<TetrisPiece>().fallingTimer = fallingTimer;
        tetrisPiece.GetComponent<TetrisPiece>().shape = shape;
        tetrisPiece.GetComponent<TetrisPiece>().tileSize = tileSize;
        tetrisPiece.GetComponent<TetrisPiece>().field = this;
        tetrisPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[pieceNum];

        //Spawn the piece and set the tile location
        tetrisPiece.GetComponent<TetrisPiece>().SetFormation(spawnPos, pieceFormation);
    }

    /// <summary>
    /// Get the shape of the tetris piece
    /// </summary>
    /// <param name="pieceNum">The tetris piece number to form</param>
    /// <returns></returns>
    private Queue<Vector2> GetFormation(int pieceNum)
    {
        Queue<Vector2> pieceFormation = new Queue<Vector2>();


        switch (pieceNum)
        {
            //iPiece
            case 0:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.right * 2);
                break;

            //oPiece
            case 1:
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(new Vector2(1, 1));
                pieceFormation.Enqueue(Vector2.right);
                break;

            //tPiece
            case 2:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.down);
                pieceFormation.Enqueue(Vector2.right);
                break;

            //jPiece
            case 3:
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(new Vector2(1, -1));
                break;

            //lPiece
            case 4:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(new Vector2(-1, -1));
                break;

            //sPiece
            case 5:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(new Vector2(1, 1));
                break;

            //zPiece
            case 6:
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(new Vector2(-1, 1));
                break;
        }

        return pieceFormation;
    }

    /// <summary>
    /// Test if a location is occupied or not
    /// </summary>
    /// <param name="pos">The position to test</param>
    /// <returns>Whether the location is occupied</returns>
    public bool TestOccupancy(Vector2 pos)
    {
        //If the position isn't in the play area return that it is occupied
        if (pos.x < 0) { return true; }
        else if (pos.x >= width) { return true; }
        else if (pos.y < 0) { return true; }

        return tileOccupancy[pos];
    }

    /// <summary>
    /// Set the image or gameobject that is in the position
    /// </summary>
    /// <param name="pos">The position of the tile</param>
    /// <param name="tile">The gameobject that will be placed there</param>
    public void PlaceTile(Vector2 pos, GameObject tile)
    {
        //Set the gameobject if there isn't one set already
        if (tileObjects[pos] == null)
        {
            GameObject newTile = Instantiate(tile);
            newTile.transform.SetParent(transform);
            newTile.SetActive(false);

            tileObjects[pos] = newTile;
        }

        //Set the image if there is a gameobject there, and set it to be occupied
        tileObjects[pos].GetComponent<Image>().sprite = tile.GetComponent<Image>().sprite;
        tileOccupancy[pos] = true;
    }

    /// <summary>
    /// Test for lines that can be cleared and clear them
    /// </summary>
    /// <param name="lines">The lines that can be cleared</param>
    public void GetClearLines(List<int> lines)
    {
        //Go through the lines that can be cleared
        for (int i = 0; i < lines.Count; i++)
        {
            bool addToClear = true;

            for (int x = 0; x < width; x++)
            {
                //If there is a tile that isn't occupied don't clear the line
                if (!tileOccupancy[new Vector2(x, lines[i])])
                {
                    addToClear = false;
                    break;
                }
            }

            //Add lines that have all of their tiles occupied
            if (addToClear) { linesToClear.Add(lines[i]); }
        }

        //Return if there are no lines to clear
        if (linesToClear.Count == 0) { return; }

        //Sort the lines from smallest to biggest
        linesToClear.Sort();

        //Loop through all the lines to clear
        foreach (int line in linesToClear)
        {
            //Set all of the tiles in the line to be unoccupied and have the blank background
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, line);
                tileOccupancy[pos] = false;

                string fullSpriteName = tileObjects[pos].GetComponent<Image>().sprite.name;

                string tile = fullSpriteName.Split('.')[0];
                string color = fullSpriteName.Split('.')[1];
                int darkness = int.Parse(fullSpriteName.Split('.')[2]);

                tileObjects[pos].GetComponent<Image>().sprite = SpriteSheetManager.GetSprite(tile, color, darkness - 2);
            }
        }

        clearLineTimer = .1f;
    }

    /// <summary>
    /// Remove the tiles that have been cleared and move the rest of the tiles down
    /// </summary>
    private void ClearLines()
    {
        //The height to move the tiles down
        int fallHeight = 1;

        for (int y = linesToClear[0] + 1; y < height * 2; y++)
        {

            //Increase the height tiles will drop if the y is equal to a cleared line
            if (linesToClear.Count > fallHeight)
            {
                if (y == linesToClear[fallHeight]) { fallHeight++; continue; }
            }

            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);

                //Go to the next tile if there is no gameobject
                if (tileObjects[pos + Vector2.down * fallHeight] == null) { continue; }

                //If tile object doesn't exist continue
                if (tileObjects[pos] == null)
                {
                    tileObjects[pos + Vector2.down * fallHeight].GetComponent<Image>().sprite = backgroundSprite;
                    tileOccupancy[pos + Vector2.down * fallHeight] = false;
                    continue;
                }

                //Set the tile on top of the gap to the bottom of the gap and set the position to be occupied
                tileObjects[pos + Vector2.down * fallHeight].GetComponent<Image>().sprite = tileObjects[pos].GetComponent<Image>().sprite;
                tileOccupancy[pos + Vector2.down * fallHeight] = tileOccupancy[pos];

                //Unoccupy the tile on top
                tileOccupancy[pos] = false;

                //Destroy the gameobject if the position isn't in the visible play area
                if (pos.y >= 20) { Destroy(tileObjects[pos]); }
            }
        }

        //Clear the list of lines that need to be removed
        linesToClear.Clear();
    }
}
