using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCLogic{

	List<int> generatedIds;

	public SCLogic(){
		generatedIds = new List<int>();
	}

	public int generateUniqueId(){
		if(generatedIds.Count >= SCCommunicator.MAX_CONNECTIONS * 2){
			return -1;
		}
	regen:
		int id = Random.Range(0, SCCommunicator.MAX_CONNECTIONS * 2);
		if(generatedIds.Contains(id)){
			goto regen;
		}
		generatedIds.Add(id);
		return id;
	}

	public void freeUniqueId(int uniqueId){
		generatedIds.Remove(uniqueId);
	}
}
