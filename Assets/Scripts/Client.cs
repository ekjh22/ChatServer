using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GoldServer.Server
{
    // Client
    public partial class Server : MonoBehaviour {

        [Header("클라이언트")]

        [Tooltip("자신이 들어갈 채팅방 포트를 입력하는 함수")]
        [SerializeField] private InputField _portInput;

        TcpClient _socket;
        NetworkStream _stream;
        StreamWriter _writer;
        StreamReader _reader;

        string _clientName;
        bool _socketReady;

        public void ConnectedToServer() {

            if (_socketReady)
                return;

            var _ip = _ipInput.text == "" ? "127.0.0.1" : _ipInput.text;
            var _port = _portInput.text == "" ? 7777 : int.Parse(_portInput.text);

            try {

                _socket = new TcpClient(_ip, _port);
                _stream = _socket.GetStream();
                _writer = new StreamWriter(_stream);
                _reader = new StreamReader(_stream);

                _socketReady = true;

                if (PlayerPrefs.HasKey("Chat") && PlayerPrefs.GetInt("Port") == _port) {

                   _chatText.text = PlayerPrefs.GetString("Chat");
                }

                PlayerPrefs.SetString("IP", _ip);
                PlayerPrefs.SetInt("Port", _port);
                PlayerPrefs.SetString("Name", _nameInput.text);
            }
            catch (Exception e) {

                ShowMessage($"Socket Error : {e.Message}");
            }
        }

        void ClientStart() {
        
            if (PlayerPrefs.HasKey("IP") && PlayerPrefs.HasKey("Port")) {

                _ipInput.text = PlayerPrefs.GetString("IP");
                _portInput.text = PlayerPrefs.GetInt("Port").ToString();
            }
        }

        void ClientUpdate() {

            if (_socketReady && _stream.DataAvailable) {

                var _data = _reader.ReadLine();
                if (_data != null)
                    OnIncommingData(_data);
            }
        }

        void OnIncommingData(string _data) {

            if (_data == "%NAME") {

                _clientName = _nameInput.text == "" ? "Guest" + UnityEngine.Random.Range(0, 200) : _nameInput.text;
                Send($"&NAME|{_clientName}");
                return;
            }

            ShowMessage(_data);
        }

        void Send(string _data) {

            if (!_socketReady)
                return;

            _writer.WriteLine(_data);
            _writer.Flush();
        }

        public void OnSendButton(InputField _sendInput) {

#if (UNITY_EDITOR || UNITY_STANDALONE)
            if (!Input.GetButtonDown("Submit"))
                return;
            _sendInput.ActivateInputField();
#endif
            if (_sendInput.text.Trim() == "")
                return;

            var _message = _sendInput.text;
            _sendInput.text = "";
            Send(_message);
        }

        public void OnClearButton() {

            _chatText.text = "";
        }

        void OnApplicationQuit() {

            CloseSocket();
        }

        void CloseSocket() {

            if (!_socketReady)
                return;

            _writer.Close();
            _reader.Close();
            _socket.Close();

            PlayerPrefs.SetString("Chat", _chatText.text);
            
            _socketReady = false;
        }

    }
}