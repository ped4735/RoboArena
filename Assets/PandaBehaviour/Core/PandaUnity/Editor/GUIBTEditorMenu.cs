#if ! PANDA_BT_FREE
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace Panda
{
    public class GUIBTEditorMenu
    {


        [MenuItem("Panda BT/Edit/Copy", false, 10)]
        public static void Copy()
        {
            GUIBTScript.Copy();
        }

        [MenuItem("Panda BT/Edit/Cut", false, 10)]
        public static void Cut()
        {
            GUIBTScript.Cut();
        }

        const string menu_paste = "Panda BT/Edit/Paste";
        [MenuItem(menu_paste, false, 11)]
        public static void Paste()
        {
            GUIBTScript.Paste(forceAboveInsertion:false);
        }
        


        [MenuItem(menu_paste, true)]
        public static bool PasteValidate()
        {
            return GUIBTScript.PasteValidate();
        }

        [MenuItem("Panda BT/Edit/Duplicate", false, 30)]
        public static void Duplicate()
        {
            GUIBTScript.Duplicate();
        }

        [MenuItem("Panda BT/Edit/Delete", false, 31)]
        public static void Delete()
        {
            GUIBTScript.Delete();
        }

        [MenuItem("Panda BT/Edit/Delete", true)]
        public static bool DeleteNodeValidate()
        {
            return GUIBTScript.active != null && GUIBTScript.active.SelectionCount() > 0;
        }

        [MenuItem("Panda BT/Edit/Create Node", false, 50)]
        public static void CreateNode()
        {
            GUIBTScript.CreateNode();
        }

        const string menu_comment = "Panda BT/Edit/Comment";
        [MenuItem(menu_comment, false, 70)]
        public static void Comment()
        {
            GUIBTScript.SelectionComment();
        }

        [MenuItem(menu_comment, true)]
        public static bool CommentValidate()
        {
            return GUIBTScript.CommentValidate();
        }

        const string menu_uncomment = "Panda BT/Edit/Uncomment";
        [MenuItem(menu_uncomment, false, 71)]
        public static void Uncomment()
        {
            GUIBTScript.SelectionUncomment();
        }
        [MenuItem(menu_uncomment, true)]
        public static bool UncommentValidate()
        {
            return GUIBTScript.UncommentValidate();
        }

        [MenuItem("Panda BT/Edit/Empty Line", false, 72)]
        public static void EmptyLine()
        {
            GUIBTScript.EmptyLine();
        }


        const string open_implementation_menu = "Panda BT/Edit/Open C# implementation";
        [MenuItem(open_implementation_menu, false, 80)]
        public static void Open()
        {
            GUIBTScript.OpenTask();
        }


        [MenuItem(open_implementation_menu, true)]
        public static bool OpenValidate()
        {
            return GUIBTScript.OpenTaskValidate();
        }


        [MenuItem("Panda BT/Break Point/Break On Start", false, 72)]
        public static void Break_PointSet_Start()
        {
            GUIBTScript.BreakPoint_Set_Start();
        }

        [MenuItem("Panda BT/Break Point/Break On Succeed", false, 72)]
        public static void Break_PointSet_Succeed()
        {
            GUIBTScript.BreakPoint_Set_Succeed();
        }

        [MenuItem("Panda BT/Break Point/Break On Fail", false, 72)]
        public static void Break_PointSet_Fail()
        {
            GUIBTScript.BreakPoint_Set_Fail();
        }


        [MenuItem("Panda BT/Break Point/Clear", false, 72)]
        public static void Break_PointSet_Clear()
        {
            GUIBTScript.BreakPoint_Clear();
        }

        [MenuItem("Panda BT/Break Point/Clear All", false, 72)]
        public static void Break_PointSet_ClearAll()
        {
            GUIBTScript.BreakPoint_ClearAll();
        }


    }
}
#endif
