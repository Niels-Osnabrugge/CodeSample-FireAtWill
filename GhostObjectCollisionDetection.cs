using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostObjectCollisionDetection : MonoBehaviour {

	public Material GreenMaterial;
	public Transform IsGroundedRaycastObject;
	public Material RedMaterial;
	private MeshRenderer[] _meshes;
	private List<GameObject> _obstructedObjects = new List<GameObject>();
	private bool isGrounded;
	public bool canPlace;

	void Start()
	{
		_meshes = this.transform.GetComponentsInChildren<MeshRenderer> ();
	}

	void Update()
	{
		CheckIfGrounded ();
	}

	void OnTriggerEnter(Collider collision)
	{
		_obstructedObjects.Add (collision.transform.gameObject);
		ChangeCollisionColors ();
	}

	void OnTriggerExit(Collider collision)
	{
		_obstructedObjects.Remove(collision.transform.gameObject);
		ChangeCollisionColors ();
	}

	public bool HasCollision()
	{
		return _obstructedObjects.Count > 0;
	}

	private void ChangeCollisionColors()
	{
		if (HasCollision () || !isGrounded) 
		{
			foreach (MeshRenderer mesh in _meshes) 
			{
				mesh.material = RedMaterial;
			}
			canPlace = false;
		} 
		else 
		{
			foreach (MeshRenderer mesh in _meshes) 
			{
				mesh.material = GreenMaterial;
			}
		}
	}

	private void CheckIfGrounded()
	{
		Vector3 down = Vector3.down;
		RaycastHit hit;
		if (Physics.Raycast (IsGroundedRaycastObject.position, down, out hit, .7f, 1 << LayerMask.NameToLayer ("Default"))) 
		{
			if (!isGrounded) 
			{
				isGrounded = true;
				ChangeCollisionColors ();
			}
		} 
		else 
		{
			if (isGrounded) 
			{
				isGrounded = false;
				ChangeCollisionColors ();
			}
		}
	}
}
