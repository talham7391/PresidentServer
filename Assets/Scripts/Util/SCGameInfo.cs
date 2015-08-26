using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SCGameInfo{

	public const int TIME_OUT = 2;

	private string mCreatedByUser;
	private int mCreatedByConnectionId;
	private int mCreatedByUniqueId;
	private bool mUserDisconnected;
	private bool mHasPassword;
	private int mNumberOfConnectedPlayers;
	private int mTotalNumberOfPlayers;
	private SCConnectionInfo mConnectionInfo;
	private Action<SCGameInfo> mOnTimeoutCallback;

	private float timeSinceDisconnect;

	public SCGameInfo(string createdByUser, int createdByConnectionId, int createdByUniqueId, bool hasPassword, int totalNumberOfPlayers, SCConnectionInfo connectionInfo, Action<SCGameInfo> onTimeoutCallback){
		mCreatedByUser = createdByUser;
		mCreatedByConnectionId = createdByConnectionId;
		mCreatedByUniqueId = createdByUniqueId;
		mUserDisconnected = false;
		mHasPassword = hasPassword;
		mNumberOfConnectedPlayers = 0;
		mTotalNumberOfPlayers = totalNumberOfPlayers;
		mConnectionInfo = connectionInfo;
		timeSinceDisconnect = 0;
		mOnTimeoutCallback = onTimeoutCallback;
	}

	public void update(float deltaTime){
		timeSinceDisconnect += deltaTime;
		if(timeSinceDisconnect >= TIME_OUT){
			if(mOnTimeoutCallback != null){
				mOnTimeoutCallback(this);
				mOnTimeoutCallback = null;
			}
		}
	}

	public void reset(){
		timeSinceDisconnect = 0;
	}

	public string createdByUser{
		get{
			return mCreatedByUser;
		}
	}

	public int createdByConnectionId{
		get{
			return mCreatedByConnectionId;
		}
		set{
			mCreatedByConnectionId = value;
		}
	}

	public int createdByUniqueId{
		get{
			return mCreatedByUniqueId;
		}
	}

	public bool userDisconnected{
		get{
			return mUserDisconnected;
		}
		set{
			mUserDisconnected = value;
		}
	}

	public bool hasPassword{
		get{
			return mHasPassword;
		}
	}

	public int numberOfConnectedPlayers{
		get{
			return mNumberOfConnectedPlayers;
		}
		set{
			mNumberOfConnectedPlayers = value;
		}
	}

	public int totalNumberOfPlayers{
		get{
			return mTotalNumberOfPlayers;
		}
	}

	public SCConnectionInfo connectionInfo{
		get{
			return mConnectionInfo;
		}
	}

	/********************************************************************************************/
	/** Static Util Functions *******************************************************************/
	/********************************************************************************************/

	public static SCGameInfo getGameCreatedByConnectionId(List<SCGameInfo> games, int connectionId){
		for(int i = 0; i < games.Count; ++i){
			if(games[i].createdByConnectionId == connectionId){
				return games[i];
			}
		}
		return null;
	}

	public static SCGameInfo getGameCreatedByUniqueId(List<SCGameInfo> games, int uniqueId){
		for(int i = 0; i < games.Count; ++i){
			if(games[i].createdByUniqueId == uniqueId){
				return games[i];
			}
		}
		return null;
	}

	public static SCGameInfo getGameCreatedByUser(List<SCGameInfo> games, string user){
		for(int i = 0; i < games.Count; ++i){
			if(games[i].createdByUser == user){
				return games[i];
			}
		}
		return null;
	}
}
