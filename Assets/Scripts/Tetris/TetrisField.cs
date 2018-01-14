using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisField : PlayField {

    [HideInInspector]
    public int width = 10;
    public int height = 20;

    private GameObject tetrisPieceObj;
    private List<Vector2> spawnLocations = new List<Vector2>();

	// Use this for initialization
	void Start () {

        //Calculate the sice of the tiles
        CalcTileSize();

        //Load the resources for the play field
        tetrisPieceObj = Resources.Load("TetrisPiece") as GameObject;
        LoadResources();

        //Create the visible and hidden play field
        CreateField(width, height);
        CreateTileBuffer(width, height);

        //Set the spawn locations for tetris pieces
        spawnLocations.Add(new Vector2(width / 2, height));
        if (width % 2 == 0) { spawnLocations.Add(new Vector2(width / 2 - 1, height)); }

        //TEMPERARY
        SpawnPiece();
	}
	
	// Update is called once per frame
	void Update () {
		
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

        //Set the positions of each tile in the piece
        switch (randPiece)
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
                pieceFormation.Enqueue(Vector2.up);
                pieceFormation.Enqueue(Vector2.right);
                break;

            //jPiece
            case 3:
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(new Vector2(-1, 1));
                break;

            //lPiece
            case 4:
                pieceFormation.Enqueue(Vector2.left);
                pieceFormation.Enqueue(Vector2.zero);
                pieceFormation.Enqueue(Vector2.right);
                pieceFormation.Enqueue(new Vector2(1, 1));
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

        //Set the spawn location for the piece
        Vector2 spawnPos = spawnLocations[0];
        if (randPiece <= 1) { spawnPos = spawnLocations[spawnLocations.Count - 1]; }

        //Create a new piece
        GameObject newPiece = Instantiate(tetrisPieceObj, transform);

        //Set information about the piece
        newPiece.GetComponent<TetrisPiece>().fallingTimer = 2;
        newPiece.GetComponent<TetrisPiece>().tileSize = tileSize;
        newPiece.GetComponent<TetrisPiece>().field = this;

        //Spawn the piece and set the tile location
        newPiece.GetComponent<TetrisPiece>().SetFormation(spawnPos, pieceFormation);
    }
}
