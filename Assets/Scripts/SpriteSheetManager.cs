using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSheetManager : MonoBehaviour {

    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    /*
    public Texture spriteSheet;
    private Dictionary<string, List<Color>> colorPalletes = new Dictionary<string, List<Color>>();
    private Dictionary<string, Sprite> baseSprites = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> createdSprites = new Dictionary<string, Sprite>();
    */

	// Use this for initialization
	void Start () {

        int i = 0;
        //Get all of the sprites in the spritesheet
        foreach (Sprite sprite in Resources.LoadAll<Sprite>("Textures")) {

            string name = sprite.ToString();
            name = name.Remove(name.Length - " (UnityEngine.Sprite)".Length, " (UnityEngine.Sprite)".Length);

            sprites[name] = sprite;
        }

        //SetColorPalletes();
	}

    /*
    private void SetColorPalletes()
    {
        colorPalletes["Tomato"] = new List<Color>() {hexColor("FDF5F1"), hexColor("FE935A"), hexColor("E25322"), hexColor("E25322"), hexColor("E25322"), hexColor("E25322")};
    }

    private Color hexColor(string hex)
    {
        if (!hex.StartsWith("#")) { hex.Insert(0, "#"); }

        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    public Sprite GetSprite(string _spriteName, string color, int brightness)
    {
        string spriteName = color + _spriteName + brightness.ToString();

        if (createdSprites[spriteName]) { return createdSprites[spriteName]; }

        return null;
    }

    private Sprite CreateSprite(string _spriteName, string color, int brightness)
    {
        List<Color> pallete = colorPalletes[color];
        return null;
    }
    */
}
