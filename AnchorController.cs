using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AnchorController : MonoBehaviour, Interactable, INetworkInteractable
{
	
	public Anchor Anchor;
	public AnchorPosition AnchorLocation;
	private BoatController _boatController;
	private Animator _animator;
	private float _dropDuration = 3.5f;
	private NetworkIdentity _identity;
	[SerializeField] private List<GameObject> _interactingPlayers = new List<GameObject>();
	[SerializeField] private List<GameObject> _interactingLocations = new List<GameObject>();
	[SerializeField] private float _secondsNeededToPullAnchor = 11;
	

	void Start ()
	{
		_identity = GetComponent<NetworkIdentity> ();
		_animator = transform.GetComponentInChildren<Animator>();
		_boatController = transform.GetComponentInParent<BoatController>();
	}

	void FixedUpdate () 
	{
		if (_interactingPlayers.Count > 0)
		{
			Pull();
		}
		else if (Anchor.PulledUpTime > -1)
		{
			ReleaseAfterPull();
		}
	}

	public string MessageToDisplay()
	{
		if (Anchor.DroppedTime + _dropDuration > Time.time)
		{
			return string.Empty;
		}
		else if(Anchor.PulledUpTime > float.Epsilon && _interactingPlayers.Count == 0)
		{
			return string.Empty;
		}
		else if (Anchor.IsDropped())
		{
			return "Return the " + AnchorLocation + " anchor";
		}
		else
		{
			return "Release the " + AnchorLocation +  " anchor";
		}
	}

	public void Interact(GameObject player)
	{
		if (Anchor.DroppedTime + _dropDuration < Time.time) 
		{
			player.GetComponent<InteractionNetwork> ().CmdInteract (_identity, player);
			ApplyInteractionLogic (player);
		}
	}
	private void ApplyInteractionLogic(GameObject player)
	{
		if (Anchor.IsDropped()) 
		{
			StartPulling (player);
		}
		else 
		{
			DropAnchor ();
		}
	}

	public void HoldInteract(GameObject player)
	{
		//Do nothing
	}

	private void StartPulling(GameObject player)
	{
		_interactingPlayers.Add(player);
		_animator.SetBool("Pulling", true);
		_animator.speed = _interactingPlayers.Count;

		if (player.GetComponent<NetworkIdentity> ().hasAuthority && player.GetComponent<InteractRaycast>().enabled == true) 
		{
			player.GetComponent<InteractRaycast> ().StartInteracting ();
			Transform newParent = _interactingLocations [_interactingPlayers.Count - 1].transform;

			player.transform.parent = newParent;
			player.transform.localEulerAngles = new Vector3 (0, 0, -90);
			player.transform.localPosition = Vector3.zero;
		}
	}

	public void StopInteracting(GameObject player)
	{
		if (player.GetComponent<NetworkIdentity>().hasAuthority)
		{
			player.GetComponent<InteractionNetwork>().CmdStopInteract(_identity, player);
		}
		StopInteractingLogic (player);
	}

	private void StopInteractingLogic(GameObject player)
	{
		RemovePullingPlayer(player);
	}

	private void Pull()
	{
		for (int i = 0; i < _interactingPlayers.Count; i++)
		{
			Anchor.PulledUpTime += Time.deltaTime;
		}

		if (Anchor.PulledUpTime > _secondsNeededToPullAnchor)
		{
			FinishRaiseAnchor();
		}
	}

	private void FinishRaiseAnchor()
	{
		Anchor.PulledUpTime = -1;
		Anchor.gameObject.SetActive(true);
		Anchor.SetAnchorIsDropped(false);
		_animator.SetBool("Pulling", false);
		_animator.SetTrigger("StopCurrent");

		foreach (GameObject player in _interactingPlayers)
		{
			if (player.GetComponent<NetworkIdentity>().hasAuthority)
			{
				player.GetComponent<InteractRaycast>().ResetInteracting();
			}
		}
		_interactingPlayers.Clear();
	}

	private void ReleaseAfterPull()
	{
		if (Anchor.PulledUpTime < float.Epsilon)
		{
			Anchor.PulledUpTime = -1;
			_animator.SetTrigger("StopCurrent");
		}
		else
		{
			Anchor.PulledUpTime -= Time.deltaTime * 3;
		}
	}

	public void RemovePullingPlayer(GameObject player)
	{
		_interactingPlayers.Remove(player);

		if (_interactingPlayers.Count == 0)
		{
			_animator.speed = 1;
			_animator.SetBool("Pulling", false);
		}
		else
		{
			_animator.speed = _interactingPlayers.Count;
		}
	}

	public void DropAnchor()
	{
		if(!Anchor.IsDropped())
		{
			Anchor.DroppedTime = Time.time;
			Anchor.SetAnchorIsDropped(true);
			Anchor.gameObject.SetActive(false);
			_animator.SetTrigger("Release");
		}
	}


	public void NetworkInteract(GameObject player)
	{
		ApplyInteractionLogic (player);
	}

	public void NetworkStopInteract(GameObject player)
	{
		StopInteractingLogic(player);
	}

	public void NetworkUseInteract (GameObject player, Vector3 amount){}
}
