using UnityEngine;
using System.Collections;

public class SimpleMouseOrbit : MonoBehaviour {

	public Transform target;

	public float distance = 5f;

	public float xSpeed = 2f;
	public float ySpeed = 2f;
	private float xSpeedReal = 0f;
	private float ySpeedReal = 0f; 
	public float height = 2f;
	public float xAdjuster = 2f;

	float x = 0.0f;
	float y = 0.0f;
	public Quaternion rotation;

	// Use this for initialization
	void Start () 
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		xSpeedReal = xSpeed;
		ySpeedReal = ySpeed;

	}
	public void lockCamera()
    {
		xSpeedReal = 0f;
		ySpeedReal = 0f;
    }
	public void unLockCamera()
	{
		xSpeedReal = xSpeed;
		ySpeedReal = ySpeed;
	}

	public void SetTarget(Transform t) { target = t; }
	void Update () 
	{
        
		if (target) 
		{
			x += Input.GetAxis("Mouse X") * xSpeedReal * distance;
			y -= Input.GetAxis("Mouse Y") * ySpeedReal;
			rotation = Quaternion.Euler(y, x, 0);

			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position;
			position.y += height;



			transform.rotation = rotation;
			transform.position = position;
		}
	}
		
}