using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

    public int players = 1;
    public GameObject Canvas;

	// Use this for initialization
	public virtual void Start () {
        CreateFields();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Create a play field for each player
    /// </summary>
    private void CreateFields()
    {
        for (int i=0; i<players; i++)
        {
            //Create an empty object for the player's field
            GameObject field = new GameObject();
            field.name = "Player " + (i+1) + " Field";

            //Add the player field to the canvas and set the position relative to the player amount
            field.transform.SetParent(Canvas.transform);
            field.transform.localPosition = new Vector2(-Screen.width / 2f + (Screen.width / (float)players) * (i + .5f), 0);

            //Set the type of the field the player has
            field.AddComponent<TetrisField>();
            field.GetComponent<TetrisField>().players = players;
        }
    }
}
