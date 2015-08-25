using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCBrain{

	private SCCommunicator communicator;
	private SCLogic logic;

	List<SCGameInfo> games;
	List<SCCommandBehaviour> commandBehaviours;

	public SCBrain(SCCommunicator communicator){
		Debug.Log("SCBrain| Master server started.");
		this.communicator = communicator;
		logic = new SCLogic();

		games = new List<SCGameInfo>();
		commandBehaviours = new List<SCCommandBehaviour>();

		addCommandBehaviours();
	}

	private void addCommandBehaviours(){
		commandBehaviours.Add(new SCCommandBehaviour("first_time", onFirstTimeCommand));
		commandBehaviours.Add(new SCCommandBehaviour("reconnecting", onReconnectingCommand));

		commandBehaviours.Add(new SCCommandBehaviour("create_game", onCreateGameCommand));
		commandBehaviours.Add(new SCCommandBehaviour("update_game", onUpdateGameCommand));
		commandBehaviours.Add(new SCCommandBehaviour("forget_game", onForgetGameCommand));
		commandBehaviours.Add(new SCCommandBehaviour("request_game", onRequestGameCommand));
	}

	public void processMessage(string command, SCMessageInfo info){
		for(int i = 0; i < commandBehaviours.Count; ++i){
			if(commandBehaviours[i].command == command){
				commandBehaviours[i].executeCallback(info);
			}
		}
	}

	public void processReconnection(int uniqueId, int connectionId){
		SCGameInfo game = SCGameInfo.getGameCreatedByUniqueId(games, uniqueId);
		if(game == null){
			return;
		}

		Debug.Log("SCBrain| Game status updated to \"Connected\" created by user: " + game.createdByUser);
		game.createdByConnectionId = connectionId;
		game.userDisconnected = false;
	}

	public void processDisconnection(int connectionId){
		SCGameInfo game = SCGameInfo.getGameCreatedByConnectionId(games, connectionId);
		if(game == null){
			return;
		}

		Debug.Log("SCBrain| Game status updated to \"Disconnected\" created by user: " + game.createdByUser);
		game.userDisconnected = true;
	}

	/********************************************************************************************/
	/** Command Functions ***********************************************************************/
	/********************************************************************************************/

	private void onFirstTimeCommand(SCMessageInfo info){
		communicator.sendMessageTo(info.fromConnectionId, "connected:uniqueId=" + logic.generateUniqueId());
	}

	private void onReconnectingCommand(SCMessageInfo info){
		string uniqueId = info.getValue("uniqueId");
		if(uniqueId == null){
			return;
		}
		processReconnection(SCNetworkUtil.toInt(uniqueId), info.fromConnectionId);
	}

	private void onCreateGameCommand(SCMessageInfo info){
		string user = info.getValue("user");
		string uniqueId = info.getValue("uniqueId");
		string hasPassword = info.getValue("pass");
		string totalPlayers = info.getValue("total");
		if(user == null || uniqueId == null || hasPassword == null || totalPlayers == null){
			return;
		}
		Debug.Log("SCBrain| New game created by user: " + user);
		games.Add(new SCGameInfo(user,
		                         info.fromConnectionId,
		                         SCNetworkUtil.toInt(uniqueId),
		                         (hasPassword == "true" ? true : false),
		                         SCNetworkUtil.toInt(totalPlayers),
		                         communicator.getConnectionInfo(info.fromConnectionId)));
	}

	private void onUpdateGameCommand(SCMessageInfo info){
		string players = info.getValue("players");
		if(players == null){
			return;
		}

		SCGameInfo game = SCGameInfo.getGameCreatedByConnectionId(games, info.fromConnectionId);
		int iPlayers = SCNetworkUtil.toInt(players);
		if(iPlayers == game.totalNumberOfPlayers){
			forget(game);
		}else{
			Debug.Log("SCBrain| Players updated to " + iPlayers + " for game created by user: " + game.createdByUser);
			game.numberOfConnectedPlayers = iPlayers;
		}
	}

	private void onForgetGameCommand(SCMessageInfo info){
		forget(SCGameInfo.getGameCreatedByConnectionId(games, info.fromConnectionId));
	}

	private void onRequestGameCommand(SCMessageInfo info){
		string user = info.getValue("user");
		if(user == null){
			return;
		}
		
		SCGameInfo game = SCGameInfo.getGameCreatedByUser(games, user);
		if(game == null){
			communicator.sendMessageTo(info.fromConnectionId, "request_game_info:error=game_not_found");
			return;
		}
		Debug.Log("SCBrain| User requested game created by: " + game.createdByUser);
		communicator.sendMessageTo(info.fromConnectionId, "request_game_info:" + 
		                           						  "user=" + game.createdByUser + "," +
		                           						  "pass=" + (game.hasPassword ? "true" : "false") + "," +
		                           						  "players=" + game.numberOfConnectedPlayers + "," +
		                           						  "ip=" + game.connectionInfo.getIp() + "," +
		                           						  "port=" + game.connectionInfo.getPort());
	}

	/********************************************************************************************/
	/** Util Functions **************************************************************************/
	/********************************************************************************************/

	private void forget(SCGameInfo game){
		Debug.Log("SCBrain| Forgot game created by user: " + game.createdByUser);
		communicator.disconnectFrom(game.createdByConnectionId);
		logic.freeUniqueId(game.createdByUniqueId);
		games.Remove(game);
	}

	public void createSampleGame(){
		Debug.Log("SCBrain| Created a sample game");
		games.Add(new SCGameInfo("billyha", 40, 500, false, 5, new SCConnectionInfo("random haha", 400003)));
	}

	/********************************************************************************************/
	/** Bridge Functions ************************************************************************/
	/********************************************************************************************/

	public int generateUniqueId(){
		return logic.generateUniqueId();
	}
}
