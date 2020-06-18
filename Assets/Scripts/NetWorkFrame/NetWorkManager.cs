using Com.Example;
using Google.Protobuf;
using Msg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace NetWorkFrame
{
    public class NetWorkManager : Singleton<NetWorkManager>
    {
        Socket _socket; //服务器端socket

        private byte[] _receiveBuffer = new byte[1024]; //接收的数据，必须为字节
        int recvLen; //接收的数据长度

        private const int HEAD_SIZE = 4;
        private const int HEAD_NUM = 3;

        private float CONNECT_TIME_OUT = 3.0f;
        private float REQ_TIME_OUT = 5.0f;
        private float KEEP_ALIVE_TIME_OUT = 10.0f;

        private bool _isKeepAlive = false;
        Dictionary<string, IMessage> _msgIDDict;

        private HashSet<ENetworkMessage> _forcePushMessageType;//强制推送消息map
        private HashSet<ENetworkMessage> _needReqMessageType;//需要请求消息类型

        public bool IsConncted
        {
            get { return _socket != null && _socket.Connected; }
        }

        #region LifeCycle
        public override void Init()
        {
            _msgIDDict = new Dictionary<string, IMessage>();

            GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("点击按钮");
                Student ss = new Student();
                ss.Id = 1;
                ss.Email = "22222222";
                SocketSend(ss);
            });

            Debug.Log("client running ...");

            InitForcePushMessageType();
            InitNeedReqMessageType();

            MessageDispatcher.GetInstance().RegisterMessageHandler((uint)EModelMessage.SOCKET_CONNECTED,OnSocketConnected);
            MessageDispatcher.GetInstance().RegisterMessageHandler((uint)EModelMessage.SOCKET_DISCONNECTED, OnSocketDisConnected);

            MessageDispatcher.GetInstance().DispatchMessageAsync((uint)EModelMessage.SOCKET_DISCONNECTED, null);

        }   

        public override void Release()
        {
            //消息UnRegisterMessageHandler
            MessageDispatcher.GetInstance().UnRegisterMessageHandler((uint)EModelMessage.SOCKET_CONNECTED, OnSocketConnected);
            MessageDispatcher.GetInstance().UnRegisterMessageHandler((uint)EModelMessage.SOCKET_DISCONNECTED, OnSocketDisConnected);

            CloseConnection();
        }

        public void OnSocketConnected(uint iMessageType,object kParam)
        {
            if (_receiveBuffer==null)
            {
                _receiveBuffer = new byte[_socket.ReceiveBufferSize];
            }
            _isKeepAlive = true;
            Debug.Log("socket 连接成功");
            BeginReceivePacket();
        }

        public void OnSocketDisConnected(uint iMessageType,object kParam)
        {
            StartCoroutine(BeginTryConnect());
        }

        public void OnSocketDisConnected()
        {
            StartCoroutine(BeginTryConnect());
        }
        #endregion

        #region Connection
        //初始化
        private IEnumerator BeginConnection()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.BeginConnect(IPConfig.IPAddress, IPConfig.IPPort, new AsyncCallback(FinishConnection), null);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.StackTrace);
                yield break;
            }

            yield return new WaitForSeconds(CONNECT_TIME_OUT);

            if (_socket.Connected == false)
            {
                Debug.Log("Client Connect Time Out...");
                CloseConnection();
            }

            _isKeepAlive = _socket.Connected;
        }

        private void FinishConnection(IAsyncResult ar)
        {
            _socket.EndConnect(ar);
            if (_socket.Connected)
            {
                //派发连接成功消息
                Debug.Log("连接后派发消息-！！！");
                MessageDispatcher.GetInstance().DispatchMessageAsync((uint)EModelMessage.SOCKET_CONNECTED, null);
            }
        }

        private void CloseConnection()
        {
            //最后关闭服务器
            if (_socket != null)
            {
                if (_socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    Debug.Log("Client Close...");
                }
                _socket.Close();
            }
        }

        /// <summary>
        /// 当无法接收到心跳包的时候尝试重新连接服务器
        /// </summary>
        /// <returns></returns>
        private IEnumerator BeginTryConnect()
        {
            yield return null;
            while (_socket == null||!_socket.Connected||!_isKeepAlive)
            {
                CloseConnection();
                yield return StartCoroutine(BeginConnection());
            }

            while (_isKeepAlive)
            {
                _isKeepAlive = false;
                yield return new WaitForSeconds(KEEP_ALIVE_TIME_OUT);
            }

            //派发消息 没连接上 disconnected
            MessageDispatcher.GetInstance().DispatchMessageAsync((uint)EModelMessage.SOCKET_DISCONNECTED,null);
        }

        public void OnKeepAliveSync()
        {
            _isKeepAlive = true;
        }
        #endregion

        #region ReceivePacket

        private void BeginReceivePacket()
        {
            try
            {
                lock (_socket)
                {
                    _socket.BeginReceive(_receiveBuffer, 0, _socket.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(EndReceivePacket), null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
        private void EndReceivePacket(IAsyncResult ar)
        {
            int bytesRead = -1;
            try
            {
                if (IsConncted)
                {
                    lock (_socket)
                    {
                        bytesRead = _socket.EndReceive(ar);
                    }
                }
                if (bytesRead == -1)
                {
                    CloseConnection();
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.Log("Receive Closed !");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message+ "\n"+ex.StackTrace+"\n"+ex.Source);
            }

            //Begin Read
            int position = 0;
            while (position<bytesRead)
            {
                int bufferSize = MiniConverter.BytesToInt(_receiveBuffer,position+HEAD_SIZE*0);
                //TODO 
            }

            _receiveBuffer = new byte[1024];
            recvLen = _socket.Receive(_receiveBuffer);
            Student ss =ToolForProtobuf.Deserialize<Student>(_receiveBuffer);
            Debug.Log(" 收 到 消 息 " + ss.Id + " -- = " + ss.Email);
        }

        /// <summary>
        /// 配置需要回复服务器的消息类型
        /// </summary>
        private void InitForcePushMessageType()
        {
            _forcePushMessageType = new HashSet<ENetworkMessage> {
                ENetworkMessage.KeepAliveSync,
                ENetworkMessage.OfflineSync,
                ENetworkMessage.ReceiveChatSync,
            };
        }

        /// <summary>
        /// 配置在Rsq包种同时需要Req包信息的消息类型
        /// </summary>
        private void InitNeedReqMessageType()
        {
            _needReqMessageType = new HashSet<ENetworkMessage> {
                ENetworkMessage.SendChatRsp,
            };
        }

        #endregion

        #region SendPacket
        void SocketSend<T>(T packet) where T : IMessage
        {
            byte[] bf = ToolForProtobuf.Serialize(packet);
            _socket.Send(bf, bf.Length, SocketFlags.None);
        }
        #endregion


    }
}
