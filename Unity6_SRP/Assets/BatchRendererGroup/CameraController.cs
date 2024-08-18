using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public Vector3 EndPos;
	public float totalTime = 10;

	Vector3 startPos;
	float time = 0;
	void Awake()
	{
		startPos = transform.position;
		time = 0;
	}
	void Update()
	{
		time += Time.deltaTime;
		if(time >= totalTime)
		{
			time = 0;
		}
		transform.position = Vector3.Lerp(startPos, EndPos, time/totalTime);
	}
}