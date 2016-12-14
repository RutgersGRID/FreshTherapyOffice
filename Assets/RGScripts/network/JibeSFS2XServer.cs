/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 *
 * JibeSmartFoxServer.cs Revision 1.3.1105.12
 * Provides Jibe implementation on SmartFox2X platform */

namespace ReactionGrid.Jibe
{
	using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Sfs2X;
    using Sfs2X.Core;
	using Sfs2X.Util;
    using Sfs2X.Entities;
    using Sfs2X.Entities.Data;
    using Sfs2X.Entities.Variables;
    using Sfs2X.Requests;
    using Sfs2X.Logging;

    public static class SmartFox2MessageType
    {
        public const string Transform = "t";          // "t" - player transform data
        public const string ForceSend = "f";          // "f" - means force our local player to send his transform
        public const string Animation = "a";          // "a" - for animation message received
        public const string VisualAppearance = "v";   // "v" - for visual appearance message
        public const string Name = "n";               // "n" - for name message
        public const string Speech = "s";             // "s" - for speech indicator message, user is speaking or has voice
        public const string Custom = "x";             // "x" - for custom messages (relay position of objects, network sync events, etc)
    }

    public class JibeSFS2XServer : JibeServerBase
    {
        private string _serverAddress;
        private int _serverPort;
        private string _zone;
        private string _defaultRoom;
        private string _roomPassword;
        private bool _dynamicRooms;
		private int _blueboxPort;
        private JibePlayer localPlayer;
        private JibeDebugLevel _debugLevel = JibeDebugLevel.ALL;
        private SmartFox smartFox;

        private string _dressingRoomName;
        private bool _roomAddAttempt;

        private bool isJoining = false;

        private bool _reconnectAttempt = false; // used by reconnection logic
        private string _reconnectCurrentRoom; // used by reconnection logic

        /// <summary>
        /// Default constructor for the SmartFox2X edition of the Jibe Server, has several required parameters
        /// </summary>
        /// <param name="ip">IP address of the server</param>
        /// <param name="port">The port to connect to on the server</param>
        /// <param name="zone">The Zone in SmartFox2X</param>
        /// <param name="defaultRoom">The default room - in a multiple room scenario, designating one room a default is handy in case you set up scenes and omit a specific room name.</param>
        /// <param name="roomPassword">The password used for the rooms - currently the server must be configured to have the same password for all rooms used per Jibe installation</param>
        /// <param name="dynamicRooms">Enable Dynamic Room support - requires specific web application configuration and is only for specific use cases</param>
        /// <param name="dispatchInterval">How often to process received network traffic - longer intervals will reduce network overhead at the expense of accuracy for avatar position updates in particular, though particularly long intervals will show a more noticable effect in chat lag and other updates.</param>
        /// <param name="sendInterval">How often to send updates over the network - longer intervals will reduce network overhead at the expense of accuracy for avatar position updates in particular, though particularly long intervals will show a more noticable effect in chat lag and other updates.</param>
        /// <param name="debugLevel">Controls level of verbosity in console window - useful for debugging purposes</param>
        /// <param name="debugDelegate">Delegate for regular debug messages</param>
        /// <param name="debugWarningDelegate">Delegate for warning level debug messages</param>
        /// <param name="debugErrorDelegate">Delegate for error level debug messages</param>
		/// <param name="blueBoxPort">Port to use for HTTP traffic, for firewall-heavy situations where regular TCP/UDP are blocked</param>
        public JibeSFS2XServer(string ip, int port, string zone, string defaultRoom, string roomPassword, bool dynamicRooms, int dispatchInterval, int sendInterval, JibeDebugLevel debugLevel, DebugOutputDelegate debugDelegate, DebugWarningOutputDelegate debugWarningDelegate, DebugErrorOutputDelegate debugErrorDelegate, int blueBoxPort)
            : base(dispatchInterval, sendInterval)
        {
            this.DebugListeners = debugDelegate;
            this.DebugWarningListeners = debugWarningDelegate;
            this.DebugErrorListeners = debugErrorDelegate;

            _serverAddress = ip;
            _serverPort = port;
            _zone = zone;
            _defaultRoom = defaultRoom;
            _roomPassword = roomPassword;
            _dynamicRooms = dynamicRooms;
            _debugLevel = debugLevel;
			_blueboxPort = blueBoxPort;

            this.localPlayer = new JibePlayer(-1);
            try
            {
                // SmartFox client API has boolean on or off for verbose logging to console. Only display full SmartFox debug messages if Jibe is set to Debug Level of ALL
                //smartFox = new SmartFox(debugLevel == JibeDebugLevel.ALL);
                //smartFox.ThreadSafeMode = true;
				#if UNITY_WEBGL
				_serverPort = 8888;
				Debug.Log("_serverPort: " + _serverPort);
				#endif

				// Initialize SFS2X client and add listeners
				// WebGL build uses a different constructor
				#if !UNITY_WEBGL
				smartFox = new SmartFox();
				#else
				smartFox = new SmartFox(UseWebSocket.WS);
				#endif

			}
            catch (Exception e)
            {
                this.DebugReturn(e.ToString());
            }
        }

