using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayField : MonoBehaviour {

    public int width;
    public int height;

    [HideInInspector]
    public int players;

    public float tileSize;
    public Dictionary<Vector2, Vector2> tilePositions = new Dictionary<Vector2, Vector2>();
    public Dictionary<Vector2, GameObject> tileObjects = new Dictionary<Vector2, GameObject>();

    private GameObject tempBackgroundTile;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    /// <summary>
    /// Load the resources needed for the playfield
    /// </summary>
    public void LoadResources()
    {
        tempBackgroundTile = Resources.Load("Temp Background Tile") as GameObject;
    }

    /// <summary>
    /// Calculate the size that the tiles will be in the playfield
    /// </summary>
    public void CalcTileSize(int padding)
    {
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
    /// Create the play field for the player
    /// </summary>
    /// <param name="width">Width of the play field</param>
    /// <param name="height">Height of the play field</param>
    public void CreateField(int width, int height, Sprite backgroundTile)
    {
        //Empty object to hold all the background tiles
        GameObject background = new GameObject();
        background.name = "Background";
        background.transform.SetParent(transform);
        background.transform.localPosition = Vector2.zero;

        //Create the play area with the given width and height
        for (int x=-width / 2; x<width / 2 + width % 2; x++)
        {
            for (int y=-height / 2; y<height / 2 + height % 2; y++)
            {
                //Create the background tile and calculate the position that it has
                GameObject newTile = Instantiate(tempBackgroundTile, Vector2.zero, Quaternion.identity, background.transform);
                newTile.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
                newTile.transform.localPosition = new Vector3(x + ((width + 1) % 2) / 2f, y + ((height + 1) % 2) / 2f) * tileSize;
                newTile.name = (x + width/2) + ", " + (y + height/2);

                newTile.GetComponent<Image>().sprite = backgroundTile;

                //Add the grid and actual position to the dictionary
                tilePositions[new Vector2(x + width / 2, y + height / 2)] = newTile.transform.localPosition;
                tileObjects[new Vector2(x + width / 2, y + height / 2)] = newTile;
            }
        }
    }
    
    /// <summary>
    /// Create invisible tiles above the play area
    /// </summary>
    /// <param name="width">Width of the play area</param>
    /// <param name="height">Height of the buffer area</param>
    public void CreateTileBuffer(int width, int height)
    {
        //Add the positions to the dictionary
        for (int x=0; x<width; x++)
        {
            for (int y=height; y<height * 2; y++)
            {
                tilePositions[new Vector2(x, y)] = new Vector2(int.MaxValue, int.MaxValue);
                tileObjects[new Vector2(x, y)] = null;
            }
        }
    }

    public void ShakeField(float duration, Vector3 position = default(Vector3), Vector3 direction = default(Vector3))
    {
        FieldShake shaker = GetComponent<FieldShake>();

        shaker.shakeDuration = duration;
        shaker.shakePosition = position;
    }
}
