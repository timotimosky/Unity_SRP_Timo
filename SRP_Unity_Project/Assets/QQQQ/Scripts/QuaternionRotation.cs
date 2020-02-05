using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionRotation : MonoBehaviour
{

	public Texture iconTexture ;
	public Transform target ;

	// Static ID to represent this quaternion.
	private static int sID = 0;

	private int myID ;
	private Camera mainCamera ;
	private Vector3 start ;
	private Vector3 end ;
	private float angle ;

	// The GUI window rectangle that shows the properties of the 
	// quaternion.
	private Rect windowRect  = new Rect(20, 20, 300, 50);

		// Set to true if either the start or end points have moved and the visual representation of
		// the vector needs to be updated.
	private bool vectorHasChanged  = false;
	// Set to true when we clicked on the collider for this game object.
	private bool drawVector  = false;

	private GameObject arrowBody ;
	private GameObject arrowHead ;
	public GameObject arrowBodyPrefab ;
	public GameObject arrowHeadPrefab ;
	Camera camera;

	Camera GetCamera()
	{
		if (camera != null) 
			return camera;
		else return Camera.main;
	}
	Renderer arrowBody_Renderer;
	void Start()
	{
		myID = sID++;
		mainCamera = GetCamera();

		// Create a new material that can be used to color the quaternion representation.
		var newMaterial = new Material(Shader.Find("Diffuse"));
		newMaterial.color = Color.red;

		// Create a clone of the arrow body prefab.
		arrowBody = GameObject.Instantiate(arrowBodyPrefab, transform.position, transform.rotation);

	    arrowBody_Renderer = arrowBody.GetComponent<Renderer>();

		arrowBody_Renderer.material = newMaterial;

		// Initially disable rendering of this GO.
		arrowBody_Renderer.enabled = false;

		// Create a clone of the arrow head prefab.
		arrowHead = GameObject.Instantiate(arrowHeadPrefab, transform.position, transform.rotation);
		arrowBody_Renderer.material = newMaterial;

		DragCollider dragCollider  = arrowHead.GetComponent("DragCollider") as DragCollider;
		dragCollider.quaternionScript = this;

		// Initially disable rendering of this GO.
		arrowBody_Renderer.enabled = false;
	}

	void Update()
	{
		if (vectorHasChanged && arrowBody != null && arrowHead != null)
		{
			var direction = (end - start).normalized;
			var arrowLength = Vector3.Distance(start, end);

			// Position and orient the arrow body correctly.
			arrowBody.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
			arrowBody.transform.position = start;

			arrowBody.transform.localScale = new Vector3(arrowBody.transform.localScale.x, arrowLength, arrowBody.transform.localScale.z);


			arrowBody_Renderer.enabled = true;
			arrowBody_Renderer.material.color = new Color(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));

			// Position and orient the arrow head correctly.
			arrowHead.transform.position = end;
			arrowHead.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
			arrowBody_Renderer.material.color = new Color(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
			arrowBody_Renderer.enabled = true;

			vectorHasChanged = false;
		}
	}

	void GUIControlWindow(int windowID  )
	{
		string angleAsText  = angle.ToString();
		GUILayout.BeginHorizontal();
		angleAsText = GUILayout.TextField(angleAsText, GUILayout.Width(50));
		try
		{
			angle = float.Parse(angleAsText);
		}
		catch (Exception e)
		{
			angle = 0.0f;
		};

		angle = GUILayout.HorizontalSlider(angle, 0.0f, 360.0f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("X")) // TODO: Make red
		{
			start = target.transform.position;
			end = start + Vector3.right * 2.0f;
			vectorHasChanged = true;
		}
		if (GUILayout.Button("Y")) // TODO: Make Green
		{
			start = target.transform.position;
			end = start + Vector3.up * 2.0f;
			vectorHasChanged = true;
		}
		if (GUILayout.Button("Z")) // TODO: Make Blue
		{
			start = target.transform.position;
			end = start + Vector3.forward * 2.0f;
			vectorHasChanged = true;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("-X")) // TODO: Make red
		{
			start = target.transform.position;
			end = start - Vector3.right * 2.0f;
			vectorHasChanged = true;
		}
		if (GUILayout.Button("-Y")) // TODO: Make Green
		{
			start = target.transform.position;
			end = start - Vector3.up * 2.0f;
			vectorHasChanged = true;
		}
		if (GUILayout.Button("-Z")) // TODO: Make Blue
		{
			start = target.transform.position;
			end = start - Vector3.forward * 2.0f;
			vectorHasChanged = true;
		}
		GUILayout.EndHorizontal();

		target.transform.rotation = Quaternion.AngleAxis(angle, (end - start).normalized);
	}

	void OnGUI()
	{
		var textureSize = 64.0f; // iconTexture.width;
		Vector3 screenPos  = mainCamera.WorldToScreenPoint(transform.position);
		var x = screenPos.x - textureSize / 2.0f;
		var y = screenPos.y - textureSize / 2.0f;

		GUI.Label(new Rect(x, y, textureSize, textureSize), iconTexture);

		windowRect = GUILayout.Window(myID, windowRect, GUIControlWindow, "Quaternion " + myID);
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawIcon(transform.position, "Quaternion Rotation.png");
	}

	// OnMouseDown is called when the user has pressed
	// the mouse button while over the GUIElement or Collider.
	void OnMouseDown()
	{
		if (!Input.GetKey(KeyCode.LeftAlt))
		{
			drawVector = true;
			// Calculate the distance from the camera to this position.
			var distance = Vector3.Distance(mainCamera.transform.position, transform.position);
			var mousePos = Input.mousePosition;
			var startPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
			SetStartPoint(startPoint);
		}
	}

	void OnMouseUp()
	{
		drawVector = false;
	}

	// OnMouseDrag is called when the user has clicked
	// on a GUIElement or Collider and is still holding down the mouse.
	void OnMouseDrag()
	{
		if (drawVector)
		{
			var distance = Vector3.Distance(mainCamera.transform.position, transform.position);
			var mousePos = Input.mousePosition;
			var endPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
			SetEndPoint(endPoint);
		}
	}

	void SetStartPoint(Vector3 startPoint  )
	{
		start = startPoint;
		vectorHasChanged = true;
	}

	public void SetEndPoint(Vector3 endPoint  )
	{
		end = endPoint;
		vectorHasChanged = true;
	}
}
