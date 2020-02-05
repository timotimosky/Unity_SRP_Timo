using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragCollider : MonoBehaviour
{

	// Set to true if we are currently dragging the collider
	// attached to the GameObject.
	private bool isDragging  = false;
	private Camera mainCamera ;
	Camera camera;
	private Vector3 initialPosition ;
	// How far away the object is when it is "clicked"
	private float distance ;

	//[NoSerialized]

	public QuaternionRotation quaternionScript ;

	Camera GetCamera()
	{
		if (camera != null) 
			return camera;
		else return Camera.main;
	}

	void Start()
	{
		mainCamera = GetCamera();
	}

	void Update()
	{

	}

	// OnMouseDown is called when the user has pressed
	// the mouse button while over the GUIElement or Collider.
	void OnMouseDown()
	{
		if (!Input.GetKey(KeyCode.LeftAlt)) // Left-Alt is used to manipulate the camera.
		{
			isDragging = true;
			initialPosition = transform.position;
			distance = Vector3.Distance(mainCamera.transform.position, transform.position) - (mainCamera.nearClipPlane);

			//		var hit : RaycastHit;
			//		if ( collider.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), hit, Mathf.Infinity ) )
			//		{
			//			distance = hit.distance + ( mainCamera.near/2.0f );	
			//		}		 
		}
	}

	void OnMouseUp()
	{
		isDragging = false;
	}

	void OnMouseDrag()
	{
		if (isDragging)
		{
			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			var newPosition = ray.GetPoint(distance); // mainCamera.ScreenToWorldPoint(Vector3(mousePos.x, mousePos.y, distance));
			if (quaternionScript != null)
			{
				quaternionScript.SetEndPoint(newPosition);
			}
		}
	}
}