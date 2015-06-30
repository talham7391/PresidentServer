using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using System.Collections;
using System.Text;

public class SCMainServer{

	private const int PORT = 2461;

	private int hostId;
	private int reliableChannel;

	public SCMainServer(){

		NetworkTransport.Init();

		ConnectionConfig config = new ConnectionConfig();
		reliableChannel = config.AddChannel(QosType.Reliable);

		HostTopology topology = new HostTopology(config, 6);
		hostId = NetworkTransport.AddHost(topology, PORT);

		Debug.Log("Seems to have been created.");
	}

	public void checkForConnections(){
		int recHostId;
		int connectionId;
		int channelId;
		byte[] recBuffer = new byte[1024];
		int bufferSize = 1024;
		int dataSize;
		byte error;
		NetworkEventType recData = NetworkTransport.Receive(out recHostId,
		                                                    out connectionId,
		                                                    out channelId,
		                                                    recBuffer,
		                                                    bufferSize,
		                                                    out dataSize,
		                                                    out error);
		switch(recData){
		case NetworkEventType.Nothing:
			break;
		case NetworkEventType.ConnectEvent:
			Debug.Log("Someone Connected!");
			if(recHostId == hostId){
				Debug.Log("Trying to connect back...");
				string address;
				int port;
				NetworkID network;
				NodeID dstNode;
				byte lError;
				NetworkTransport.GetConnectionInfo(hostId,
				                                   connectionId,
				                                   out address,
				                                   out port,
				                                   out network,
				                                   out dstNode,
				                                   out error);
				byte fError;
				NetworkTransport.Connect(hostId, address, port, 0, out fError);
				Debug.Log("Connection Request Sent.");
			}
			break;
		case NetworkEventType.DataEvent:
			Debug.Log("Someone sent a message:");
			string message = shortenString(Encoding.UTF8.GetString(recBuffer));
			if(message == "hello"){
				Debug.Log("Inteded message recieved!");
			}
			break;
		}
	}

	private string shortenString(string target){
		int stringLength = target.Length;
		for(int i = 0; i < stringLength; ++i){
			if(target[i] == '\0'){
				return target.Substring(0, i);
			}
		}
		return target;
	}
}
