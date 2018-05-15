using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPickup : MonoBehaviour {

	public float SecondsNeededToPickup;
	public float MaxPickupRange;
	private float _holdPickupTime;
	private Camera _camera;
	private GameObject _objectToPickup;
	private GameObject _pickedupObject;
	private GameObject _ghostObject;
	private float _userRotation;
	private GhostObjectCollisionDetection _ghostCollision;

	void Start()
	{
		_camera = this.GetComponent<Camera> ();
	}

	void Update()
	{
		if (_pickedupObject == null) 
		{
			if (CheckIfPickupObject ()) 
			{
				PickupObject ();
				_holdPickupTime = 0;
			}
		} 
		else 
		{
			CheckIfRotateObject ();
			if (CheckIfPlaceObject ()) 
			{
				PlaceObject ();
			}
		}
	}

	private bool CheckIfPickupObject()
	{
		if (Input.GetButton ("PickupObject")) 
		{
			RaycastHit hit;
			if (Physics.Raycast (transform.position, _camera.transform.forward, out hit, MaxPickupRange, LayerMask.NameToLayer("Player"))) 
			{
				if (hit.transform.gameObject.GetComponent<PlaceableObject>() != null) 
				{
					if (hit.transform.gameObject == _objectToPickup) 
					{
						_holdPickupTime += Time.deltaTime;
						if (_holdPickupTime > SecondsNeededToPickup) 
						{
							return true;
						} 
						else 
						{
							return false;
						}
					} 
					else 
					{
						_objectToPickup = hit.transform.gameObject;
					}
				}
			}
		}
		_holdPickupTime = 0;
		return false;
	}

	private void PlaceObject()
	{
		_pickedupObject.transform.position = _ghostObject.transform.position;
		_pickedupObject.transform.rotation = _ghostObject.transform.rotation;
		Destroy (_ghostObject);
		_pickedupObject.SetActive (true);
		_pickedupObject = null;
	}

	private bool CheckIfPlaceObject()
	{
		RaycastHit hit;
		if (Physics.Raycast (transform.position, _camera.transform.forward, out hit, MaxPickupRange, 1 << LayerMask.NameToLayer ("Default"))) 
		{
			_ghostObject.transform.position = hit.point;

				_ghostObject.transform.rotation = Quaternion.FromToRotation (transform.up, hit.normal) * transform.rotation;
			_ghostObject.transform.Rotate (0, _userRotation, 0);
			if (!_ghostCollision.HasCollision ()) 
			{
				if (Input.GetButton ("PlaceObject")) 
				{
					return true;
				}
			}
		}
		return false;
	}

	private void CheckIfRotateObject()
	{
		if (Input.GetKey(KeyCode.E)) 
		{
			_userRotation++;
		}
		if (Input.GetKey (KeyCode.Q)) 
		{
			_userRotation--;
		}
	}

	private void PickupObject()
	{
		_objectToPickup.SetActive (false);
		_pickedupObject = _objectToPickup;
		_ghostObject = _pickedupObject.GetComponent<PlaceableObject> ().GhostObject;
		_ghostObject = Instantiate (_ghostObject, Vector3.zero, _ghostObject.transform.rotation, null);
		_ghostCollision = _ghostObject.GetComponent<GhostObjectCollisionDetection> ();
	}
}
