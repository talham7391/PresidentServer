using UnityEngine;
using System.Collections;

public class SCSource : MonoBehaviour {

	private bool serverCreated = false;
	SCMainServer server;

	void Update(){
		if(Input.GetKeyDown("c")){
			createMainServer();
		}
	}

	private void createMainServer(){
		if(serverCreated){
			Debug.Log("Main server already created.");
			return;
		}
		serverCreated = true;
		Debug.Log("Created the main server.");

		server = gameObject.AddComponent<SCMainServer>();
	}
}
