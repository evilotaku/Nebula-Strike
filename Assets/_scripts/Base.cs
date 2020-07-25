using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{

	public int maxHP;
	public int currentHP;
	public int team;
	public GameObject deathEffect;
	public GameObject finalDeathEffect;
	bool isDying = false;

	public void TakeDamage (int dmg)
	{
		if (isDying) 
		{
			return;
		}
		currentHP -= dmg;
		if (currentHP <= 0) 
		{
			isDying = true;    
			StartCoroutine (Death ());
		}
	}

	public void Heal (int hp)
	{
		if (isDying) {
			return;
		}

		if (currentHP < maxHP) {
			currentHP += hp;
			currentHP = Mathf.Clamp (currentHP, 0, maxHP);
		}


	}

	IEnumerator Death ()
	{
		GameObject death = Instantiate (deathEffect);
		death.transform.position = transform.position;
		GameObject.Destroy (death, 3);
		yield return new WaitForSeconds (3);
		GameObject finalDeath = Instantiate (finalDeathEffect);
		finalDeath.transform.position = transform.position;
		GameObject.Destroy (finalDeath, 2);
		GameObject.Destroy (gameObject);
	}
	// Use this for initialization
	void Start ()
	{


    }
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
