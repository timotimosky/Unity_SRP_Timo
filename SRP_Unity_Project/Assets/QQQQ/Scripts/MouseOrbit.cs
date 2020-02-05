using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOrbit : MonoBehaviour
{
	Transform target ;
	float distance = 10.0f;

	float xSpeed = 250.0f;
	float ySpeed = 120.0f;

	float yMinLimit = -20;
	float yMaxLimit = 80;

	private float x = 0.0f;
	private float y = 0.0f;

	 [AddComponentMenu("Camera-Control/Mouse Orbit")]
	void Start()
	{
		var angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	void LateUpdate()
	{
		if (target)
		{
			if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt)) // Alt+Left button pressed
			{
				x += Input.GetAxis("Mouse X") * xSpeed * 0.04f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.04f;

				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}
			else if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt)) // Alt+Right button pressed
			{
				distance += (Input.GetAxis("Mouse Y") - Input.GetAxis("Mouse X")) * ySpeed * 0.02f;
			}

			// Add mouse-scroll wheel
			distance -= Input.GetAxis("Mouse ScrollWheel") * ySpeed * 0.02f;
			// Clamp distance so we don't go through the origin of the target.	 	
			distance = Mathf.Clamp(distance, 1.0f, 250.0f);

			var rotation = Quaternion.Euler(y, x, 0);
			var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

			transform.rotation = rotation;
			transform.position = position;
		}
	}

	static float ClampAngle(float angle , float min , float max )
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp(angle, min, max);
	}
}