        public override IJibePlayer Connect()
        {
            try
            {
                if (!smartFox.IsConnected)
                {
                    this.DebugReturn("Attempting to connect " + _serverAddress + ":" + _serverPort);
                   smartFox.Connect(_serverAddress, _serverPort);

                    if (!_reconnectAttempt)
                    {
                        SubscribeEvents();
                    }
                }
                else
                {
                    this.DebugReturn("Already connected");
                }
            }
            catch (Exception e)
            {
                this.DebugReturn(_debugLevel, "It all went horribly wrong (reconnecting smartfox) to " + _serverAddress + ":" + _serverPort + e.Message + e.StackTrace, JibeErrorLevel.ERROR);
                return null;
            }


            return this.localPlayer;
        }

        /// <summary>
        /// Only call this method when you need to clear out all references to remote players - likely this will only be called as part of a reconnection attempt
        /// </summary>
        private void ClearRemotePlayers()
        {
            foreach (IJibePlayer player in Players)
            {
                player.RaiseDisconnect();
            }
            _jibePlayers.Clear();
        }
        #region Jibe Server Common Methods - each supported network infrastructure will have to implement these methods


        public override void RequestLogin(string name)
        {
            this.DebugReturn(name + " attempting to join zone " + _zone);
            // if this name is already in use then this call may not succeed (but we only find out results in the SmartFox OnLogin event handler)
            smartFox.Send(new LoginRequest(name, "", _zone));
        }

        public override void JoinRoom(string roomName, string roomPassword)
        {
            this.DebugReturn("123: " + roomName);
            if (isJoining) return;

            isJoining = true;
            this.DebugReturn("Joining room: " + roomName);
            if (roomName.ToLower() == "dressingroom")
            {
                if (smartFox.LastJoinedRoom == null)
                {
                    this.DebugReturn("not in a room");
                    isJoining = false;
                    OnRoomJoinResult(new RoomJoinResultEventArgs(true, ""));
                }
                else
                {
                    this.DebugReturn("User entering Dressing Room from live scene");
                    // a hardcoded value but a constant concept:
                    if (string.IsNullOrEmpty(_dressingRoomName))
                    {
                        _roomAddAttempt = true;
                        _dressingRoomName = roomName + localPlayer.PlayerID;
                        RoomSettings newRoomSettings = new RoomSettings(_dressingRoomName);
                        this.DebugReturn("Creating " + _dressingRoomName);
                        smartFox.Send(new CreateRoomRequest(newRoomSettings));
                    }
                    else
                    {
                        //Reconnect to my dressing room
                        if (smartFox.LastJoinedRoom.Name != _dressingRoomName)
                        {
                            smartFox.Send(new JoinRoomRequest(_dressingRoomName, ""));
                        }
                    }
                }
            }
            else
            {
                // Need to leave current room, if we are joined one
                if (smartFox.LastJoinedRoom == null)
                {
                    this.DebugReturn("Sending join request");
                    smartFox.Send(new JoinRoomRequest(roomName, roomPassword));
                }
                else if (smartFox.LastJoinedRoom.Name == roomName)
                {
                    isJoining = false;
                    OnRoomJoinResult(new RoomJoinResultEventArgs(true, ""));
                }
                else
                {
                    // Leave last room and join new room
                    this.DebugReturn("leaving current room and joining new room");
                    smartFox.Send(new JoinRoomRequest(roomName, roomPassword, smartFox.LastJoinedRoom.Id));
                }
            }
        }

