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

    public GameObject savedPiece;
    public int saved = 0;

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
        //TEMP
        width = 10;
        height = 20;

        SetTileSprites();

        //Calculate the sice of the tiles
        CalcTileSize(3);

        transform.localPosition -= new Vector3((tileSize / nextAreaSize) * (blockCount / 2) + tileSize / 8, (tileSize / nextAreaSize) * (blockCount / 2) + tileSize / 8);

        //Load the resources for the play field
        tetrisPieceObj = Resources.Load("TetrisPiece") as GameObject;
        LoadResources();

        //Create the visible and hidden play field
        CreateField(width, height, backgroundSprite);
        CreateTileBuffer(width, height);
        //CreateBorder();
        CreateNextPieceArea();
        CreateSavedPiece();

        foreach (Vector2 pos in tilePositions.Keys)
        {
            tileOccupancy[pos] = false;
        }

        //Set the spawn locations for tetris pieces
        spawnLocations.Add(new Vector2(width / 2, height));
        if (width % 2 == 0) { spawnLocations.Add(new Vector2(width / 2 - 1, height)); }
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

    /// <summary>
    /// Creates/load the sprites that will be used for the play area and pieces
    /// </summary>
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

    /// <summary>
    /// Create the border around the play area
    /// </summary>
    private void CreateBorder()
    {
        //Get the sprites that will make the border
        Sprite borderEdge = SpriteSheetManager.GetSprite("Border", "Tarmac", 2);
        Sprite borderCorner = SpriteSheetManager.GetSprite("BorderCorner", "Tarmac", 2);

        //Create an empty object to hold the border
        GameObject border = new GameObject();
        border.name = "Border";
        border.transform.SetParent(transform);
        border.transform.localPosition = Vector2.zero;

        //Get the temperary tile that is a base object
        GameObject tile = Resources.Load("Temp Background Tile") as GameObject;

        //Create the left border
        GameObject leftBorder = Instantiate(tile, border.transform);
        leftBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize / 2, height * tileSize);
        leftBorder.GetComponent<Image>().sprite = borderEdge;
        leftBorder.transform.localPosition = new Vector2((-width / 2) * tileSize, 0);

        //Create the right border
        GameObject rightBorder = Instantiate(leftBorder, Vector2.zero, Quaternion.Euler(0, 0, -180), border.transform);
        rightBorder.transform.localPosition = new Vector2((width / 2f) * tileSize, 0);

        //Create the top border
        GameObject topBorder = Instantiate(leftBorder, Vector2.zero, Quaternion.Euler(0, 0, -90), border.transform);
        topBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize / 2, width * tileSize);
        topBorder.transform.localPosition = new Vector2(0, (height / 2f) * tileSize);

        //Create the bottom border
        GameObject bottomBorder = Instantiate(topBorder, Vector2.zero, Quaternion.Euler(0, 0, 90), border.transform);
        bottomBorder.transform.localPosition = new Vector3(0, (-height / 2) * tileSize);

        //Create the four corners of the border
        for (int i=0; i<4; i++)
        {
            //Create the object
            GameObject corner = Instantiate(tile, Vector2.zero, Quaternion.Euler(0, 0, 90 * i), border.transform);
            corner.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize / 2, tileSize / 2f);
            corner.GetComponent<Image>().sprite = borderCorner;

            float localsize = tileSize / 4f;

            //Set the position of the object
            switch (i)
            {
                case 0: corner.transform.localPosition = new Vector2((width / 2f) * tileSize + localsize, (height / 2f) * tileSize + localsize + .1f); break;
                case 1: corner.transform.localPosition = new Vector2((-width / 2f) * tileSize - localsize, (height / 2f) * tileSize + localsize + .1f); break;
                case 2: corner.transform.localPosition = new Vector2((-width / 2f) * tileSize - localsize, (-height / 2f) * tileSize - localsize); break;
                case 3: corner.transform.localPosition = new Vector2((width / 2f) * tileSize + localsize, (-height / 2f) * tileSize - localsize); break;
            }
        }
    }

    /// <summary>
    /// Create the next piece area and populate it with tetris pieces
    /// </summary>
    private void CreateNextPieceArea()
    {
        //Empty object to hold the next piece area
        GameObject nextPiecesObj = new GameObject();

        //Base object for the next piece area
        GameObject background = Resources.Load("Temp Background Tile") as GameObject;
        
        //Set the name, parent, and position of the empty object
        nextPiecesObj.name = "Next Pieces";
        nextPiecesObj.transform.SetParent(transform);
        nextPiecesObj.transform.localPosition = Vector2.zero;

        //Create the correct amount of indicators
        for (int i=0; i<nextPieceCount; i++)
        {
            //Add a random piece to the queue
            int randPiece = Random.Range(0, 7);
            pieceOrder.Enqueue(randPiece);

            //Determine the position of the next piece
            Vector2 pos = new Vector2(width / 2 * tileSize + (tileSize / nextAreaSize) * 2 + tileSize / 4, height / 2 * tileSize - (tileSize / nextAreaSize) * 2 - (tileSize / nextAreaSize) * blockCount * i);

            //Create a new area for the next piece to be viewed
            GameObject newBackground = Instantiate(background, pos, Quaternion.identity, nextPiecesObj.transform);
            newBackground.GetComponent<RectTransform>().sizeDelta = new Vector2((tileSize / nextAreaSize) * blockCount, (tileSize / nextAreaSize) * blockCount);
            newBackground.transform.localPosition = pos;

            //Split the sprite name up for the background color
            string color = backgroundSprite.name.Split('.')[1];
            int darkness = int.Parse(backgroundSprite.name.Split('.')[2].ToString());

            //Create a sprite with the new color info
            newBackground.GetComponent<Image>().sprite = SpriteSheetManager.GetSprite("Color", color, darkness);

            //Create a tetris piece to display in the next area
            GameObject newNextPiece = Instantiate(tetrisPieceObj);
            newNextPiece.transform.SetParent(nextPiecesObj.transform);
            newNextPiece.transform.localPosition = pos;

            //Set information about the next tetris piece
            newNextPiece.GetComponent<TetrisPiece>().fallingTimer = int.MaxValue;
            newNextPiece.GetComponent<TetrisPiece>().tileSize = tileSize / 2f;
            newNextPiece.GetComponent<TetrisPiece>().field = this;
            newNextPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[randPiece];
            newNextPiece.GetComponent<TetrisPiece>().display = true;
            newNextPiece.GetComponent<TetrisPiece>().SetFormation(newNextPiece.transform.localPosition, GetFormation(randPiece));

            //Add the tetris piece to a list of next piece
            nextPieces.Add(newNextPiece);
        }
    }

    /// <summary>
    /// Update the formation of the next tetris pieces
    /// </summary>
    private void UpdateNextPieces()
    {
        //Make sure all the next pieces are filled
        while (pieceOrder.Count < nextPieceCount)
        {
            int randPiece = Random.Range(0, 7);
            pieceOrder.Enqueue(randPiece);
        }

        //Copy the queue of the tetris pieces
        Queue<int> localQueue = new Queue<int>(pieceOrder);

        //Set the new formation of the next tetris pieces.
        foreach(GameObject nextPiece in nextPieces)
        {
            nextPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[localQueue.Peek()];
            nextPiece.GetComponent<TetrisPiece>().SetFormation(nextPiece.transform.localPosition, GetFormation(localQueue.Dequeue()));
        }
    }

    /// <summary>
    /// Create the area for the saved piece to be displayed
    /// </summary>
    private void CreateSavedPiece()
    {
        //Base object to edit for the background
        GameObject background = Resources.Load("Temp Background Tile") as GameObject;

        //Calculate the position of the area
        Vector2 pos = new Vector2((-width / 2) * tileSize + (tileSize / nextAreaSize) * (blockCount / 2), height / 2 * tileSize + (tileSize / nextAreaSize) * 2 + tileSize / 4);

        //Create the background for the saved area
        GameObject newBackground = Instantiate(background, pos, Quaternion.identity, transform);
        newBackground.transform.localPosition = pos;
        newBackground.GetComponent<RectTransform>().sizeDelta = new Vector2((tileSize / nextAreaSize) * blockCount, (tileSize / nextAreaSize) * blockCount);

        //Get information about the color that the sprite should be
        string color = backgroundSprite.name.Split('.')[1];
        int darkness = int.Parse(backgroundSprite.name.Split('.')[2].ToString());

        //Create a sprite with the new color info
        newBackground.GetComponent<Image>().sprite = SpriteSheetManager.GetSprite("Color", color, darkness);

        //Create a tetris piece to display in the next area
        savedPiece = Instantiate(tetrisPieceObj);
        savedPiece.transform.SetParent(newBackground.transform);
        savedPiece.transform.localPosition = Vector2.zero;

        //Set information about the next tetris piece
        savedPiece.GetComponent<TetrisPiece>().fallingTimer = int.MaxValue;
        savedPiece.GetComponent<TetrisPiece>().tileSize = tileSize / 2f;
        savedPiece.GetComponent<TetrisPiece>().field = this;
        savedPiece.GetComponent<TetrisPiece>().display = true;

        //Set all the tiles in the piece to be inactive
        foreach (Transform tiles in savedPiece.transform)
        {
            tiles.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Save tetris piece that was passed
    /// </summary>
    /// <param name="piece"></param>
    public void SavePiece(int piece)
    {
        //The previous piece that was saved
        int oldPiece = savedPiece.GetComponent<TetrisPiece>().pieceID;

        //Set the information about the new saved piece
        savedPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[piece];
        savedPiece.GetComponent<TetrisPiece>().SetFormation(savedPiece.transform.localPosition, GetFormation(piece));
        savedPiece.GetComponent<TetrisPiece>().pieceID = piece;

        //The counter before another piece can be saved
        saved = 2;

        //If there is no saved piece return
        if (oldPiece == -1) { return; }

        //Spawn the previously saved piece
        SpawnPiece(oldPiece);
    }

    /// <summary>
    /// Spawn a random piece on the play field
    /// </summary>
    private void SpawnPiece(int piece = -1)
    {
        if (piece == -1) { piece = pieceOrder.Dequeue(); }

        Queue<Vector2> pieceFormation = GetFormation(piece);
        char shape = ' ';

        //Set the shape of the tetris piece
        switch (piece)
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
        if (piece <= 1) { spawnPos = spawnLocations[spawnLocations.Count - 1]; }

        //Create a new piece
        tetrisPiece = Instantiate(tetrisPieceObj, transform);

        //Set information about the piece
        tetrisPiece.GetComponent<TetrisPiece>().fallingTimer = fallingTimer;
        tetrisPiece.GetComponent<TetrisPiece>().shape = shape;
        tetrisPiece.GetComponent<TetrisPiece>().tileSize = tileSize;
        tetrisPiece.GetComponent<TetrisPiece>().field = this;
        tetrisPiece.GetComponent<TetrisPiece>().tileSprite = tileSprites[piece];

        tetrisPiece.GetComponent<TetrisPiece>().pieceID = piece;

        //Spawn the piece and set the tile location
        tetrisPiece.GetComponent<TetrisPiece>().SetFormation(spawnPos, pieceFormation);

        //Set that the saved piece can be switched and isn't saved
        saved--;
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
