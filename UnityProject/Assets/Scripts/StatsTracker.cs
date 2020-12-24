using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsTracker : MonoBehaviour
{
	public static StatsTracker instance;

	public enum Type
	{
		INVALID = -1,
		GRAV = 0,
		DRAG = 1,
		NORM,
		FRIC
	};

	List<int> gravTimes, dragTimes, normTimes, fricTimes;

	private void Awake()
	{
		instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		gravTimes = new List<int>();
		dragTimes = new List<int>();
		normTimes = new List<int>();
		fricTimes = new List<int>();
	}

	public void ReportTime(Type t, int d)
	{
		switch (t)
		{
			case Type.GRAV:
				gravTimes.Add(d);
				break;
			case Type.DRAG:
				dragTimes.Add(d);
				break;
			case Type.NORM:
				normTimes.Add(d);
				break;
			case Type.FRIC:
				fricTimes.Add(d);
				break;
			case Type.INVALID:
			default:
				break;
		}
	}

	public void PrintAllTimes()
	{
		if (dragTimes.Count > 1)
			Debug.Log("Drag: avg = " + avg(dragTimes).ToString() + " | N = " + dragTimes.Count.ToString());
		
		if (normTimes.Count > 1)
			Debug.Log("Normal: avg = " + avg(normTimes).ToString() + " | N = " + normTimes.Count.ToString());

		if (fricTimes.Count > 1)
			Debug.Log("Friction: avg = " + avg(fricTimes).ToString() + " | N = " + fricTimes.Count.ToString());
	}

	int avg(List<int> l)
	{
		int tot = 0;
		for (int i = 1; i < l.Count; i++)
		{
			tot += l[i];
		}
		return tot / l.Count;
	}
}
