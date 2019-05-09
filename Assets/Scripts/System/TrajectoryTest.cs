using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryTest : MonoBehaviour {

    public GameObject TrajectoryHelper;
    public Transform MaxHeight;
	public Transform StartPos;
	public Transform EndPos;
    public float AngleI;
	public float InitialSpeed;
	public float Gravity;
	public int Interval;

	private Transform[] helpers;
	private float height;
	private float time;
	private float timeInterval;
	private float distance;
	// Update is called once per frame

	void Awake()
	{
		helpers = new Transform[Interval + Interval];
		for (int i = 0; i < Interval + Interval; i++){
			helpers[i] = Instantiate(TrajectoryHelper).transform;
		}
	}
	
	
	void Update ()
	{
		height = Mathf.Pow(InitialSpeed, 2f) * Mathf.Pow(Mathf.Sin(AngleI * Mathf.Deg2Rad), 2f) / (2 * Gravity);
		distance =  Mathf.Pow(InitialSpeed, 2f) * Mathf.Sin(2*AngleI * Mathf.Deg2Rad) / Gravity;
		time =  (2 * InitialSpeed * Mathf.Sin(AngleI * Mathf.Deg2Rad)) / Gravity;
		timeInterval = time / (float)Interval;
		
		
		EndPos.position = new Vector3(StartPos.position.x + distance,StartPos.position.y,EndPos.position.y);
		MaxHeight.position = new Vector3(StartPos.position.x + distance/2f,StartPos.position.y + height,StartPos.position.y);

	
		for (int i = 0; i < Interval + Interval ; i++)
		{
			var x = (i + 1) * distance / Interval;
			var y = x * Mathf.Tan(AngleI * Mathf.Deg2Rad) - ((Gravity * Mathf.Pow(x, 2f)) /
			        (2 * Mathf.Pow(InitialSpeed, 2f) * Mathf.Pow(Mathf.Cos(AngleI * Mathf.Deg2Rad), 2f)));
			helpers[i].position = new Vector3(StartPos.position.x + x,StartPos.position.y + y,StartPos.position.y);
		}
		
	}
}
