using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class SCCommunicator : MonoBehaviour {
	
	private struct ReceivedData{
		public int hostId;
		public int connectionId;
		public int channelId;
		public byte[] buffer;
		public int bufferSize;
		public byte error;
		public string message;
		public string command;
		public ReceivedData(int hostId, int connectionId, int channelId, byte[] buffer, int bufferSize, byte error, string message, string command){
			this.hostId = hostId;
			this.connectionId = connectionId;
			this.channelId = channelId;
			this.buffer = buffer;
			this.bufferSize = bufferSize;
			this.error = error;
			this.message = message;
			this.command = command;
		}
	};

	public const int MAX_CONNECTIONS = 100;
	private const int PORT = 2464;

	private int mHostId;
	private int mReliableChannelId;

	private SCBrain brain;
	public List<Action<float>> updater;

	void Start(){
		init();

		brain = new SCBrain(this);
		updater = new List<Action<float>>();
	}
	
	private void init(){
		NetworkTransport.Init();
		
		ConnectionConfig config = new ConnectionConfig();
		mReliableChannelId = config.AddChannel(QosType.Reliable);
		
		HostTopology topology = new HostTopology(config, MAX_CONNECTIONS);
		mHostId = NetworkTransport.AddHost(topology, PORT);
	}
	
	void Update(){
		processInput();
		processUpdater();

		int hostId;
		int connectionId;
		int channelId;
		byte[] buffer = new byte[1024];
		int bufferSize = 1024;
		int recBufferSize;
		byte error;
		
		NetworkEventType rec = NetworkTransport.Receive(out hostId, out connectionId, out channelId, buffer, bufferSize, out recBufferSize, out error);
		
		ReceivedData data = new ReceivedData(hostId, connectionId, channelId, buffer, recBufferSize, error, null, null);
		
		switch(rec){
		case NetworkEventType.Nothing: break;
		case NetworkEventType.ConnectEvent: onConnectEvent(ref data); break;
		case NetworkEventType.DataEvent: onDataEvent(ref data); break;
		case NetworkEventType.DisconnectEvent: onDisconnectEvent(ref data); break;
		}
	}

	private void processInput(){
		if(Input.GetKeyDown("s")){
			brain.createSampleGame();
		}
	}

	private void processUpdater(){
		for(int i = 0; i < updater.Count; ++i){
			updater[i](Time.deltaTime);
		}
	}
	
	private void onConnectEvent(ref ReceivedData data){
		Debug.Log("SCCommunicator| Incoming connection with Id: " + data.connectionId);
		sendMessageTo(data.connectionId, "waiting");
	}
	
	private void onDataEvent(ref ReceivedData data){
		if(!(mHostId == data.hostId)){
			return;
		}else{
			string message = SCNetworkUtil.getStringFromBuffer(data.buffer);
			string command = SCNetworkUtil.getCommand(message);
			SCMessageInfo info = SCNetworkUtil.decodeMessage(message);
			info.fromConnectionId = data.connectionId;
			Debug.Log("SCCommunicator| Received: \"" + message + "\" from connection Id: " + data.connectionId);
			brain.processMessage(command, info);
		}
	}

	private void onDisconnectEvent(ref ReceivedData data){
		Debug.Log("SCCommunicator| Disconnected from connection Id: " + data.connectionId);
		brain.processDisconnection(data.connectionId);
	}
	
	public void sendMessageTo(int connectionId, string message){
		Debug.Log("SCCommunicator| Sent message: \"" + message + "\" to connection Id: " + connectionId);
		SCNetworkUtil.sendMessage(mHostId, connectionId, mReliableChannelId, message);
	}

	public void disconnectFrom(int connectionId){
		byte error;
		NetworkTransport.Disconnect(mHostId, connectionId, out error);
	}

	public SCConnectionInfo getConnectionInfo(int connectionId){
		return SCNetworkUtil.getConnectionInfo(mHostId, connectionId);
	}
}
