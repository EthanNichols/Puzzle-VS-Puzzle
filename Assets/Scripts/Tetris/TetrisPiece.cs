using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisPiece : MonoBehaviour
{

    [HideInInspector]
    public GameObject ghostPiece;

    public char shape;
    public Sprite tileSprite;
    public float tileSize;
    public TetrisField field;

    public float fallingTimer;
    private float fallingTimerReset;

    public float moveTimer = .1f;
    private float moveTimeReset;

    private int rotation = 0;
    private Vector2 rotationCenter;

    private List<Vector2> tilePositions = new List<Vector2>();
    private List<GameObject> tiles = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        //Set the reset for the timer
        fallingTimerReset = fallingTimer;
        moveTimeReset = moveTimer;


        moveTimer = int.MaxValue;

        if (ghostPiece != null)
        {
            if (CanFall()) { fallingTimer = 0; Fall(); }
            else { field.gameOver = true; }

            if (shape == 'o' || shape == 's' || shape == 'z')
            {
                if (CanFall()) { fallingTimer = 0; Fall(); }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!ghostPiece) { return; }

        //Start the timers
        fallingTimer -= Time.deltaTime;

        if (fallingTimer <= 0) { Fall(); }

        MoveInput();
    }

    /// <summary>
    /// Set the position and formation of the tetris piece tiles
    /// </summary>
    /// <param name="spawnPos">The grid position the piece spawns at</param>
    /// <param name="formation">The list of position each tile is at</param>
    public void SetFormation(Vector2 spawnPos, Queue<Vector2> formation)
    {
        //Set the formation of the piece
        foreach (Transform tile in transform)
        {
            //Set the position and size of the tile piece
            tile.transform.localPosition = formation.Peek() * tileSize;
            tile.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
            tile.GetComponent<Image>().sprite = tileSprite;

            //Set the grid position of the tile piece
            tilePositions.Add(spawnPos + formation.Peek());
            tiles.Add(tile.gameObject);

            tile.gameObject.SetActive(false);
            formation.Dequeue();
        }

        //Set the position of the entire piece
        transform.localPosition = field.tilePositions[spawnPos + Vector2.down] + Vector2.up * tileSize;
        rotationCenter = spawnPos;

        ghostPiece = Instantiate(gameObject, Vector3.zero, Quaternion.identity, transform.parent.transform);
        ghostPiece.name = "GhostPiece";
        ghostPiece.GetComponent<TetrisPiece>().tiles.Clear();
        ghostPiece.GetComponent<TetrisPiece>().ghostPiece = null;
        foreach (Transform tile in ghostPiece.transform)
        {
            ghostPiece.GetComponent<TetrisPiece>().tiles.Add(tile.gameObject);
            tile.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        }

        ResetGhostPiece();
    }

    /// <summary>
    /// Make the piece fall one tile
    /// </summary>
    private void Fall()
    {
        do
        {
            //Make sure the piece can fall a tile
            if (CanFall())
            {

                //Move all of the tiles in the piece down a tile
                for (int i = 0; i < tilePositions.Count; i++)
                {
                    tilePositions[i] += Vector2.down;
                    if (tilePositions[i].y < 20) { tiles[i].SetActive(true); }
                    else { tiles[i].SetActive(false); }
                }

                //Move the entire piece down a tile
                transform.localPosition += Vector3.down * tileSize;
                rotationCenter += Vector2.down;
            }
            else
            {
                if (ghostPiece == null) { fallingTimer = int.MaxValue; return; }

                //The lines that could be clear
                List<int> clearLines = new List<int>();

                //Place the tile pieces onto the field
                for (int i = 0; i < tiles.Count; i++)
                {
                    field.PlaceTile(tilePositions[i], tiles[i]);
                    if (!clearLines.Contains((int)tilePositions[i].y)) { clearLines.Add((int)tilePositions[i].y); }
                }

                //Determine if a line can be cleared and clear it
                field.GetClearLines(clearLines);

                //Destroy the tetris piece
                Destroy(ghostPiece);
                Destroy(gameObject);
                break;
            }
        } while ((fallingTimer += fallingTimerReset) < 0);

        //Reset the timer
        fallingTimer = fallingTimerReset;

        ResetGhostPiece();
    }

    /// <summary>
    /// Whether the piece can fall
    /// </summary>
    /// <returns>Whether the piece can fall</returns>
    private bool CanFall()
    {
        //Test if a tile can't fall return false
        foreach (Vector2 pos in tilePositions)
        {
            if (field.TestOccupancy(pos + Vector2.down)) { return false; }
        }

        //The piece can fall
        return true;
    }

    /// <summary>
    /// Get inputs and call functions to move the piece
    /// </summary>
    private void MoveInput()
    {
        //The timer between automatic movement when a button is held down
        moveTimer -= Time.deltaTime;

        //Get input to rotate the piece
        if (Input.GetKeyDown(KeyCode.E)) { Rotate(1); }
        else if (Input.GetKeyDown(KeyCode.Q)) { Rotate(-1); }
    
        //Get input to move the piece to the left
        if (Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.B))
        {
            if (moveTimer <= 0)
            {
                Move(Vector2.left);
                moveTimer = moveTimeReset;
            }
        }

        //Get input to more the piece to the right
        else if (Input.GetKey(KeyCode.D) &&
                !Input.GetKey(KeyCode.A))
        {
            if (moveTimer <= 0)
            {
                Move(Vector2.right);
                moveTimer = moveTimeReset;
            }

        //Get input to make the piece fall a tile
        } else if (Input.GetKey(KeyCode.S))
        {
            if (moveTimer <= 0)
            {
                Fall();
                moveTimer = moveTimeReset;
                fallingTimer = fallingTimerReset;
            }
        }

        //Get input to put the piece in its place
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            fallingTimer = int.MinValue;
            Fall();
        } else
        {
            moveTimer = 0;
        }
    }

    /// <summary>
    /// Test if the piece can move in the direction, and possibly place it in the new position
    /// </summary>
    /// <param name="direction">The direction the piece will move to</param>
    private void Move(Vector2 direction)
    {
        //Return if the new position of a tile would enter an occupied space
        for (int i = 0; i < tilePositions.Count; i++)
        {
            if (field.TestOccupancy(tilePositions[i] + direction)) { return; }
        }

        //Move the tiles to the new position
        for (int i = 0; i < tilePositions.Count; i++)
        {
            tilePositions[i] += direction;
            if (tilePositions[i].y < 20) { tiles[i].SetActive(true); }
            else { tiles[i].SetActive(false); }
        }

        //Move the overall piece to the new position
        transform.position += (Vector3)(direction * tileSize);
        rotationCenter += direction;

        ResetGhostPiece();
    }

    /// <summary>
    /// Test if the piece can rotate and set it to the new orientation
    /// </summary>
    /// <param name="direction"></param>
    private void Rotate(int direction)
    {
        //The new position, rotation, and 
        List<Vector2> newPositions = new List<Vector2>(tilePositions);
        int newRotation = rotation + direction;
        Vector2 piecePos = transform.localPosition / tileSize;

        //Make sure it is only rotated in 4 directions
        if (newRotation < 0) { newRotation = 3; }
        else if (newRotation > 3) { newRotation = 0; }

        //Rotate the piece by the shape
        if (shape == 'i')
        {
            if (newRotation == 0)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 0);
                newPositions[3] = new Vector2(2, 0);
            }
            else if (newRotation == 1)
            {
                newPositions[0] = new Vector2(1, 1);
                newPositions[1] = new Vector2(1, 0);
                newPositions[2] = new Vector2(1, -1);
                newPositions[3] = new Vector2(1, -2);
            }
            else if (newRotation == 2)
            {
                newPositions[0] = new Vector2(-1, -1);
                newPositions[1] = new Vector2(0, -1);
                newPositions[2] = new Vector2(1, -1);
                newPositions[3] = new Vector2(2, -1);
            }
            else if (newRotation == 3)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, -1);
                newPositions[3] = new Vector2(0, -2);
            }
        }
        else if (shape == 't')
        {
            if (newRotation == 0)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, -1);
                newPositions[3] = new Vector2(1, 0);
            }
            else if (newRotation == 1)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, 1);
                newPositions[3] = new Vector2(0, -1);
            }
            else if (newRotation == 2)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, 1);
                newPositions[3] = new Vector2(1, 0);
            }
            else if (newRotation == 3)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 0);
                newPositions[3] = new Vector2(0, -1);
            }
        }
        else if (shape == 'j')
        {
            if (newRotation == 0)
            {
                newPositions[0] = new Vector2(1, -1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(-1, 0);
                newPositions[3] = new Vector2(1, 0);
            }
            else if (newRotation == 1)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, -1);
                newPositions[3] = new Vector2(-1, -1);
            }
            else if (newRotation == 2)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 0);
                newPositions[3] = new Vector2(-1, 1);
            }
            else if (newRotation == 3)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 1);
                newPositions[3] = new Vector2(0, -1);
            }
        }
        else if (shape == 'l')
        {
            if (newRotation == 0)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 0);
                newPositions[3] = new Vector2(-1, -1);
            }
            else if (newRotation == 1)
            {
                newPositions[0] = new Vector2(0, -1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, 1);
                newPositions[3] = new Vector2(-1, 1);
            }
            else if (newRotation == 2)
            {
                newPositions[0] = new Vector2(1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(-1, 0);
                newPositions[3] = new Vector2(1, 1);
            }
            else if (newRotation == 3)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, -1);
                newPositions[3] = new Vector2(1, -1);
            }
        }
        else if (shape == 's')
        {
            if (newRotation == 0)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, 1);
                newPositions[3] = new Vector2(1, 1);
            }
            else if (newRotation == 1)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 0);
                newPositions[3] = new Vector2(1, -1);
            }
            else if (newRotation == 2)
            {
                newPositions[0] = new Vector2(1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, -1);
                newPositions[3] = new Vector2(-1, -1);
            }
            else if (newRotation == 3)
            {
                newPositions[0] = new Vector2(0, -1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(-1, 0);
                newPositions[3] = new Vector2(-1, 1);
            }
        }
        else if (shape == 'z')
        {
            if (newRotation == 0)
            {
                newPositions[0] = new Vector2(1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, 1);
                newPositions[3] = new Vector2(-1, 1);
            }
            else if (newRotation == 1)
            {
                newPositions[0] = new Vector2(0, -1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(1, 0);
                newPositions[3] = new Vector2(1, 1);
            }
            else if (newRotation == 2)
            {
                newPositions[0] = new Vector2(-1, 0);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(0, -1);
                newPositions[3] = new Vector2(1, -1);
            }
            else if (newRotation == 3)
            {
                newPositions[0] = new Vector2(0, 1);
                newPositions[1] = new Vector2(0, 0);
                newPositions[2] = new Vector2(-1, 0);
                newPositions[3] = new Vector2(-1, -1);
            }
        }
        else
        {
            return;
        }

        //Test if the new positions won't be in a place that is already occupied
        for (int i = 0; i < newPositions.Count; i++)
        {
            if (field.TestOccupancy(rotationCenter + newPositions[i])) { return; }
        }

        //Set the new positions of the tiles relative to the center rotation point
        for (int i = 0; i < newPositions.Count; i++)
        {
            tilePositions[i] = rotationCenter + newPositions[i];

            if (tilePositions[i].y < 20) { tiles[i].SetActive(true); }
            else { tiles[i].SetActive(false); }

            tiles[i].transform.localPosition = newPositions[i] * tileSize;
        }

        //Set the new rotation of the piece
        rotation = newRotation;

        ResetGhostPiece();
    }

    /// <summary>
    /// Return the ghost piece to the actual piece location and set all the tile positions
    /// </summary>
    private void ResetGhostPiece()
    {
        if (!ghostPiece) { return; }

        ghostPiece.transform.localPosition = transform.localPosition;
        ghostPiece.GetComponent<TetrisPiece>().rotationCenter = rotationCenter;
        ghostPiece.GetComponent<TetrisPiece>().rotation = rotation;
        ghostPiece.GetComponent<TetrisPiece>().tilePositions = new List<Vector2>(tilePositions);

        ghostPiece.GetComponent<TetrisPiece>().fallingTimer = int.MinValue;
        ghostPiece.GetComponent<TetrisPiece>().fallingTimerReset = 0;
        ghostPiece.GetComponent<TetrisPiece>().Fall();

        for (int i=0; i<tilePositions.Count; i++)
        {
            ghostPiece.GetComponent<TetrisPiece>().tiles[i].transform.localPosition = tiles[i].transform.localPosition;
        }
    }
}
