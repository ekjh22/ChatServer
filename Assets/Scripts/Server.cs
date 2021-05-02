using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JWT;

namespace GoldServer.Server
{
    /// <summary>
    /// 서버에 접속한 클라이언트
    /// </summary>
    public class ServerClient {

        public TcpClient Tcp;
        public string ClientName { get; set; }

        public ServerClient(TcpClient _socket) {

            ClientName = "Guest";
            Tcp = _socket;
        }
    }

    // Server
    public partial class Server : MonoBehaviour {

        [Header("서버")]
        
        [Tooltip("자신의 IP값을 입력하는 오브젝트")]
        [SerializeField] private InputField _ipInput;
        
        [Tooltip("자신이 채팅방에서 사용할 이름을 입력하는 오브젝트")]
        [SerializeField] private InputField _nameInput;

        List<ServerClient> _clients        = new List<ServerClient>();
        List<ServerClient> _disconnectList = new List<ServerClient>();

        TcpListener _server = null;
        bool        _serverStarted = false;

        public void ServerCreate() {

            try {
                _clients = new List<ServerClient>();

                var _port = _portInput.text == "" ? 7777 : int.Parse(_portInput.text);

                _server = new TcpListener(IPAddress.Any, _port);
                _server.Start();

                StartListening();
                _serverStarted = true;
                ShowMessage($"Server Started : Port [{_port}]");
            }
            catch (Exception e) {

                ShowMessage($"Socket error : {e.Message}");
            }
        }

        void Awake() {

            JWTStart();
            ServerStart();
            ClientStart();
        }

        void Update() {

            ServerUpdate();
            ClientUpdate();
        }

        void ServerStart() {

            if (PlayerPrefs.HasKey("Name")) {
                _nameInput.text = PlayerPrefs.GetString("Name");
            }
        }

        void ServerUpdate() {

            if (!_serverStarted)
                return;

            for (int i = 0; i < _clients.Count; i++) {

                if (!IsConnected(_clients[i].Tcp)) {

                    _clients[i].Tcp.Close();
                    _disconnectList.Add(_clients[i]);

                    continue;
                } else {

                    NetworkStream _stream = _clients[i].Tcp.GetStream();

                    if (_stream.DataAvailable) {

                        var _data = new StreamReader(_stream, true).ReadLine();
                        if (_data != null)
                        {
                            OnIncommingData(_clients[i], _data);
                        }

                    }
                }
            }

            for (int i = 0; i < _disconnectList.Count - 1; i++) {

                Broadcast($"{_disconnectList[i].ClientName} Disconnected", _clients);

                _clients.Remove(_disconnectList[i]);
                _disconnectList.RemoveAt(i);
            }
        }

        bool IsConnected(TcpClient _client) {

            try {

                if (_client != null && _client.Client != null && _client.Client.Connected) {

                    if (_client.Client.Poll(0, SelectMode.SelectRead))
                        return !(_client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                    return true;
                }
                else return false;
            } catch {

                return false;
            }
        }

        void StartListening() {

            _server.BeginAcceptTcpClient(AcceptTcpClient, _server);
        }

        void AcceptTcpClient(IAsyncResult _ar) {

            TcpListener _listener = (TcpListener)_ar.AsyncState;
            _clients.Add(new ServerClient(_listener.EndAcceptTcpClient(_ar)));

            StartListening();
            Broadcast("%NAME", new List<ServerClient>() { _clients[_clients.Count - 1] });
        }

        void OnIncommingData(ServerClient _client, string _data) {

            if (_data.Contains("&NAME")) {

                _client.ClientName = _data.Split('|')[1];

                Encode(_client);

                Broadcast($"{_client.ClientName} Connected", _clients);
                return;
            }

            Broadcast($"{_client.ClientName} : {_data}", _clients);
        }

        void Broadcast(string _data, List<ServerClient> _client) {

            for (int i = 0; i < _client.Count; i++) {

                try {

                    StreamWriter _writer = new StreamWriter(_client[i].Tcp.GetStream());
                    _writer.WriteLine(_data);
                    _writer.Flush();
                } catch (Exception e) {

                    ShowMessage($"Write Error : {e.Message} to Client {_client[i].ClientName}");
                }
            }
        }
    }
}