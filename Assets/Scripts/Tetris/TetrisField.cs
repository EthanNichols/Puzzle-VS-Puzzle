using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisField : PlayField
{

    [HideInInspector]
    public GameObject tetrisPiece;

    public int width = 10;
    public int height = 20;

    public bool gameOver = false;

    private GameObject tetrisPieceObj;
    private List<Vector2> spawnLocations = new List<Vector2>();

    private Dictionary<Vector2, bool> tileOccupancy = new Dictionary<Vector2, bool>();


    // Use this for initialization
    void Start()
    {

        //Calculate the sice of the tiles
        CalcTileSize();

        //Load the resources for the play field
        tetrisPieceObj = Resources.Load("TetrisPiece") as GameObject;
        LoadResources();

        //Create the visible and hidden play field
        CreateField(width, height);
        CreateTileBuffer(width, height);

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
        if (tetrisPiece == null)
        {
            SpawnPiece();
        }
    }

    /// <summary>
    /// Calculate the size that the tiles will be in the playfield
    /// </summary>
    private void CalcTileSize()
    {
        //Padding between the edge of the possible area and the play area
        int padding = 2;

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
        //Random piece, and the position for the tile in that piece
        int randPiece = Random.Range(0, 7);
        Queue<Vector2> pieceFormation = new Queue<Vector2>();
        char shape = ' ';

        //Set the positions of each tile in the piece
        switch (randPiece)
        {
            //iPiece
            case 0:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.right * 2);

                shape = 'i';
                break;

            //oPiece
            case 1:
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(new Vector2(1, 1));
                pieceFormation.Enqueue(Vector2.right);

                shape = 'o';
                break;

            //tPiece
            case 2:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(Vector2.right);

                shape = 't';
                break;

            //jPiece
            case 3:
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(new Vector2(-1, 1));

                shape = 'j';
                break;

            //lPiece
            case 4:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(new Vector2(1, 1));

                shape = 'l';
                break;

            //sPiece
            case 5:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(new Vector2(1, 1));

                shape = 's';
                break;

            //zPiece
            case 6:
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(new Vector2(-1, 1));

                shape = 'z';
                break;
        }

        //Set the spawn location for the piece
        Vector2 spawnPos = spawnLocations[0];
        if (randPiece <= 1) { spawnPos = spawnLocations[spawnLocations.Count - 1]; }

        //Create a new piece
        tetrisPiece = Instantiate(tetrisPieceObj, transform);

        //Set information about the piece
        tetrisPiece.GetComponent<TetrisPiece>().fallingTimer = 2f;
        tetrisPiece.GetComponent<TetrisPiece>().shape = shape;
        tetrisPiece.GetComponent<TetrisPiece>().tileSize = tileSize;
        tetrisPiece.GetComponent<TetrisPiece>().field = this;

        //Spawn the piece and set the tile location
        tetrisPiece.GetComponent<TetrisPiece>().SetFormation(spawnPos, pieceFormation);
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
        if (tileObjects[pos] == null) { tileObjects[pos] = tile; }
        
        //Set the image if there is a gameobject there, and set it to be occupied
        tileObjects[pos].GetComponent<Image>().sprite = tile.GetComponent<Image>().sprite;
        tileOccupancy[pos] = true;
    }

    /// <summary>
    /// Test for lines that can be cleared and clear them
    /// </summary>
    /// <param name="lines">The lines that can be cleared</param>
    public void ClearLines(List<int> lines)
    {
        //Lines that will be cleared
        List<int> linesToClear = new List<int>();

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
                tileObjects[pos].GetComponent<Image>().sprite = SpriteSheetManager.sprites["Tiles_15"];
            }
        }

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

                //If tile object doesn't exist continue
                if (tileObjects[pos] == null) { continue; }

                //if (tileObjects[pos + Vector2.down * fallHeight].GetComponent<Image>().sprite = tileObjects[pos].GetComponent<Image>().sprite)

                //Set the tile on top of the gap to the bottom of the gap and set the position to be occupied
                tileObjects[pos + Vector2.down * fallHeight].GetComponent<Image>().sprite = tileObjects[pos].GetComponent<Image>().sprite;
                tileOccupancy[pos + Vector2.down * fallHeight] = tileOccupancy[pos];

                //Unoccupy the tile on top
                tileOccupancy[pos] = false;
            }
        }
    }
}
