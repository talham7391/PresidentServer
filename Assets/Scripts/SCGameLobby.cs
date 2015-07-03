using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class SCGameLobby{

	private struct Player{
		public string address;
		public int port;
		public Player(string address, int port){
			this.address = address;
			this.port = port;
		}
	};

	string mHostIp;
	int mNumberOfPlayers;
	string mGameName;

	private List<Player> players;

	public SCGameLobby(string hostIp, int numberOfPlayers, string gameName){
		mHostIp = hostIp;
		mNumberOfPlayers = numberOfPlayers;
		mGameName = gameName;
		players = new List<Player>();
	}

	public void addPlayer(string playerAddress, int playerPort){
		players.Add(new Player(playerAddress, playerPort));
		if(players.Count == mNumberOfPlayers){
			initConnectionTransfer();
		}
	}

	private void initConnectionTransfer(){
		// send all players info for the local server
		// send a disconnect request to all the players
		// have the players connect to the local server
	}

	public string getGameName(){
		return mGameName;
	}
}
