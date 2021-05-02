using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GoldServer.Server
{
    // Chat
    public partial class Server : MonoBehaviour {

        [Header("채팅창")]

        [Tooltip("자신의 메세지를 입력하는 오브젝트")]
        [SerializeField] private InputField    _sendInput;
        
        [Tooltip("메시지를 담아두는 콘텐트 오브젝트")]
        [SerializeField] private RectTransform _chatContent;

        [Tooltip("입력한 텍스트를 써주는 오브젝트")]
        [SerializeField] private Text          _chatText;

        [Tooltip("채팅창 스크롤을 담당하는 렉트 오브젝트")]
        [SerializeField] private ScrollRect    _chatScrollRect;

        /// <summary>
        /// 채팅창에 채팅내역을 써주는 함수
        /// </summary>
        /// <param name="_data">채팅창에 쓰여질 내용</param>
        void ShowMessage(string _data) {

            Debug.Log(_data);

            _chatText.text += _chatText.text == "" ? _data : "\n" + _data;

            Fit(_chatText.GetComponent<RectTransform>());
            Fit(_chatContent);

            Task.Run(() => ScrollDelay(30));
        }

        void Fit(RectTransform _rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);

        async void ScrollDelay(int _duration) {

            await Task.Delay(_duration);
            await Task.Run(() => _chatScrollRect.verticalScrollbar.value = 0);
        }
    }
}
