using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class SCMainServer : MonoBehaviour{

	private struct ReceivedData{
		public int hostId;
		public int connectionId;
		public int channelId;
		public byte[] buffer;
		public int bufferSize;
		public byte error;
		public string message;
		public string command;
	};

	private struct CommandBehaviour{
		public string command;
		public Action<ReceivedData> callBack;
		public CommandBehaviour(string command, Action<ReceivedData> callBack){
			this.command = command;
			this.callBack = callBack;
		}
	};

	private struct ConnectionInfo{
		public string address;
		public int port;
		public ConnectionInfo(string address, int port){
			this.address = address;
			this.port = port;
		}
	};

	private const int PORT = 2462;

	private int mHostId;
	private int mReliableChannelId;

	private List<int> currentConnectionIds;
	private List<CommandBehaviour> commandBehaviours;
	private List<SCGameLobby> lobbies;

	void Start(){

		currentConnectionIds = new List<int>();
		commandBehaviours = new List<CommandBehaviour>();
		lobbies = new List<SCGameLobby>();

		addCommandBehaviours();

		NetworkTransport.Init();

		ConnectionConfig config = new ConnectionConfig();
		mReliableChannelId = config.AddChannel(QosType.Reliable);
		HostTopology topology = new HostTopology(config, 100);

		mHostId = NetworkTransport.AddHost(topology, PORT);
	}

	void Update(){
		int hostId;
		int connectionId;
		int channelId;
		byte[] buffer = new byte[1024];
		int bufferSize = 1024;
		int recBufferSize;
		byte error;
		NetworkEventType rec = NetworkTransport.Receive(out hostId, out connectionId, out channelId, buffer, bufferSize, out recBufferSize, out error);

		ReceivedData data = new ReceivedData();
		data.hostId = hostId;
		data.connectionId = connectionId;
		data.channelId = channelId;
		data.buffer = buffer;
		data.bufferSize = recBufferSize;
		data.error = error;
		data.message = null;
		data.command = null;

		switch(rec){
		case NetworkEventType.Nothing: break;
		case NetworkEventType.ConnectEvent: onConnectEvent(ref data); break;
		case NetworkEventType.DataEvent: onDataEvent(ref data); break;
		}
	}

	//*******************************************************************************
	// Event Functions
	//*******************************************************************************

	private void onConnectEvent(ref ReceivedData data){
		Debug.Log("Someone is trying to connect...");
		connectBack(ref data);
	}

	private void onDataEvent(ref ReceivedData data){
		bool functionCalled = false;
		string message = SCNetworkUtil.getStringFromBuffer(data.buffer);
		string command = SCNetworkUtil.getCommand(message);
		for(int i = 0; i < commandBehaviours.Count; ++i){
			if(command == commandBehaviours[i].command){
				data.message = message;
				data.command = command;
				commandBehaviours[i].callBack(data);
				functionCalled = true;
			}
		}
		if(!functionCalled){
			Debug.Log("Received: " + message);
		}
	}

	//*******************************************************************************
	// Helper Functions
	//*******************************************************************************

	private void addCommandBehaviours(){
		addCommandBehaviour("create_new_game", onNewGameRequest);
		addCommandBehaviour("connect_to_game", onConnectToGameRequest);
	}

	//*******************************************************************************
	// Util Functions
	//*******************************************************************************

	private void addCommandBehaviour(string command, Action<ReceivedData> callBack){
		CommandBehaviour messageBehaviour = new CommandBehaviour(command, callBack);
		commandBehaviours.Add(messageBehaviour);
	}

	private void sendMessage(string message, int connectionId){
		SCNetworkUtil.sendMessage(mHostId, connectionId, mReliableChannelId, message);
	}

	private ConnectionInfo getConnectionInfo(ref ReceivedData data){
		string address;
		int port;
		NetworkID networkId;
		NodeID nodeId;
		byte error;
		NetworkTransport.GetConnectionInfo(data.hostId, data.connectionId, out address, out port, out networkId, out nodeId, out error);
		ConnectionInfo info = new ConnectionInfo(address, port);
		return info;
	}

	//*******************************************************************************
	// Response Functions
	//*******************************************************************************

	private void connectBack(ref ReceivedData data){
		for(int i = 0; i < currentConnectionIds.Count; ++i){
			if(data.connectionId == currentConnectionIds[i]){
				Debug.Log("This person is already connected.");
				return;
			}
		}
		ConnectionInfo info = getConnectionInfo(ref data);
		byte cError;
		currentConnectionIds.Add(NetworkTransport.Connect(mHostId, info.address, info.port, 0, out cError));
		Debug.Log("Successfully connected.");
	}

	private void onNewGameRequest(ReceivedData data){
		Debug.Log("Someone has requested a new game...");
		ConnectionInfo connectionInfo = getConnectionInfo(ref data);
		SCMessageInfo messageInfo = SCNetworkUtil.decodeMessage(data.message);
		if(messageInfo == null){
			sendMessage("Game not created: no info", data.connectionId);
			Debug.Log("Game not created.");
			return;
		}
		int numberOfPlayers = SCNetworkUtil.toInt(messageInfo.getValue("number_of_players"));
		if(numberOfPlayers <= 0){
			sendMessage("Game not created: Invalid number of players.", data.connectionId);
			Debug.Log("Game not created.");
			return;
		}
		string gameName = messageInfo.getValue("game_name");
		if(gameName == null){
			sendMessage("Game not created: Invalid game name.", data.connectionId);
			Debug.Log("Game not created.");
			return;
		}
		SCGameLobby lobby = new SCGameLobby(connectionInfo.address, numberOfPlayers, messageInfo.getValue("game_name"));
		lobbies.Add(lobby);
		sendMessage("new_game_created", data.connectionId);
		sendMessage("Waiting for others to join...", data.connectionId);
		Debug.Log("New game created.");
	}

	private void onConnectToGameRequest(ReceivedData data){
		ConnectionInfo connectionInfo = getConnectionInfo(ref data);
		SCMessageInfo messageInfo = SCNetworkUtil.decodeMessage(data.message);
		string gameName = messageInfo.getValue("game_name");
		for(int i = 0; i < lobbies.Count; ++i){
			if(gameName == lobbies[i].getGameName()){
				lobbies[i].addPlayer(connectionInfo.address, connectionInfo.port);
				sendMessage("Added to lobby.", data.connectionId);
			}
		}
		sendMessage("This game has not been created.", data.connectionId);
		return;
	}
}
