using UnityEngine;
using System.Collections;

public class SCSource : MonoBehaviour {

	SCMainServer mainServer;
	bool mainServerCreated;

	void Start(){
		mainServer = null;
		mainServerCreated = false;
		createMainServer();
	}

	void Update(){
		mainServer.checkForConnections();
	}

	private void createMainServer(){
		if(mainServerCreated){
			Debug.Log("Main server has already been created");
			return;
		}
		mainServerCreated = true;

		mainServer = new SCMainServer();
	}
}
