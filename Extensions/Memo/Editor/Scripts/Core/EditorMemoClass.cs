using System;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class EditorMemo : MemoTreeElement {

        public string Date;
        public string Memo;
        public string URL;
        public UnityEditorMemoLabel Label;
        public UnityEditorMemoTexture Tex;
        public MemoObject ObjectRef;
        public bool IsEdit;

        public EditorMemo( string memo, int type, int tex, string url ) {
            Date        = DateTime.Now.RenderDate();
            Memo        = memo;
            Label       = ( UnityEditorMemoLabel )type;
            Tex         = ( UnityEditorMemoTexture )tex;
            ObjectRef   = new MemoObject(null);
            URL         = url;
        }

        public void Initialize( int id ) {
            this.id = id;
            this.name = Memo;
            IsEdit = false;
            ObjectRef.Initialize();
        }

        public static EditorMemo Root {
            get {
                var root = new EditorMemo( "", 0, 0, "" ) {
                    id    = -1,
                    depth = -1,
                    name  = "root"
                };
                return root;
            }
        }

    }

}