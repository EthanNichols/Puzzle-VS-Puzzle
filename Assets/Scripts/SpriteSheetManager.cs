using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSheetManager : MonoBehaviour {

    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

	// Use this for initialization
	void Start () {

        //Get all of the sprites in the spritesheet
        foreach (Sprite sprite in Resources.LoadAll<Sprite>("Textures")) {

            string name = sprite.ToString();
            name = name.Remove(name.Length - " (UnityEngine.Sprite)".Length, " (UnityEngine.Sprite)".Length);

            sprites[name] = sprite;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
