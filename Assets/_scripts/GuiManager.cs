using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiManager : MonoBehaviour {

    public static GuiManager instance;
    public GameObject[] players;
    private int startingPlayers;
    private int currentPlayers; 
    private bool hasRan = false;

    void Awake()
    {
        instance = this;
    }

	public void findPlayersAtStart()
    {
        if (hasRan = false)
        {
            players = GameObject.FindGameObjectsWithTag("Players");
            startingPlayers = players.Length;
            hasRan = true; 
        } else
        {
            //lol
        }

    }


    

    public void findCurrentPlayers()
    {
        if (currentPlayers < startingPlayers)
        {

        }
        
    }


}
