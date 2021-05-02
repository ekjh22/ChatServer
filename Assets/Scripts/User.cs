using JWT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoldServer.Server
{
    public class User {

        public string NickName { get; set; }
    }

    // User
    public partial class Server : MonoBehaviour {

        string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjbGFpbTEiOjAsImNsYWltMiI6ImNsYWltMi12YWx1ZSJ9.8pwBI_HtXqI3UgQHQ_rDRnSQRxFL1SR8fbQoS-5kM5s";
        string secretKey = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

        void JWTStart() {

            if (PlayerPrefs.HasKey("Token")) {

                token = PlayerPrefs.GetString("Token");
                Decode();
            }
        }

        void Encode(ServerClient _client) {

            var payload = new Dictionary<string, object>()
            {
                { "Name", _client.ClientName },
                { "TCP", _client.Tcp }
            };

            token = JsonWebToken.Encode( payload, secretKey, JwtHashAlgorithm.HS256);
            Debug.Log("Encode : " + token);

            PlayerPrefs.SetString("Token", token);
        }

        void Decode() {

            try {

                var result = JsonWebToken.Decode(token, secretKey, false);
                Debug.Log("Decode : " + result);

            } catch (SignatureVerificationException) {

                Debug.Log("Invalid Token");
            }
        }
    }
}