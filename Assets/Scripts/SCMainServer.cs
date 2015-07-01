using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class SCMainServer : MonoBehaviour{

	private struct ReceivedData{
		public int hostId;
		public int connectionId;
		public int channelId;
		public byte[] buffer;
		public int bufferSize;
		public byte error;
	};

	private const int PORT = 2462;

	private int mHostId;
	private int mReliableConnectionId;

	private List<int> currentConnectionIds;

	void Start(){

		currentConnectionIds = new List<int>();

		NetworkTransport.Init();

		ConnectionConfig config = new ConnectionConfig();
		mReliableConnectionId = config.AddChannel(QosType.Reliable);
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

		switch(rec){
		case NetworkEventType.Nothing: break;
		case NetworkEventType.ConnectEvent: onConnectEvent(ref data); break;
		case NetworkEventType.DataEvent: onDataEvent(ref data); break;
		}
	}

	private void onConnectEvent(ref ReceivedData data){
		Debug.Log("Someone is trying to connect...");
		connectBack(ref data);
	}

	private void onDataEvent(ref ReceivedData data){

	}

	private void connectBack(ref ReceivedData data){
		string address;
		int port;
		NetworkID networkId;
		NodeID nodeId;
		byte error;
		NetworkTransport.GetConnectionInfo(data.hostId, data.connectionId, out address, out port, out networkId, out nodeId, out error);
		byte cError;
		currentConnectionIds.Add(NetworkTransport.Connect(mHostId, address, port, 0, out cError));
		Debug.Log("Successfully connected.");
	}
}
