using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions.Memo {

    internal static class MemoWindowHelper {

        //======================================================================
        // window varies
        //======================================================================

        public static MemoSaveData Data { get; private set; }

        public static readonly Vector2  WINDOW_SIZE          = new Vector2( 350f, 400f );

        public const string WINDOW_TITLE                     = "备忘录";

        public const string TEXT_NO_MEMO                     = "没有备忘录.";
        public const string TEXT_CREATEMEMO_TITLE            = "添加备忘录到 ";
        public const string TEXT_CATEGORY_DESC               = "创建新类别" ;
        public const string TEXT_LABEL_LIST                  = "标签配置";

        public const string WARNING_MEMO_EMPTY               = "备忘录: 备忘录不能为空.";
        public const string WARNING_CATEGORY_EMPTY           = "备忘录: 类别名称为空.";

        public const string UNDO_POST                        = "备忘录 添加";
        public const string UNDO_DELETEPOST                  = "备忘录 删除";
        public const string UNDO_CATEGORYCHANGE              = "备忘录 更改类别";
        public const string UNDO_EDITPOST                    = "备忘录 编辑添加类别";
        public const string UNDO_DRAFT                       = "备忘录 编辑草稿";

        public static readonly string[] MENU_DISPLAY_MEMO    = { "lastest 100", "older" };
        public static readonly string[] POSTMEMO_TYPE        = { "普通", "重要", "问题" };

        public static readonly bool[] FOOTER_TOGGLE          = { true, false, false, false, false, false };

        //======================================================================
        // initialize
        //======================================================================

        public static void LoadData() {
            Data = MemoFileUtility.LoadUnityEditorMemoData();
        }

        public static void OnGUIFirst( float windowWidth ) {
            GUI.skin.label.richText = true;
            GUI.skin.box.richText = true;
        }

        public static void OnGUIEnd() {
            GUI.skin.label.richText = false;
            GUI.skin.box.richText = false;
        }

        //======================================================================
        // public
        //======================================================================

        public static MemoCategory GetCategory( int idx ) {
            if ( Data == null )
                return null;

            if ( idx >= Data.Category.Count )
                idx = 0;
            return Data.Category[idx];
        }

        public static void CheckMemoHasRootElement( int categoryIdx ) {
            var category = GetCategory( categoryIdx );
            if( category != null ) {
                var memo = category.Memo;
                if ( memo.Count == 0 || memo[0].depth != -1 )
                {
                    for ( int i = 0; i < memo.Count; i++ )
                    {
                        memo[ i ].id = i;
                        memo[ i ].depth = 0;
                    }
                    memo.Insert( 0, EditorMemo.Root );
                }
            }
        }

        public static List<EditorMemo> DisplayMemoList( MemoCategory currentCategory, int label ) {
            if ( Data == null )
                return null;

            return currentCategory.Memo.Where( m => label == 0 || m.Label == ( UnityEditorMemoLabel )label ).Reverse().ToList();
        }

        //======================================================================
        // footer toggle area Utility
        //======================================================================

        /// <summary>
        /// custom toggle system
        /// </summary>
        public static int ChangeFooterStatus( int label, ref bool[] footer ) {
            var selectedNum = checkSelectedToggle( ref footer );
            var selected = -1;
            for ( int i = 0; i < footer.Length; i++ ) {
                if( footer[i] ) {
                    if( label == i ) {
                        footer[i] = selectedNum == 2 ? false : true;
                    } else {
                        footer[i] = true;
                        selected = i;
                    }
                } else {
                    if ( label == i && selectedNum == 0 )
                        footer[i] = true;
                }
            }
            if ( selected == -1 )
                selected = label;

            return selected;
        }

        private static int checkSelectedToggle( ref bool[] footer ) {
            var selectedNum = 0;
            for( int i = 0; i < footer.Length; i++ ) {
                if ( footer[i] )
                    selectedNum++;
            }
            if( selectedNum > 2 ) // avoid error
                footer = new bool[] { true, false, false, false, false, false };
            return selectedNum;
        }


    }

}