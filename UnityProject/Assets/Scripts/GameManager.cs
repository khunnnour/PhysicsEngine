using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	[Header("UI Elements")]
	public GameObject winHolder;
	public Text basketText;
	public Text timerText;
	public Text winText;
	public Text scoreText;
	[Header("Scoring")]
	public float baseScore = 1.0f;
	public float maxScoreModifier = 3.0f;
	public float threeDistance = 4.5f;
	[Header("Game Settings")]
	public float totalTime = 45f;
	public float scoreRequired = 20f;

	private Transform play;
	private Transform hoop;
	private bool gameFinished;
	private float score;
	private float timer;
	private int baskets;

	void Awake()
	{
		instance = this;

		//play = GameObject.FindGameObjectWithTag("Player").transform;
		//hoop = GameObject.FindGameObjectWithTag("Hoop").transform;
		//
		//winHolder.SetActive(false);
	}

	// Start is called before the first frame update
	void Start()
	{
		gameFinished = false;

		ResetGame();
		//UpdateScoreUI();
	}

	// Update is called once per frame
	void Update()
	{
		//if (GetGameRunning())
		//	UpdateTimer();
		//
		//if (Input.GetKeyDown(KeyCode.R))
		//	ResetGame();
		//
		//if (timer < 0)
		//	EndGame();
	}

	private void ResetGame()
	{
		score = 0f;
		baskets = 0;
		timer = totalTime;
		gameFinished = false;

		//GameObject.FindGameObjectWithTag("Ball").GetComponent<BasketballScript>().RetrieveLure();
		//
		//winHolder.SetActive(false);
		//UpdateScoreUI();
	}

	//private void UpdateTimer()
	//{
	//	// Update the timer	
	//	timer -= Time.deltaTime;
	//
	//	// Get total minutes passed
	//	int min = (int)(timer / 60f);
	//	// Get total seconds passed
	//	int sec = (int)(timer - (min * 60));
	//
	//	if (sec > 9)
	//		timerText.text = min.ToString("F0") + ":" + sec.ToString("F0");
	//	else
	//		timerText.text = min.ToString("F0") + ":0" + sec.ToString("F0");
	//}
	//
	//private void EndGame()
	//{
	//	// End game
	//	gameFinished = true;
	//
	//	// create win string
	//	string displayText = "You did ";
	//
	//	if (score >= scoreRequired)
	//		displayText += "it!";
	//	else
	//		displayText += "not do it!";
	//
	//	winText.text = displayText;
	//
	//	displayText = "You earned a total of " + score.ToString("F1") + " points";
	//	scoreText.text = displayText;
	//
	//	winHolder.SetActive(true);
	//}

	private void UpdateScoreUI()
	{
		basketText.text = "Baskets: " + baskets.ToString("F0");
	}

	public bool GetGameRunning()
	{
		return !gameFinished;
	}

	public void ScorePoint()
	{
		// Get distance and remove y
		Vector3 diff = hoop.position - play.position;
		diff.y = 0f;

		// Calculate distance modifier
		float mod = diff.magnitude / maxScoreModifier;
		mod = Mathf.Min(mod, 1f);

		// Calculate actual points
		float points = baseScore * maxScoreModifier * mod;

		// Update points
		score += points;

		baskets++;

		UpdateScoreUI();
	}
}
