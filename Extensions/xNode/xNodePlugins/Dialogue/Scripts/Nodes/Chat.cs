using System.Collections.Generic;
using System.Text;
using Dialogue.Submodules.TextVariable;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using XNode;

namespace Dialogue
{
    [NodeTint("#CCFFCC")]
    public class Chat : DialogueBaseNode
    {
        public CharacterInfo character;
        [TextArea] public string maintext;
        public List<ConditionText> conditionTexts;
        public Texture2D mainTexture;
        public List<ConditionTexture> conditionTextures;

        [Output(instancePortList = true)] public List<Option> options = new List<Option>();

        [System.Serializable]
        public class Option
        {
            public Condition condition;
            [Space] public string text;
            public AudioClip voiceClip;
        }

        [System.Serializable]
        public class ConditionText
        {
            public Condition condition;
            [TextArea] public string text;
        }

        [System.Serializable]
        public class ConditionTexture
        {
            public Condition condition;
            public Texture2D texture;
        }

        public void ChooseOption(int index)
        {
            NodePort port = null;
            if (options.Count == 0)
            {
                port = GetOutputPort("output");
            }
            else
            {
                if (options.Count <= index) return;

                var tmpOP = options[index];
                if (tmpOP.condition.callback.target != null &&
                    tmpOP.condition.callback.Invoke() != tmpOP.condition.value)
                    return;

                port = GetOutputPort(nameof(options) + " " + index);
            }


            if (port == null) return;
            for (int i = 0; i < port.ConnectionCount; i++)
            {
                NodePort connection = port.GetConnection(i);
                (connection.node as DialogueBaseNode).Trigger();
            }
        }

        public override void Trigger()
        {
            (graph as DialogueGraph).current = this;
        }

        public string[] GetOptionsText()
        {
            string[] result = new string[options.Count];

            for (int i = 0; i < result.Length; i++)
            {
                var tmpOp = options[i];
                if (tmpOp.condition.callback.target != null &&
                    tmpOp.condition.callback.Invoke() != tmpOp.condition.value)
                    continue;
                result[i] = TextVariableProcessor.ProcessVariable(tmpOp.text);
            }

            return result;
        }

        public string GetText()
        {
            var chatText = maintext;
            StringBuilder builder = new StringBuilder(chatText);
            foreach (var conText in conditionTexts)
            {
                if (conText.condition.callback.target && conText.condition.callback.Invoke() == conText.condition.value)
                {
                    builder.AppendLine();
                    builder.Append(conText.text);
                }

            }
            chatText = builder.ToString();
            return TextVariableProcessor.ProcessVariable(chatText);
        }

        public Texture2D GetTexture()
        {
            Texture2D result = mainTexture;
            foreach (var conTexture in conditionTextures)
            {
                if (conTexture.condition.callback.target && conTexture.condition.callback.Invoke() == conTexture.condition.value)
                {
                    result = conTexture.texture;
                    break;
                }
            }
            return result;
        }
    }
}