using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFightField : PlayField {

    private List<Sprite> tileSprites = new List<Sprite>();
    private Sprite backgroundSprite;

    // Use this for initialization
    void Start () {

        //Temp
        width = 6;
        height = 13;

        SetTileSprites();

        //Calculate the sice of the tiles
        CalcTileSize(3);

        //Load the resources for the play field
        LoadResources();

        //Create the visible and hidden play field
        CreateField(width, height, backgroundSprite);
        CreateTileBuffer(width, height);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Creates/load the sprites that will be used for the play area and pieces
    /// </summary>
    public void SetTileSprites()
    {
        int darkness = 0;
        string tile = "RoundedBlock";

        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Tomato", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Cheese", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Spinach", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Water", darkness));
        tileSprites.Add(SpriteSheetManager.GetSprite(tile, "Lilac", darkness));

        backgroundSprite = SpriteSheetManager.GetSprite(tile, "Tarmac", darkness);
    }
}
