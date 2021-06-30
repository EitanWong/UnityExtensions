using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dialogue
{
    public struct DialogueInfo
    {
        private DialogueInfo(string characterName, string mainText, string[] optionText,Texture2D mainTexture,Texture2D characterTexture)
        {
            this.CharacterName = characterName;
            this.MainText = mainText;
            this.OptionText = optionText;
            this.MainTexture = mainTexture;
            this.CharacterTexture = characterTexture;
        }

        public string MainText { get;  set; }
        public string CharacterName { get;  set; }
        public string[] OptionText { get; private set; }

        public Texture2D MainTexture { get; set; }
        public Texture2D CharacterTexture { get; set; }
        
        public static DialogueInfo Build(Chat chat)
        {
            var characterName = chat.character ? chat.character.name : string.Empty;
            
            return new DialogueInfo(characterName, chat.GetText(), chat.GetOptionsText(),chat.GetTexture(),chat.character.characterTexture2D);
        }
    }
}