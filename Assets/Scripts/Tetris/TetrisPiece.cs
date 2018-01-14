using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisPiece : MonoBehaviour {

    [HideInInspector]
    public float tileSize;
    public TetrisField field;

    public float fallingTimer;
    private float fallingTimerReset;

    private List<Vector2> tilePositions = new List<Vector2>();

	// Use this for initialization
	void Start () {

        //Set the reset for the timer
        fallingTimerReset = fallingTimer;
	}
	
	// Update is called once per frame
	void Update () {
        Fall();
    }

    /// <summary>
    /// Make the piece fall one tile
    /// </summary>
    private void Fall()
    {
        //Start the timers
        fallingTimer -= Time.deltaTime;

        //Test if the timer is finished
        if (fallingTimer <= 0)
        {
            //Reset the timer
            fallingTimer = fallingTimerReset;

            //Move all of the tiles in the piece down a tile
            for (int i = 0; i < tilePositions.Count; i++)
            {
                tilePositions[i] += Vector2.down;
            }

            //Move the entire piece down a tile
            transform.localPosition += Vector3.down * tileSize;
        }
    }

    /// <summary>
    /// Set the position and formation of the tetris piece tiles
    /// </summary>
    /// <param name="spawnPos">The grid position the piece spawns at</param>
    /// <param name="formation">The list of position each tile is at</param>
    public void SetFormation(Vector2 spawnPos, Queue<Vector2> formation)
    {
        //Set the formation of the piece
        foreach(Transform tile in transform)
        {
            //Set the position and size of the tile piece
            tile.transform.localPosition = formation.Peek() * tileSize;
            tile.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);

            //Set the grid position of the tile piece
            tilePositions.Add(formation.Peek() + Vector2.up);
            formation.Dequeue();
        }

        //Set the position of the entire piece
        transform.localPosition = field.tilePositions[spawnPos + Vector2.down] + Vector2.up * tileSize;
    }
}
