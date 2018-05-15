using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable{
	string MessageToDisplay();
	void Interact(GameObject player);
	void HoldInteract(GameObject player);

	void StopInteracting(GameObject player);
}
