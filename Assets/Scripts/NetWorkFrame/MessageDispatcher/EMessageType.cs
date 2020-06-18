using System.Collections;
using UnityEngine;

namespace NetWorkFrame
{
    #region 主要事件 1000-2000
    public enum EModelMessage : uint
    {
        SOCKET_CONNECTED=1000,
        SOCKET_DISCONNECTED,

        TRY_LOGIN,
        REQ_FINISH,
        REQ_TIMEOUT,
        SEND_CHAT_TIMEOUT,
        UPLOAD_FINISH,
        DOWNLOAD_FINISH,
        TEST_RECEIVED,
    }
    #endregion

    #region 界面事件 General 2000 - 2000

    public enum EUIMessage : uint
    {
        UPDATE_FRIEND_DETAIL = 2000,
        UPDATE_SEND_CHAT,
        UPDATE_RECEIVE_CHAT,
        UPDATE_PERSONAL_DETAIL,
        UPDATE_CHAT_LIST,
        TOGGLE_GROUP_MEMBER,
        DELETE_CHAT_ITEM,
    }

    #endregion

}