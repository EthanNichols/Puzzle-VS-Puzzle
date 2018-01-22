using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSheetManager : MonoBehaviour {

    private static Dictionary<string, List<Color>> colorPalletes = new Dictionary<string, List<Color>>();
    private static Dictionary<string, Sprite> baseSprites = new Dictionary<string, Sprite>();
    private static Dictionary<string, Sprite> createdSprites = new Dictionary<string, Sprite>();

	// Use this for initialization
	void Start () {
        SetBaseSprites();
        SetColorPalletes();
    }

    /// <summary>
    /// Get all of the base sprites
    /// </summary>
    private void SetBaseSprites()
    {
        foreach(Sprite sprite in Resources.LoadAll<Sprite>("Textures/SpriteSheetBase"))
        {
            //Get the name and remove extra string info
            string name = sprite.ToString();
            name = name.Split(' ')[0];

            //Add the sprite to the base textures
            baseSprites[name] = sprite;
        }
    }

    /// <summary>
    /// Set all the possible color palletes and put them in the pallete dictionary
    /// </summary>
    private void SetColorPalletes()
    {
        colorPalletes["Tomato"] = new List<Color>()     { hexColor("FDF5F1"), hexColor("FE935A"), hexColor("E25322"), hexColor("A82424"), hexColor("691B28"), hexColor("1B141E") };
        colorPalletes["Caramel"] = new List<Color>()    { hexColor("FDF5F1"), hexColor("FFBF89"), hexColor("E7825A"), hexColor("BE5340"), hexColor("7A321C"), hexColor("1B141E") };
        colorPalletes["Chocolate"] = new List<Color>()  { hexColor("FDF5F1"), hexColor("FFBF89"), hexColor("D08058"), hexColor("974E49"), hexColor("5A303F"), hexColor("1B141E") };
        colorPalletes["Cheese"] = new List<Color>()     { hexColor("FDF5F1"), hexColor("FFC95C"), hexColor("EB8A06"), hexColor("BE5340"), hexColor("7A321C"), hexColor("1B141E") };
        colorPalletes["Spinach"] = new List<Color>()    { hexColor("FDF5F1"), hexColor("CDE042"), hexColor("68B229"), hexColor("257D2C"), hexColor("1B4E44"), hexColor("1B141E") };
        colorPalletes["Mint"] = new List<Color>()       { hexColor("FDF5F1"), hexColor("7BECBF"), hexColor("38AA91"), hexColor("29777E"), hexColor("25446C"), hexColor("1B141E") };
        colorPalletes["Water"] = new List<Color>()      { hexColor("FDF5F1"), hexColor("5ED7EF"), hexColor("2096CD"), hexColor("2662AB"), hexColor("303386"), hexColor("1B141E") };
        colorPalletes["Thistle"] = new List<Color>()    { hexColor("FDF5F1"), hexColor("A3CCFF"), hexColor("788DDE"), hexColor("5458C0"), hexColor("303386"), hexColor("1B141E") };
        colorPalletes["Lilac"] = new List<Color>()      { hexColor("FDF5F1"), hexColor("EFA1CE"), hexColor("B66CBE"), hexColor("74448D"), hexColor("432F65"), hexColor("1B141E") };
        colorPalletes["Rose"] = new List<Color>()       { hexColor("FDF5F1"), hexColor("FFB2B2"), hexColor("EA6D9D"), hexColor("AF407F"), hexColor("75224A"), hexColor("1B141E") };
        colorPalletes["Cherry"] = new List<Color>()     { hexColor("FDF5F1"), hexColor("FFB2B2"), hexColor("EB7171"), hexColor("B1415C"), hexColor("75224A"), hexColor("1B141E") };
        colorPalletes["Ancient"] = new List<Color>()    { hexColor("FDF5F1"), hexColor("E3C4B0"), hexColor("B18E8E"), hexColor("74647F"), hexColor("3E3F64"), hexColor("1B141E") };
        colorPalletes["Green Tea"] = new List<Color>()  { hexColor("FDF5F1"), hexColor("DFDD9A"), hexColor("9CAA74"), hexColor("617B47"), hexColor("2A4E32"), hexColor("1B141E") };
        colorPalletes["Power"] = new List<Color>()      { hexColor("FDF5F1"), hexColor("B8D8D1"), hexColor("759DA9"), hexColor("526A98"), hexColor("3E3F64"), hexColor("1B141E") };
        colorPalletes["Tarmac"] = new List<Color>()     { hexColor("FDF5F1"), hexColor("CCC1BE"), hexColor("918692"), hexColor("5D5B6E"), hexColor("38384C"), hexColor("1B141E") };
    }

    /// <summary>
    /// Convert Hexadecimal numbers to color
    /// </summary>
    /// <param name="hex">Hex string</param>
    /// <returns></returns>
    private Color hexColor(string hex)
    {
        //Add a '#' to the hex string if there isn't one already
        if (!hex.StartsWith("#")) { hex = hex.Insert(0, "#"); }

        //Set and return the color
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    /// <summary>
    /// Get the sprite
    /// </summary>
    /// <param name="_spriteName">Base sprite name</param>
    /// <param name="color">Color pallette name</param>
    /// <param name="darkness">The darkness of the pallete</param>
    /// <returns></returns>
    public static Sprite GetSprite(string _spriteName, string color, int darkness = 0)
    {
        //Determine the sprites' name
        string spriteName = _spriteName + "." + color + "." + darkness.ToString();

        //If the sprite exists return the sprite, else create it then retun the sprite
        if (createdSprites.ContainsKey(spriteName)) { return createdSprites[spriteName]; }
        else { createdSprites[spriteName] = CreateSprite(_spriteName, color, darkness); }

        createdSprites[spriteName].name = spriteName;
        return createdSprites[spriteName];
    }

    /// <summary>
    /// Create a new texture and sprite to add to the created sprites
    /// </summary>
    /// <param name="spriteName">Base sprite name</param>
    /// <param name="color">Color pallete name</param>
    /// <param name="darkness">The darkness of the pallete</param>
    /// <returns></returns>
    private static Sprite CreateSprite(string spriteName, string color, int darkness)
    {
        //Get the color pallete for the sprite
        List<Color> pallete = colorPalletes[color];

        //Calculate the percentage all the colors take up
        float colorPercent = 100 / (float)(pallete.Count - 1);

        //Get the base sprite from the original texture
        Sprite baseSprite = baseSprites[spriteName];

        //Create a new texture and set it's display properties
        Texture2D croppedTexture = new Texture2D((int)baseSprite.rect.width, (int)baseSprite.rect.height);
        croppedTexture.wrapMode = TextureWrapMode.Clamp;
        croppedTexture.filterMode = FilterMode.Point;

        //Get the pixels in the original texture and set the new texture's pixels
        Color[] pixels = baseSprite.texture.GetPixels((int)baseSprite.textureRect.x, (int)baseSprite.textureRect.y, (int)baseSprite.rect.width, (int)baseSprite.rect.height);
        croppedTexture.SetPixels(pixels);

        //Recolor the pixels in the new texture
        for (int x=0; x< baseSprite.rect.width; x++)
        {
            for (int y=0; y<baseSprite.rect.height; y++)
            {
                //Calculate the color that it should be
                int palleteNum = Mathf.FloorToInt((croppedTexture.GetPixel(x, y).r * 100) / colorPercent) + 1 - darkness;

                //Make sure the color id is within the pallete
                if (palleteNum <= 0) { palleteNum = 0; }
                else if (palleteNum >= pallete.Count) { palleteNum = pallete.Count - 1; }

                Color newColor = pallete[(pallete.Count - 1) - palleteNum];
                newColor.a = croppedTexture.GetPixel(x, y).a;

                //Set the new color of the pixel
                croppedTexture.SetPixel(x, y, newColor);
            }
        }
        //Apply the changes new texture
        croppedTexture.Apply();

        //Create a new sprite using the new texture
        Sprite newSprite = Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), Vector2.zero);

        return newSprite;
    }
}
