using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

namespace UnityExtensions.Memo {

    internal static class MemoSlackHelper {

        private const string APIURL = @"https://slack.com/api/chat.postMessage?token={0}&channel={1}&text={2}&attachments=[{3},{4}]";
        private readonly static string[] FaceEmoji = new string[] { "", ":slightly_smiling_face:", ":rage:", ":sweat:" };

        public static bool Post( EditorMemo memo, string categoryName ) {
            var token = EditorMemoPrefs.UnityEditorMemoSlackAccessToken;
            var channel = EditorMemoPrefs.UnityEditorMemoSlackChannel;
            if( string.IsNullOrEmpty( token ) ) {
                Debug.LogWarning( "备忘录: 您必须设置访问令牌." );
                return false;
            }
            if( string.IsNullOrEmpty( channel ) ) {
                Debug.LogWarning( "备忘录: 您必须设置访问令牌." );
                return false;
            }

            var text = memo.Memo;
            if( !string.IsNullOrEmpty( memo.URL ) )
                text += string.Format( "\n<{0}|URL>", memo.URL );

            var titleAttachment = new Attachment {
                title = string.Format( "【{0}】 - {1} {2}\n", PlayerSettings.productName, categoryName, FaceEmoji[ ( int )memo.Tex ] ),
                color = "FFFFFF",
            };
            var memoAttachment = new Attachment {
                color  = ColorUtility.ToHtmlStringRGB( MemoGUIHelper.Colors.LabelColor( memo.Label ) ),
                text   = text,
                footer = memo.Date,
            };

            var url = string.Format( APIURL, token, channel, "", JsonUtility.ToJson( titleAttachment ), JsonUtility.ToJson( memoAttachment ) );
            var post = postCo( url );
            while( post.MoveNext() ) { }
            return ( bool )post.Current;
        }

        private static IEnumerator postCo( string url ) {
            var req = UnityWebRequest.Get( url );
#if UNITY_2017_2_OR_NEWER
            yield return req.SendWebRequest();
#else
            yield return req.Send();
#endif

            //Debug.Log( url );
            if( !string.IsNullOrEmpty( req.error ) ) {
                EditorUtility.DisplayDialog( "备忘录", "无法将备忘录添加到 Slack.\n" + req.error, "确定" );
                yield return false;
            } else {
                yield return true;
            }

        }

        [Serializable]
        public class Attachment {
            public string color;
            public string title;
            public string text;
            public string footer;
        }

    }

}