        public override void LeaveRoom()
        {
            Room currentRoom = smartFox.LastJoinedRoom;
            if (currentRoom != null)
            {
                this.DebugReturn("Leaving room " + currentRoom.Name);
                smartFox.RemoveJoinedRoom(currentRoom);
                _jibePlayers.Clear();
                // Ready to leave room, raise event so the next scene can be loaded
                OnRoomLeaveResult(new RoomLeaveResultEventArgs(true, ""));
            }
            else
            {
                // We're not in a room - consider this a success!
                OnRoomLeaveResult(new RoomLeaveResultEventArgs(true, ""));
            }
        }

        public override void Disconnect()
        {
            localPlayer.RaiseDisconnect();
            smartFox.Disconnect();
        }
        protected override void ProcessIncomingMessages()
        {
            // Process Incoming and Send Outgoing are called once per update loop from Network Controller
            smartFox.ProcessEvents();
        }
        protected override void SendOutgoingMessages()
        {
            // SFS2X has no discrete send and receive, so only need to call ProcessEventQueue once per update loop.
        }

        #endregion
        #region SmartFox Events - these are raised in response to various calls (to connect, to log in, etc.) and in response to server events (new players in scene, new chat)
        private void SubscribeEvents()
        {
            this.DebugReturn("subscribe events");
            // Register callback delegate
            smartFox.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            smartFox.AddEventListener(SFSEvent.LOGIN, OnLogin);
            smartFox.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
            smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
            smartFox.AddEventListener(SFSEvent.ROOM_ADD, OnAddRoom);
            smartFox.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnJoinRoomError);
            smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExit);
            smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
            smartFox.AddEventListener(SFSEvent.PRIVATE_MESSAGE, OnPrivateMessage);
            smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectReceived);
            smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
            smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreateError);
            smartFox.AddLogListener(LogLevel.DEBUG, OnDebugMessage);
        }
        private void UnregisterSFSSceneCallbacks()
        {
            // This should be called when switching scenes, so callbacks from the backend do not trigger code in this scene
            smartFox.RemoveAllEventListeners();
        }

        // Handle connection response from server
        public void OnConnection(BaseEvent evt)
        {
            bool success = (bool)evt.Params["success"];
            if (success)
            {
                this.DebugReturn("Connection successfull!");
                this._isConnected = true;
                // If this is a reconnection attempt, then re-join the previous zone
                if (_reconnectAttempt && !string.IsNullOrEmpty(_reconnectCurrentRoom))
                {
                    this.DebugReturn("Reconnecting to zone " + _zone);
                    this.RequestLogin(this.localPlayer.Name);
                }
                else if (_reconnectAttempt && string.IsNullOrEmpty(_reconnectCurrentRoom))
                {
                    this.DebugReturn("Not reconnecting to a zone - client has not yet logged in!");
                    _reconnectAttempt = false;
                }
            }
            else
            {
                this.DebugReturn("Can't connect!" + (string)evt.Params["errorMessage"]);
                this.DebugReturn("Bluebox may be a possibility - configured for port " + _blueboxPort + " BlueBox support is on the way in future editions!");
            }
        }

        public void OnLogin(BaseEvent evt)
        {
            try
            {
				if (evt.Params.Contains("success") && !(bool)evt.Params["success"])
                {
                    string error = (string)evt.Params["errorMessage"];
                    this.DebugReturn("Login error: " + error);
                    OnLoginResult(new LoginResultEventArgs(false, error));
                    return;
                }
                else
                {
                    this.DebugReturn("Logged in successfully - Configuring Local Player");
                    // remove any old instances of player from the scene
                    ClearRemotePlayers();
                    // update player ID
                    if (smartFox.MySelf == null)
                    {
                        this.DebugReturn("smartfox STILL doesn't know who I am!");
                    }
                    else
                    {
                        this.DebugReturn("SF Player: " + smartFox.MySelf.Id);
                    }
                    this.localPlayer.Name = smartFox.MySelf.Name;
                    this.localPlayer.PlayerID = smartFox.MySelf.Id;
                    OnLoginResult(new LoginResultEventArgs(true, ""));
                    if (_reconnectAttempt)
                    {
                        JoinRoom(_reconnectCurrentRoom, _roomPassword);
                    }
                }
            }
            catch (Exception ex)
            {
                this.DebugReturn("Exception handling login request: " + ex.Message + " " + ex.StackTrace);
            }
        }
        void OnJoinRoom(BaseEvent evt)
        {
            Room room = (Room)evt.Params["room"];
            this.DebugReturn("Room " + room.Name + " joined successfully");
            _reconnectCurrentRoom = room.Name;
            StorePlayerSettings();
            isJoining = false;
			OnRoomJoinResult(new RoomJoinResultEventArgs(true, ""));
            _reconnectAttempt = false; // no longer need this flag once the user is at this stage
            foreach (User user in room.UserList)
            {
                if (user.Id != localPlayer.PlayerID)
                {
                    if (_jibePlayers[user.Id] == null)
                    {
                        AddNewRemotePlayer(user);
                    }
                }
                SendAppearance();
            }
        }
        void OnAddRoom(BaseEvent evt)
        {
            if (_roomAddAttempt)
            {
                _roomAddAttempt = false;
                Room room = (Room)evt.Params["room"];
                this.DebugReturn("Added room successfully - now joining " + room.Name);
                smartFox.Send(new JoinRoomRequest(room.Name, ""));
            }
        }
        void OnJoinRoomError(BaseEvent evt)
        {
            string message = (string)evt.Params["errorMessage"];
            this.DebugReturn(_debugLevel, "ERROR joining room: " + message, JibeErrorLevel.WARNING);
            OnRoomJoinResult(new RoomJoinResultEventArgs(false, message + " Default room is configured as " + _defaultRoom + " and dynamic room support is set enabled=" + _dynamicRooms));
        }
        void OnRoomCreateError(BaseEvent evt)
        {
            string message = (string)evt.Params["errorMessage"];
            this.DebugReturn(_debugLevel, "ERROR creating room: " + message, JibeErrorLevel.WARNING);
            //OnRoomJoinResult(new RoomJoinResultEventArgs(false, message));
        }
        private void OnUserEnterRoom(int roomId, User user)
        {
            // This will be invoked when remote player enters our room
            this.DebugReturn("Remote user entering room!");
            if (user.Id != localPlayer.PlayerID)
            {
                if (_jibePlayers[user.Id] == null)
                {
                    AddNewRemotePlayer(user);
                }
            }
            SendAppearance();
        }
        private void OnUserExit(BaseEvent evt)
        {
            Room room = (Room)evt.Params["room"];
            User user = (User)evt.Params["user"];
            if (user != null)
            {
                this.DebugReturn("User " + user.Name + ", userId: " + user.Id + " leaving room " + room.Name);

            // This will be invoked when a remote player leaves the room

                if (this._jibePlayers.ContainsKey(user.Id))
            {
                this.OnLostRemotePlayer(new RemotePlayerEventArgs(_jibePlayers[user.Id]));
                _jibePlayers.Remove(user.Id);
            }
                if (user.Id == localPlayer.PlayerID)
            {
                OnRoomLeaveResult(new RoomLeaveResultEventArgs(true, ""));
            }
            }
        }
        private void OnUserCountChange(BaseEvent evt)
        {
            List<IJibePlayer> currentPlayers = new List<IJibePlayer>();
            List<IJibePlayer> playersToGo = new List<IJibePlayer>();
            foreach (User user in smartFox.LastJoinedRoom.UserList)
            {
                currentPlayers.Add(GetJibePlayerFromSFS2XUser(user));
            }
            lock (Players)
            {
                foreach (IJibePlayer player in currentPlayers)
                {
                    if (!currentPlayers.Contains(player))
                    {
                        playersToGo.Add(player);
                    }
                }
            }
            foreach (IJibePlayer leavingPlayer in playersToGo)
            {
                this.OnLostRemotePlayer(new RemotePlayerEventArgs(_jibePlayers[leavingPlayer.PlayerID]));
                _jibePlayers.Remove(leavingPlayer.PlayerID);
            }
        }

        public void OnConnectionLost(BaseEvent evt)
        {
            isJoining = false;
            this._isConnected = false;
            this.DebugReturn("OnConnectionLost - Client is a gonner - time to try and connect to zone " + _zone + " and room " + _reconnectCurrentRoom + "!");
            if (!_reconnectAttempt)
            {
                _reconnectAttempt = true;
                // Clear out old editions of localPlayer, attempt to reconnect to server
                localPlayer.RaiseDisconnect();
                Connect();
            }
        }

        public void OnDebugMessage(BaseEvent evt)
        {
            string message = (string)evt.Params["message"];
            this.DebugReturn("[SFS DEBUG] " + message);
        }

        public void OnLoginError(BaseEvent evt)
        {
            string error = (string)evt.Params["errorMessage"];
            this.DebugReturn("Login error: " + error);
            OnLoginResult(new LoginResultEventArgs(false, error));
            return;
        }

        void OnLogout(BaseEvent evt)
        {
            isJoining = false;
            this.DebugReturn("OnLogout");
        }
        void OnPublicMessage(BaseEvent evt)
        {
            // Public chat message handler
            try
            {
                string message = (string)evt.Params["message"];
                User sender = (User)evt.Params["sender"];
                if (sender.Id != localPlayer.PlayerID)
                {
                    IJibePlayer remotePlayer = GetJibePlayerFromSFS2XUser(sender);
                    // If it's not myself
                    this.OnNewChatMessage(new ChatEventArgs(remotePlayer, message));
                }

            }
            catch (Exception ex)
            {
                this.DebugReturn(_debugLevel, "Exception handling public message: " + ex.Message + ex.StackTrace, JibeErrorLevel.ERROR);
            }
        }
        void OnPrivateMessage(BaseEvent evt)
        {
            // Private chat message handler
            try
            {
                string message = (string)evt.Params["message"];
				this.DebugReturn(message);
                User sender = (User)evt.Params["sender"];

                IJibePlayer remotePlayer = GetJibePlayerFromSFS2XUser(sender);
                this.OnNewPrivateChatMessage(new PrivateChatEventArgs(remotePlayer, message));

            }
            catch (Exception ex)
            {
                this.DebugReturn(_debugLevel, "Exception handling private message: " + ex.Message + ex.StackTrace, JibeErrorLevel.ERROR);
            }
        }
        void OnObjectReceived(BaseEvent evt)
        {
            SFSObject data = (SFSObject)evt.Params["message"];
            User sender = (User)evt.Params["sender"];
            string dataReceived = data.GetUtfString("_cmd");
            switch (dataReceived)
            {
                case SmartFox2MessageType.Transform:
                    //this.DebugReturn("Transform from " + sender.Id);
                    UpdateRemoteTransform(data, GetJibePlayerFromSFS2XUser(sender));
                    break;
                case SmartFox2MessageType.ForceSend:
                    //this.DebugReturn("Force Send from " + sender.Id);
                    ProcessFullUpdateRequest(data);
                    break;
                case SmartFox2MessageType.Animation:
                    //this.DebugReturn("Anim from " + sender.Id);
                    UpdateRemoteAnimation(data, GetJibePlayerFromSFS2XUser(sender));
                    break;
                case SmartFox2MessageType.VisualAppearance:
                    //this.DebugReturn("Appearance from " + sender.Id);
                    UpdateRemoteAppearance(data, GetJibePlayerFromSFS2XUser(sender));
                    break;
                case SmartFox2MessageType.Name:
                    //this.DebugReturn("Name from " + sender.Id);
                    UpdateRemoteName(data, GetJibePlayerFromSFS2XUser(sender));
                    break;
                case SmartFox2MessageType.Speech:
                    //this.DebugReturn("Speech from " + sender.Id);
                    UpdateRemoteSpeech(data, GetJibePlayerFromSFS2XUser(sender));
                    break;
                case SmartFox2MessageType.Custom:
                    //this.DebugReturn("Custom from " + sender.Id);
                    ProcessCustomData(data, GetJibePlayerFromSFS2XUser(sender));
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void StorePlayerSettings()
        {
            // Keep SmartFox server up-to-date with information about the current player
            List<UserVariable> userVars = new List<UserVariable>();
            userVars.Add(new SFSUserVariable("name", localPlayer.Name));
            userVars.Add(new SFSUserVariable("hair", localPlayer.Hair));
            userVars.Add(new SFSUserVariable("avatar", localPlayer.AvatarModel));
            userVars.Add(new SFSUserVariable("skin", localPlayer.Skin));
            smartFox.MySelf.SetVariables(userVars);
        }

        private IJibePlayer GetJibePlayerFromSFS2XUser(User user)
        {
            int i = user.Id;
            if (i == localPlayer.PlayerID)
            {
                return localPlayer;
            }
            else if (!_jibePlayers.ContainsKey(user.Id))
            {
                return AddNewRemotePlayer(user);
            }
            else
            {
                // pass back a reference to the remote player object
                return _jibePlayers[user.Id];
            }
        }

        private IJibePlayer AddNewRemotePlayer(User user)
        {
            // Each time a new message comes in we check to see if the user is already known. If not, we add them!
            int i = user.Id;
            if (!this._jibePlayers.ContainsKey(i) && i != localPlayer.PlayerID)
            {
                this.DebugReturn("Spawning new remote player: " + user.Name + ", ID: " + user.Id);
                this._jibePlayers[i] = new JibePlayer(i);
                this._jibePlayers[i].Name = user.Name;
                this.OnNewRemotePlayer(new RemotePlayerEventArgs(_jibePlayers[i]));
                RequestFullUpdate(_jibePlayers[i]);
            }
            return _jibePlayers[i];
        }
        private void UpdateRemoteAppearance(SFSObject data, IJibePlayer player)
        {
            player.Skin = (string)data.GetUtfString("skin");
            player.Hair = (string)data.GetUtfString("hair");
            player.AvatarModel = data.GetInt("avatar");
        }
        private void UpdateRemoteAnimation(SFSObject data, IJibePlayer player)
        {
            string currentAnim = data.GetUtfString("mes");
            player.Animation = currentAnim;
        }
        private void UpdateRemoteName(SFSObject data, IJibePlayer player)
        {
            string name = data.GetUtfString("name");
            player.Name = name;
        }
        private void UpdateRemoteSpeech(SFSObject data, IJibePlayer player)
        {
            int voiceStatus = data.GetInt("v");
            player.Voice = (JibePlayerVoice)voiceStatus;
            this.DebugReturn("Player voice updated: " + player.Name + ":" + player.Voice);
        }
        private void ProcessFullUpdateRequest(SFSObject data)
        {
            //if this message is addressed to this player
            if (data.GetInt("to_uid") == smartFox.MySelf.Id)
            {
                this.DebugReturn("ForceSendTransform Message Received");
                SendFullUpdate();
            }
        }
        private void SendFullUpdate()
        {
            SendTransform(localPlayer.PosX, localPlayer.PosY, localPlayer.PosZ, localPlayer.RotX, localPlayer.RotY, localPlayer.RotZ, localPlayer.RotW);
            SendAnimation(localPlayer.Animation.ToString());
            SendAppearance();
            SendSpeech();
        }

        // updates a (remote player) position. directly gets the new position from the received event
        internal void UpdateRemoteTransform(SFSObject data, IJibePlayer player)
        {
            float newPosX = data.GetFloat("x");
            float newPosY = data.GetFloat("y");
            float newPosZ = data.GetFloat("z");
            float newRotX = data.GetFloat("rx");
            float newRotY = data.GetFloat("ry");
            float newRotZ = data.GetFloat("rz");
            float newRotW = data.GetFloat("w");

            lock (this)
            {
                player.PosX = newPosX;
                player.PosY = newPosY;
                player.PosZ = newPosZ;
                player.RotX = newRotX;
                player.RotY = newRotY;
                player.RotZ = newRotZ;
                player.RotW = newRotW;
            }
        }
        internal void ProcessCustomData(SFSObject data, IJibePlayer player)
        {
            Dictionary<string, string> dataReceived = new Dictionary<string, string>();
            foreach (string key in data.GetKeys())
            {
                if (key != "_cmd") dataReceived[key] = data.GetUtfString(key);
            }
//            this.DebugReturn("Received new custom data with " + dataReceived.Count + " values");
            this.OnCustomData(new CustomDataEventArgs(player, dataReceived));
        }

        #region DataSend code
        public override void SendTransform(float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.Transform);  //contains transform sync data.
            data.PutFloat("x", posX);
            data.PutFloat("y", posY);
            data.PutFloat("z", posZ);
            data.PutFloat("rx", rotX);
            data.PutFloat("ry", rotY);
            data.PutFloat("rz", rotZ);
            data.PutFloat("w", rotW);

            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Transform Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }

        public override void RequestFullUpdate(IJibePlayer player)
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.ForceSend);
            data.PutInt("to_uid", player.PlayerID); // Who this message is for
            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Update Request Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }

        public override void SendAnimation(string animationToPlay)
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.Animation);
            data.PutUtfString("mes", animationToPlay);
            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Animation Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }

        public override void SendAppearance()
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.VisualAppearance);
            data.PutUtfString("skin", localPlayer.Skin);
            data.PutInt("avatar", localPlayer.AvatarModel);
            data.PutUtfString("hair", localPlayer.Hair);
            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Appearance Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }

        public override void SendName()
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.Name);
            data.PutUtfString("name", localPlayer.Name);
            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Name Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }
        public override void SendSpeech()
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.Speech);
            data.PutInt("v", (int)localPlayer.Voice);
            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                this.DebugReturn(_debugLevel, "Voice Indicator Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }

        public override void SendChatMessage(string message)
        {
            try
            {
                smartFox.Send(new PublicMessageRequest(message));
            }
            catch (Exception ex)
            {
                this.DebugReturn(_debugLevel, "Failed to send chat message! " + ex.Message + ex.InnerException, JibeErrorLevel.ERROR);
            }
        }

        public override void SendPrivateChatMessage(string message, int recipientId)
        {
            try
            {
                smartFox.Send(new PrivateMessageRequest(message, recipientId));
            }
            catch (Exception ex)
            {
                this.DebugReturn(_debugLevel, "Failed to send private chat message! " + ex.Message + ex.InnerException, JibeErrorLevel.ERROR);
            }
        }

        public override void SendCustomData(Dictionary<string, string> dataToSend)
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.Custom);
            foreach (KeyValuePair<string, string> dataItem in dataToSend)
            {
                data.PutUtfString(dataItem.Key, dataItem.Value);
            }
            this.DebugReturn("Sending custom data with " + dataToSend.Count + " values");
            try
            {
                SendDatawithConnectionCheck(data);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Custom data Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }
        private void SendDatawithConnectionCheck(SFSObject dataToSend)
        {
            // This method is to check that the SmartFox connection state is still active and connected.
            if (smartFox.IsConnected)
            {
                // As long as we're not in the middle of a room join request
                if (!isJoining)
                {
                    smartFox.Send(new ObjectMessageRequest(dataToSend));
                }
				else
				{
					this.DebugReturn("Still joining room - could not send custom data");
				}
            }
            else
            {
                // If SmartFox is not connected, reconnect!
                if (!_reconnectAttempt) // if a reconnect attempt has not yet been made
                {
                    _reconnectAttempt = true;
                    localPlayer.RaiseDisconnect();
                    Connect();
                }
            }
        }

        public void SendPrivateCustomData(Dictionary<string, string> dataToSend, ICollection<User> targets)
        {
            SFSObject data = new SFSObject();
            data.PutUtfString("_cmd", SmartFox2MessageType.Custom);
            foreach (KeyValuePair<string, string> dataItem in dataToSend)
            {
                data.PutUtfString(dataItem.Key, dataItem.Value);
            }
            this.DebugReturn("Sending custom data with " + dataToSend.Count + " values");
            try
            {
                SendPrivateDatawithConnectionCheck(data,targets);
            }
            catch (Exception ex)
            {
                // connection failure?
                this.DebugReturn(_debugLevel, "Custom data Send Failure: " + ex.Message, JibeErrorLevel.WARNING);
            }
        }
        private void SendPrivateDatawithConnectionCheck(SFSObject dataToSend, ICollection<User> targets)
        {
            // This method is to check that the SmartFox connection state is still active and connected.
            if (smartFox.IsConnected)
            {
                // As long as we're not in the middle of a room join request
                if (!isJoining)
                {
                    smartFox.Send(new ObjectMessageRequest(dataToSend, smartFox.LastJoinedRoom, targets));
                }
            }
            else
            {
                // If SmartFox is not connected, reconnect!
                if (!_reconnectAttempt) // if a reconnect attempt has not yet been made
                {
                    _reconnectAttempt = true;
                    localPlayer.RaiseDisconnect();
                    Connect();
                }
            }
        }

        #endregion

    }
}
