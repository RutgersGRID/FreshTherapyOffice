/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * JibePhotonServer.cs Revision 1.4.2.1108.29
 * Provides Jibe implementation on Photon platform */

namespace ReactionGrid.Jibe
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ExitGames.Client.Photon;
    using System.Threading;

    public class JibePhotonServer : JibeServerBase, IPhotonPeerListener
    {
        public LitePeer peer;
        private string _serverAddress;
        private int _serverPort;
        private string _zone;
        // Not used in Photon:
        //private string _defaultRoom = "";
        //private string _roomPassword = "";
        //private bool _dynamicRooms = true;
        private string ipPort;

        private bool isJoining = false;
        public bool useTcp = false;
        public string Zone = "jibeChat";
        private string _currentRoom = "";
        private JibePlayer localPlayer;
        private JibeDebugLevel _debugLevel;
        #region JibePlayer Constants
        private const bool SEND_RELIABLE = true;
        private const bool SEND_UNRELIABLE = false;

        internal const byte STATUS_PLAYER_POS_X = 43;
        internal const byte STATUS_PLAYER_POS_Y = 44;
        internal const byte STATUS_PLAYER_POS_Z = 45;
        internal const byte STATUS_PLAYER_ROT_X = 46;
        internal const byte STATUS_PLAYER_ROT_Y = 47;
        internal const byte STATUS_PLAYER_ROT_Z = 48;
        internal const byte STATUS_PLAYER_ROT_W = 49;
        internal const byte STATUS_PLAYER_NAME = 50;
        internal const byte STATUS_PLAYER_SKIN = 51;
        internal const byte STATUS_PLAYER_HAIR = 52;
        internal const byte STATUS_PLAYER_ANIMATION = 53;
        internal const byte STATUS_PLAYER_AVATAR = 54;
        internal const byte STATUS_CHAT_MESSAGE = 55;
        internal const byte STATUS_CHAT_RECIPIENT = 56;
        internal const byte STATUS_VOICE = 57;

        internal const byte OP_JOIN = 90;           // server
        internal const byte OP_LEAVE = 91;          // server
        internal const byte OP_RAISEEVENT = 92;     // server
        internal const byte OP_SETPROPERTIES = 93;  // server
        internal const byte OP_GETPROPERTIES = 94;  // server

        internal const byte UD_TRANSFORM = 101;
        internal const byte UD_ANIMATION = 102;
        internal const byte UD_APPEARANCE = 103;
        internal const byte UD_NAME = 104;
        internal const byte UD_SPEECH = 107;
        internal const byte RQ_FULL_UPDATE = 106;
        internal const byte CUSTOM_DATA = 112;
        internal const byte CUSTOM_DATA_ITEM = 113;

        internal const byte OP_PING = 104;          // server

        internal const byte CHAT_MESSAGE = 105;
        //internal const byte RECEIVE_POSITION = 106;
        //internal const byte RECEIVE_ANIM = 107;
        //internal const byte RECEIVE_APPEARANCE = 108;
        //internal const byte RECEIVE_VOICE = 110;
        //internal const byte RECEIVE_NAME = 111;
        #endregion

        public JibePhotonServer(string ip, int port, string zone, string room, int dispatchInterval, int sendInterval, JibeDebugLevel debugLevel, DebugOutputDelegate debugDelegate, DebugWarningOutputDelegate debugWarningDelegate, DebugErrorOutputDelegate debugErrorDelegate)
            : base(dispatchInterval, sendInterval)
        {
            this.DebugListeners = debugDelegate;
            this.DebugWarningListeners = debugWarningDelegate;
            this.DebugErrorListeners = debugErrorDelegate;

            _serverAddress = ip;
            _serverPort = port;
            _zone = zone;
            _debugLevel = debugLevel;
            ipPort = _serverAddress + ":" + _serverPort.ToString();
            _debugLevel = debugLevel;
            this.localPlayer = new JibePlayer(-1);
        }

        public override IJibePlayer Connect()
        {
            if (this.peer == null)
            {
                this.peer = new LitePeer(this, this.useTcp);
            }
            else if (this.peer.PeerState == PeerStateValue.Connected)
            {
                this.DebugReturn("already connected!");
                return this.localPlayer;
            }
            else
            {
                this.DebugReturn("Connection wibble? " + this.peer.PeerState);
                return null;
            }
            // Specify the amount of debugging/logging from the Photon library
            DebugLevel _photonDebug = (DebugLevel)_debugLevel;
            this.peer.DebugOut = _photonDebug;

            this.DebugReturn("Attempting to connect to " + this.ipPort);
            if (!this.peer.Connect(this.ipPort, "JibePhoton"))
            {
                this.DebugReturn("not connected");
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
            OnLoginResult(new LoginResultEventArgs(true, ""));
        }

        public override void JoinRoom(string roomName, string roomPassword)
        {
            if (isJoining) return;
            if (roomName == _currentRoom)
            {
                this.DebugReturn("Join request for " + roomName + " ignored, already in room");
                isJoining = false;
                OnRoomJoinResult(new RoomJoinResultEventArgs(true, ""));
            }
            else
            {
                isJoining = true;
                this.DebugReturn("Joining room " + roomName);
                _currentRoom = roomName;
                this.peer.OpJoin(roomName);
            }
        }

        public override void LeaveRoom()
        {
            if (!string.IsNullOrEmpty(_currentRoom))
            {
                this.DebugReturn("Leaving room " + _currentRoom);
                this.peer.OpLeave(_currentRoom);
            }
        }

        // Disconnect from the server.
        public override void Disconnect()
        {
            localPlayer.RaiseDisconnect();
            if (this.peer != null)
            {
                this.peer.Disconnect();	//this will dump all prepared outgoing data and immediately send a "disconnect"
            }
        }

        protected override void ProcessIncomingMessages()
        {
            this.DispatchAll();
        }
        protected override void SendOutgoingMessages()
        {
            this.SendOutgoingCommands();
        }
        #endregion

        #region IPhotonPeerListener Members
        public void DebugReturn(DebugLevel level, string debug)
        {
            this.DebugListeners(debug); // This demo simply ignores the debug level
        }
        public void SetDebugListener(DebugOutputDelegate debugDelegate)
        {
            this.DebugListeners = debugDelegate;
        }

        // Photon library callback for state changes (connect, disconnect, etc.)
        // Processed within PhotonPeer.DispatchIncomingCommands()!
        public void PeerStatusCallback(StatusCode returnCode)
        {
            this.DebugReturn("nPeerReturn():" + returnCode);

            // handle returnCodes for connect, disconnect and errors (non-operations)
            switch (returnCode)
            {
                case StatusCode.Connect:
                    this.DebugReturn("Connect(ed)");
                    this._isConnected = true;
                    break;
                case StatusCode.Disconnect:
                    this.DebugReturn("Disconnect(ed) peer.state: " + this.peer.PeerState);
                    this.localPlayer.PlayerID = 0;
                    this._jibePlayers.Clear();
                    this._isConnected = false;
                    break;
                case StatusCode.Exception_Connect:
                    this.DebugReturn("Exception_Connect(ed) peer.state: " + this.peer.PeerState);
                    this.localPlayer.PlayerID = 0;
                    this._jibePlayers.Clear();
                    this._isConnected = false;
                    break;
                case StatusCode.Exception:
                    this.DebugReturn("Exception peer.state: " + this.peer.PeerState);
                    this._jibePlayers.Clear();
                    this.localPlayer.PlayerID = 0;
                    break;
                case StatusCode.SendError:
                    this.DebugReturn("SendError! peer.state: " + this.peer.PeerState);
                    //this._jibePlayers.Clear();
                    this.localPlayer.PlayerID = 0;
                    break;
                default:
                    this.DebugReturn("PeerStatusCallback: " + returnCode);
                    break;
            }
        }
        // Photon library callback to get us operation results (if our operation was executed server-side)
        // Only called for reliable commands! Anything sent unreliable will not produce a result.
        // Processed within PhotonPeer.DispatchIncomingCommands()!
        public void OperationResult(byte opCode, int returnCode, Hashtable returnValues, short invocID)
        {
            // handle operation returns (aside from "join", this demo does not watch for returns)
            switch (opCode)
            {
                case (byte)LiteOpCode.Join:

                    // get the local player's numer from the returnvalues, get the player from the list
                    int actorNrReturnedForOpJoin = (int)returnValues[(byte)LiteOpKey.ActorNr];
                    this.localPlayer.PlayerID = actorNrReturnedForOpJoin;

                    //this._jibePlayers[actorNrReturnedForOpJoin] = this.localPlayer;
                    this.DebugReturn("LocalPlayer: " + this.localPlayer);
                    isJoining = false;
                    OnRoomJoinResult(new RoomJoinResultEventArgs(true, ""));

                    break;
                case (byte)LiteOpCode.Leave:
                    _jibePlayers.Clear();
                    OnRoomLeaveResult(new RoomLeaveResultEventArgs(true, ""));
                    break;
            }
        }

        // Called by Photon lib for each incoming event (player- and position-data, as well as joins and leaves).
        // Processed within PhotonPeer.DispatchIncomingCommands()!
        public void EventAction(byte eventCode, Hashtable photonEvent)
        {
            incomingEventCount++;

            if (eventCode != UD_TRANSFORM)
            {
                if (_debugLevel == JibeDebugLevel.ALL)
                    this.DebugReturn(_debugLevel, "EventAction() " + eventCode + " " + Enum.GetName(typeof(LiteEventCode), eventCode), JibeErrorLevel.INFO);
            }

            int actorNr = (int)photonEvent[(byte)LiteEventKey.ActorNr];
            int[] actorsInGame = (int[])photonEvent[(byte)LiteEventKey.ActorList];
            // get the player that raised this event

            IJibePlayer p = GetJibePlayerFromPhotonUser(actorNr);

            switch (eventCode)
            {
                case (byte)LiteEventCode.Join:
                    if (p != null)
                    {
                        this.DebugReturn("Player joining: " + p);
                    }

                    this.SendAppearance(); // someone joined. as the server does not keep info: send it again
                    this.DebugReturn("jibe player has joined - spawn all remote players");
                    foreach (int i in actorsInGame)
                    {
                        GetJibePlayerFromPhotonUser(i);
                    }
                    // get the list of current players              
                    this.PrintPlayers();

                    break;
                case (byte)LiteEventCode.Leave:
                    if (this._jibePlayers[actorNr] != null)
                    {
                        this._jibePlayers[actorNr].RaiseDisconnect();
                        this._jibePlayers.Remove(actorNr);
                    }
                    if (actorNr == localPlayer.PlayerID)
                    {
                        this._jibePlayers.Clear();
                        OnRoomLeaveResult(new RoomLeaveResultEventArgs(true, ""));
                    }
                    break;
                case UD_APPEARANCE:
                    UpdateRemoteAppearance((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case UD_TRANSFORM:
                    UpdateRemoteTransform((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case UD_ANIMATION:
                    UpdateRemoteAnimation((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case CHAT_MESSAGE:
                    ProcessChat((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case UD_SPEECH:
                    UpdateRemoteSpeech((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case UD_NAME:
                    UpdateRemoteName((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case CUSTOM_DATA:
                    ProcessCustomData((Hashtable)photonEvent[(byte)LiteEventKey.Data], p);
                    break;
                case RQ_FULL_UPDATE:
                    SendFullUpdate();
                    break;

            }
        }

        private IJibePlayer GetJibePlayerFromPhotonUser(int actorNr)
        {
            if (actorNr == localPlayer.PlayerID)
            {
                return localPlayer;
            }
            else if (!this._jibePlayers.ContainsKey(actorNr))
            {
                AddNewRemotePlayer(actorNr);
            }

            // pass back a reference to the remote player object
            return _jibePlayers[actorNr];
        }

        private void AddNewRemotePlayer(int actorNr)
        {
            if (actorNr != localPlayer.PlayerID)
            {
                this._jibePlayers[actorNr] = new JibePlayer(actorNr);
                this.DebugReturn("New jibe player has joined - spawn remote player");
                this.OnNewRemotePlayer(new RemotePlayerEventArgs(_jibePlayers[actorNr]));
                RequestFullUpdate(_jibePlayers[actorNr]);
            }
        }

        #endregion
        #region Game Handling

        internal void SendFullUpdate()
        {
            SendName();
            SendTransform(localPlayer.PosX, localPlayer.PosY, localPlayer.PosZ, localPlayer.RotX, localPlayer.RotY, localPlayer.RotZ, localPlayer.RotW);
            SendAnimation(localPlayer.Animation.ToString());
            SendAppearance();
            SendSpeech();            
        }

        internal void SendTransform()
        {
            // dont move if player does not have a number or peer is not connected
            if (this.localPlayer == null || this.localPlayer.PlayerID == 0 || this.peer == null)
            {
                return;
            }

            Hashtable evInfo = new Hashtable();

            evInfo.Add((Object)STATUS_PLAYER_POS_X, localPlayer.PosX);
            evInfo.Add((Object)STATUS_PLAYER_POS_Y, localPlayer.PosY);
            evInfo.Add((Object)STATUS_PLAYER_POS_Z, localPlayer.PosZ);
            evInfo.Add((Object)STATUS_PLAYER_ROT_X, localPlayer.RotX);
            evInfo.Add((Object)STATUS_PLAYER_ROT_Y, localPlayer.RotY);
            evInfo.Add((Object)STATUS_PLAYER_ROT_Z, localPlayer.RotZ);
            evInfo.Add((Object)STATUS_PLAYER_ROT_W, localPlayer.RotW);
            peer.OpRaiseEvent(UD_TRANSFORM, evInfo, SEND_UNRELIABLE); // Don't need to enforce reliable for movement
        }

        // Actually sends the outgoing data (which was previously queued in the PhotonPeer)
        internal void SendOutgoingCommands()
        {
            this.peer.SendOutgoingCommands();
        }

        int incomingEventCount;

        internal void DispatchAll()
        {
            incomingEventCount = 0;

            while (this.peer != null && this.peer.DispatchIncomingCommands())
            {
            }

            //if (incomingEventCount > 0) DebugReturn(string.Format("Processed {0} incoming events", incomingEventCount));
        }

        // PhotonPeer.Service() combines PhotonPeer.DispatchIncomingCommands and PhotonPeer.SendOutgoingCommands.
        internal void Service()
        {
            this.peer.Service();
        }

        // Simple "help" function to print current list of players.
        // As this function uses the players list, make sure it's not called while 
        // peer.DispatchIncomingCommands() might modify the list!! (e.g. by lock(this))
        public void PrintPlayers()
        {
            string players = "Players: " + this._jibePlayers.Count;
            //foreach (JibePlayer p in this.Players)
            //{
            //    players += p.ToString() + ", ";
            //}

            this.DebugReturn(players);
        }

        #endregion

        #region JibePlayer Messages


        internal void SetInfo(Hashtable evData, IJibePlayer player)
        {
            player.Name = (string)evData[STATUS_PLAYER_NAME];
            player.Skin = (string)evData[STATUS_PLAYER_SKIN];
            player.Hair = (string)evData[STATUS_PLAYER_HAIR];
            player.Animation = (string)evData[STATUS_PLAYER_ANIMATION];
            player.AvatarModel = (int)evData[STATUS_PLAYER_AVATAR];
        }

        internal void UpdateRemoteAppearance(Hashtable evData, IJibePlayer player)
        {
            player.Skin = (string)evData[STATUS_PLAYER_SKIN];
            player.Hair = (string)evData[STATUS_PLAYER_HAIR];
            player.AvatarModel = (int)evData[STATUS_PLAYER_AVATAR];
        }
        // updates a (remote player's) position. directly gets the new position from the received event
        internal void UpdateRemoteTransform(Hashtable evData, IJibePlayer player)
        {
            player.PosX = (float)evData[STATUS_PLAYER_POS_X];
            player.PosY = (float)evData[STATUS_PLAYER_POS_Y];
            player.PosZ = (float)evData[STATUS_PLAYER_POS_Z];
            player.RotX = (float)evData[STATUS_PLAYER_ROT_X];
            player.RotY = (float)evData[STATUS_PLAYER_ROT_Y];
            player.RotZ = (float)evData[STATUS_PLAYER_ROT_Z];
            player.RotW = (float)evData[STATUS_PLAYER_ROT_W];
        }

        internal void UpdateRemoteAnimation(Hashtable evData, IJibePlayer player)
        {
            player.Animation = (string)evData[STATUS_PLAYER_ANIMATION];
        }
        internal void UpdateRemoteSpeech(Hashtable evData, IJibePlayer player)
        {
            player.Voice = (JibePlayerVoice)(int)evData[STATUS_VOICE];
        }
        internal void UpdateRemoteName(Hashtable evData, IJibePlayer player)
        {
            player.Name = (string)evData[STATUS_PLAYER_NAME];
        }

        internal void ProcessChat(Hashtable evData, IJibePlayer player)
        {
            if (evData[STATUS_CHAT_RECIPIENT] != null)
            {
                // Private chat
                if ((int)evData[STATUS_CHAT_RECIPIENT] == localPlayer.PlayerID)
                {
                    // this message meant for me!
                    string message = (string)evData[STATUS_CHAT_MESSAGE];
                    this.OnNewPrivateChatMessage(new PrivateChatEventArgs(player, message));
                }
            }
            else
            {
                // Public chat
                string message = (string)evData[STATUS_CHAT_MESSAGE];
                this.OnNewChatMessage(new ChatEventArgs(player, message));
            }
        }

        internal void ProcessCustomData(Hashtable evData, IJibePlayer player)
        {
            Dictionary<string, string> dataReceived = new Dictionary<string, string>();
            foreach (string key in evData.Keys)
            {
                if (key != "_cmd") dataReceived[key] = (string)evData[key];
            }
            this.DebugReturn("Received new custom data with " + dataReceived.Count + " values");
            this.OnCustomData(new CustomDataEventArgs(player, dataReceived));
        }
        #endregion

        public override void SendTransform(float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
        {
            //this.DebugReturn("storing transform for player " + localPlayer.PlayerID);
            localPlayer.PosX = posX;
            localPlayer.PosY = posY;
            localPlayer.PosZ = posZ;
            localPlayer.RotX = rotX;
            localPlayer.RotY = rotY;
            localPlayer.RotZ = rotZ;
            localPlayer.RotW = rotW;
            SendTransform();
        }

        public override void SendAnimation(string animationToPlay)
        {
            localPlayer.Animation = animationToPlay;
            // Send traffic to server
            Hashtable evInfo = new Hashtable();
            evInfo.Add(STATUS_PLAYER_ANIMATION, animationToPlay);
            peer.OpRaiseEvent(UD_ANIMATION, evInfo, SEND_RELIABLE);
            // Always send a position packet with an animation change to ensure a sitting avatar is positioned correctly
            SendTransform();
        }
        public override void SendAppearance()
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add((Object)STATUS_PLAYER_AVATAR, localPlayer.AvatarModel);
            evInfo.Add((Object)STATUS_PLAYER_SKIN, localPlayer.Skin);
            evInfo.Add((Object)STATUS_PLAYER_HAIR, localPlayer.Hair);
            peer.OpRaiseEvent(UD_APPEARANCE, evInfo, SEND_RELIABLE);  // information updates need to be sent reliable
        }

        public override void SendName()
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add(STATUS_PLAYER_NAME, localPlayer.Name);
            peer.OpRaiseEvent(UD_NAME, evInfo, SEND_RELIABLE);
        }

        public override void SendSpeech()
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add(STATUS_VOICE, (int)localPlayer.Voice);
            peer.OpRaiseEvent(UD_SPEECH, evInfo, SEND_UNRELIABLE);
        }
        public override void SendChatMessage(string message)
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add(STATUS_CHAT_MESSAGE, message);
            peer.OpRaiseEvent(CHAT_MESSAGE, evInfo, SEND_RELIABLE);
        }
        public override void SendPrivateChatMessage(string message, int recipientId)
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add(STATUS_CHAT_MESSAGE, message);
            evInfo.Add(STATUS_CHAT_RECIPIENT, recipientId);
            peer.OpRaiseEvent(CHAT_MESSAGE, evInfo, SEND_RELIABLE);
        }
        public override void SendCustomData(Dictionary<string, string> dataToSend)
        {
            Hashtable evInfo = new Hashtable();
            foreach (string key in dataToSend.Keys)
            {
                evInfo.Add(key,dataToSend[key]);
            }
            peer.OpRaiseEvent(CUSTOM_DATA, evInfo, SEND_RELIABLE);
        }
        public override void RequestFullUpdate(IJibePlayer player)
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add(RQ_FULL_UPDATE, player.PlayerID);
            peer.OpRaiseEvent(RQ_FULL_UPDATE, evInfo, SEND_RELIABLE);
        }

    }
}
