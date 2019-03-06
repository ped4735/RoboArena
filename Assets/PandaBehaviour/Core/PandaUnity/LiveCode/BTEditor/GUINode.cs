using UnityEngine;
using System.Collections.Generic;


namespace Panda
{
    public class GUINode
    {

        public GUINode()
        {

        }

        internal static GUINode _current = null;
        internal static int _currentParameterIndex = -1;
        
        public GUINode(NodeType type)
        {
            nodeType = type;
        }

        public GUINode(string label, NodeType type)
        {
            this.label = label.Trim();
            this.nodeType = type;
        }

        public GUINode(string label)
        {
            this.label = label.Trim();
            DefineType();
        }

        public GUINode(string label, System.Type[] types)
        {
            Promote(label, types);
        }


        public GUINode(TaskImplementation implementation)
        {
            Promote(implementation);
        }

        public int indexInLine
        {
            get
            {
                int index = -1;
                if (line != null)
                {
                    for (int i = 0; i < line.nodes.Length; i++)
                    {
                        if (line.nodes[i] == this)
                        {
                            index = i;
                            break;
                        }
                    }
                }
                return index;
            }
        }

        public void Promote(TaskImplementation implementation)
        {
            label = implementation.memberInfo.Name;
            parameters.Clear();
            foreach (var p in implementation.parameterTypes)
            {
                Parameters_Add(new GUINodeParameter(p));
            }
            DefineType();
        }

        public void Promote(string label, System.Type[] types)
        {
            this.label = label;
            parameters.Clear();
            foreach (var p in types)
            {
                Parameters_Add(new GUINodeParameter(p));
            }
            DefineType();
        }


        public enum MouseHoverPosition
        {
            Above,
            Below,
            Left,
            Right,
            Center,
            None
        }

        public enum NodeType
        {
            Structural,
            Task,
            TaskList,
            Comment,
            EmptyLine
        }

        NodeType _nodeType;
        internal NodeType nodeType
        {
            get
            {
                
                if ( line != null && label != null && label.ToLower() == "tree")
                    _nodeType = line.depth == 0 && this.indexInLine == 0 ? NodeType.Structural : NodeType.Task;

                return _nodeType;
            }

            set
            {
                _nodeType = value;
            }
        }

        void DefineType()
        {
            nodeType = NodeType.Task;
            foreach (var s in GUIBTScript.structuralNodes)
            {
                if (label.ToLower() == s)
                {
                    nodeType = NodeType.Structural;
                    break;
                }
            }

            if (label == "tree" && _line != null && _line.parent == null)
            {
                nodeType = NodeType.Task;
            }

            if (label.Trim().StartsWith("//"))
            {
                label = label.Trim();
                nodeType = NodeType.Comment;
            }
            isRectDirty = true;
        }


        internal bool _isEditedPrev = false;
        internal bool isEdited
        {
            get
            {
                if (GUIBTScript.isEventSafe)
                {
                    bool itIs = false;

                    foreach (var p in parameters)
                        if (p.isEdited)
                            itIs = true;
                    _isEditedPrev = itIs;
                }
                return _isEditedPrev;

            }
        }

        internal bool _isMouseOver = false;
        internal bool _isMouseOverAfterRepaint = false;
        internal bool isMouseOver
        {
            get
            {
                if (GUIBTScript.isEventSafe)
                {
                    _isMouseOver = _isMouseOverAfterRepaint;
                }
                return _isMouseOver;
            }

            set
            {
                _isMouseOverAfterRepaint = value;
            }
        }

        bool _isRectDirty = true;
        bool _isRectDirtyAfterRepaint = true;
        bool isRectDirty
        {
            get
            {
                if (GUIBTScript.isEventSafe)
                    _isRectDirty = _isRectDirtyAfterRepaint;
                return _isRectDirty;
            }
            set
            {
                _isRectDirtyAfterRepaint = value;
            }
        }


        public void SetDirty()
        {
            isRectDirty = true;
        }
        internal Rect rect;
        Rect[] parameterRects = null;
        public string label;

        int _controlName_lineNumber = -1;
        int _controlName_lineIndex = -1;
        string _controlName = null;
                public string controlName
        {
            get
            {
                var lineNumber = this.line != null ? this.line.number : -1;

                bool hasChanged = lineNumber != _controlName_lineNumber || _controlName_lineIndex != this.indexInLine;
                if( hasChanged )
                {
                    _controlName_lineNumber = lineNumber;
                    _controlName_lineIndex = this.indexInLine;
                    var instanceId = GUIBTScript.current != null && GUIBTScript.current.textAsset != null? GUIBTScript.current.textAsset.GetInstanceID(): -1;
                    _controlName = string.Format("Panda BT {0}-{1}-{2}", instanceId, lineNumber, this.indexInLine);
                }
                
                return _controlName ;
            }
        }
        public List<GUINodeParameter> parameters = new List<GUINodeParameter>();

        public bool _isSelectedAfterRepaint = false;
        public bool _isSelected = false;
        public bool isSelected
        {
            get
            {
                if (GUIBTScript.isEventSafe)
                {
                    _isSelected = _isSelectedAfterRepaint;
                }

                return _isSelected;
            }

            set
            {
                _isSelectedAfterRepaint = value;
            }
        }
        internal MouseHoverPosition mouseHoverPosition = MouseHoverPosition.None;
        internal GUILine _line;
        internal GUILine line
        {
            get
            {
                return _line;
            }
            set
            {
                _line = value;
                if (label == "tree" && _line != null)
                {
                    if ( !_line.isEmpty && _line.nodes[0] == this && _line.parent == null)
                        nodeType = NodeType.Structural;
                    else
                        nodeType = NodeType.Task;
                }

                isRectDirty = true;

            }
        }

        float lastClickTime = float.NegativeInfinity;
        public void OnGUI()
        {
            var node = this;
            GUINode._current = this;

            // Recompute rects when edit state has changed.
            if (GUIBTScript.isEventSafe)
            {
                if (isEdited == false && _isEditedPrev)
                    SetDirty();

                if (isEdited && _isEditedPrev == false)
                    GUIBTScript.SelectionClear();

                _isEditedPrev = isEdited;
            }

            if (node.nodeType == NodeType.TaskList)
            {
                OnGUI_TaskListSelector();
            }
            else
            {
                if ((node.isSelected || node == GUIBTScript.cursorNode) && !isRectDirty && !isEdited)
                    OnGUI_OneBlock();
                else
                    OnGUI_ColoredNode();
            }

            // Edit parameter when left mouse click.
            bool isClicked = Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            var mpos = Event.current.mousePosition;
            
            if (!isRectDirty && !GUIBTScript.isCTRLPressed && isClicked && GUIBTScript.mode == GUIBTScript.Mode.Normal )
            {
                bool isParameterClicked = false;
                for (int i = 0; i < parameters.Count; i++)
                {
                    var r = parameterRects[i];
                    var isWithinRect = r.Contains(mpos);
                    parameters[i].isEdited = isWithinRect;

                    if (isWithinRect)
                        isParameterClicked = true;

                }

                if (this.rect.Contains(mpos) && !isParameterClicked)
                {
                    bool isDoubleClicked = Time.realtimeSinceStartup - lastClickTime < 0.4f;
                    lastClickTime = Time.realtimeSinceStartup;
                    if (isDoubleClicked && this.nodeType == NodeType.Task && this.label.ToLower() != "tree")
                        GUIBTScript.OpenScript(this);
                }

            }

            // On press Enter or Esc stop editing the parameter.
            if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
            {
                foreach (var p in parameters)
                {
                    if (p.isEdited)
                        p.isEdited = false;
                }
            }

            // Locate mouse position in rect.
            if (Event.current.type == EventType.Repaint)
            {
                if (rect.Contains(mpos))
                {
                    isMouseOver = true;

                    if (GUIBTScript.cursorNode != node)
                    {
                        GUIBTScript.cursorNode = node;
                        //GUIBTScript.current.redraw = true;
                    }

                    float f = 0.3f;
                    var w = rect.width;
                    var wb = w * f;
                    var h = rect.height;
                    var hb = h * f;

                    if (mpos.y < rect.y + hb)
                        mouseHoverPosition = MouseHoverPosition.Above;
                    else
                    if (mpos.y > rect.y + h - hb && node.line.children.Count == 0)
                        mouseHoverPosition = MouseHoverPosition.Below;
                    else
                    if (mpos.x < rect.x + wb)
                        mouseHoverPosition = MouseHoverPosition.Left;
                    else
                    if (mpos.x > rect.x + w - wb)
                        mouseHoverPosition = MouseHoverPosition.Right;
                    else
                    if (nodeType == NodeType.Structural && node.line.nodes[0] == node)
                        mouseHoverPosition = MouseHoverPosition.Center;

                }
                else
                {
                    isMouseOver = false;
                }
            }
            else if (node != GUIBTScript.cursorNode)
            {
                mouseHoverPosition = MouseHoverPosition.None;
            }
        }

        int taskIndex = -1;
        private void OnGUI_TaskListSelector()
        {
#if UNITY_EDITOR
            var taskList = GUIBTScript.taskList;
            UnityEditor.EditorGUI.BeginChangeCheck();
            taskIndex = UnityEditor.EditorGUILayout.Popup(taskIndex, taskList, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(80.0f));
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {

                if (taskIndex >= GUIBTScript.structuralNodes.Length)
                {
                    var ti = GUIBTScript.taskImplementations[taskIndex - GUIBTScript.structuralNodes.Length];
                    this.Promote(ti);
                }
                else
                {
                    var option = taskList[taskIndex];
                    var lastSlash = option.LastIndexOf('/');
                    var w = option.Length - lastSlash - 1;
                    var name = option.Substring(lastSlash + 1, w);

                    string label = null;
                    System.Type[] types = null;
                    if (name == "tree")
                    {
                        label = "tree"; types = new System.Type[] { typeof(string) };
                    }
                    else
                    if (name == "repeat n")
                    {
                        label = "repeat"; types = new System.Type[] { typeof(int) };
                    }
                    else
                    {
                        label = name; types = new System.Type[0];
                    }
                    this.Promote(label, types);
                }
                GUIBTScript.SetSourcesDirty();
            }
#endif
        }

        void OnGUI_ColoredNode()
        {
            GUINode node = this;

            GUI.SetNextControlName(node.controlName);

            GUIStyle style = null;

            switch (nodeType)
            {
                case NodeType.Comment: style = BTLSyntaxHighlight.style_comment; break;
                case NodeType.Structural: style = BTLSyntaxHighlight.style_keyword; break;
                case NodeType.Task: style = label == "tree" ? BTLSyntaxHighlight.style_keyword : BTLSyntaxHighlight.style_task; break;
                case NodeType.EmptyLine: style = BTLSyntaxHighlight.style_task; break;
            }


            if (node.nodeType == NodeType.EmptyLine)
            {
                GUILayout.Label(node.label, style, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(node.label, style);
            }

            if (isRectDirty && Event.current.type == EventType.Repaint)
            {
                rect = GUILayoutUtility.GetLastRect();
                var pRects = new List<Rect>();

                float width = rect.width;
                bool withParenthesis = parameters.Count > 0 && nodeType != NodeType.Comment;
                if (withParenthesis)
                {
                    GUILayout.Label("(", BTLSyntaxHighlight.style_label);
                    width += GUILayoutUtility.GetLastRect().width;
                }

                for( int i=0; i < node.parameters.Count; i++)
                {
                    var p = node.parameters[i];
                    GUINode._currentParameterIndex = i;

                    p.OnGUI( nodeType == NodeType.Comment);
                    p.rect = GUILayoutUtility.GetLastRect();
                    pRects.Add(p.rect);
                    width += p.rect.width;

                    if (withParenthesis && i+1 < node.parameters.Count)
                    {
                        GUILayout.Label(", ", BTLSyntaxHighlight.style_label);
                        width += GUILayoutUtility.GetLastRect().width;
                    }
                }

                if (withParenthesis)
                {
                    GUILayout.Label(")", BTLSyntaxHighlight.style_label);
                    width += GUILayoutUtility.GetLastRect().width;
                }

                rect.width = width;
                parameterRects = pRects.ToArray();
                isRectDirty = false;
            }
            else
            {

                bool withParenthesis = parameters.Count > 0 && nodeType != NodeType.Comment;

                if (withParenthesis)
                    GUILayout.Label("(", BTLSyntaxHighlight.style_label);

                for (int i = 0; i < node.parameters.Count; i++)
                {
                    var p = node.parameters[i];
                    GUINode._currentParameterIndex = i;
                    p.OnGUI(nodeType == NodeType.Comment);
                    if (withParenthesis && i + 1 < node.parameters.Count)
                        GUILayout.Label(", ", BTLSyntaxHighlight.style_label);

                }

                if (withParenthesis)
                    GUILayout.Label(")", BTLSyntaxHighlight.style_label);

            }
        }

        void OnGUI_OneBlock()
        {
            GUINode node = this;
            var style = new GUIStyle(node.isSelected ? BTLSyntaxHighlight.style_selected : BTLSyntaxHighlight.style_running);
            //GUIStyle style = new GUIStyle();

            bool displayInsert = false;
            if (!node.isSelected && (GUIBTScript.mode == GUIBTScript.Mode.MouseDrag || GUIBTScript.mode == GUIBTScript.Mode.InsertingNewNode))
            {
                switch (node.mouseHoverPosition)
                {
                    case MouseHoverPosition.Above: style = BTLSyntaxHighlight.style_insert_above; break;
                    case MouseHoverPosition.Below: style = BTLSyntaxHighlight.style_insert_below; break;
                    case MouseHoverPosition.Center: style = BTLSyntaxHighlight.style_insert_in; break;
                }
                displayInsert = true;
            }

            var sbLabel = new System.Text.StringBuilder();

            if (displayInsert && mouseHoverPosition == MouseHoverPosition.Left)
            {
                sbLabel.Append("|");
                //GUILayout.Label(BTLSyntaxHighlight.texture_insert_LeftRigth, GUILayout.ExpandWidth(false));
            }

            sbLabel.Append(node.label);


            bool withParenthesis = parameters.Count > 0 && nodeType != NodeType.Comment;

            if (withParenthesis)
                sbLabel.Append("(");

            for (int i = 0; i < node.parameters.Count; i++)
            {
                var p = node.parameters[i];
                if (nodeType == NodeType.Comment && p.value.Trim() == "")
                {
                    sbLabel.Append("...");
                }
                else
                {
                    sbLabel.Append(p.value);

                    if (withParenthesis && i + 1 < node.parameters.Count)
                        sbLabel.Append(", ");
                }
            }

            if (withParenthesis)
                sbLabel.Append(")");


            if (displayInsert && mouseHoverPosition == MouseHoverPosition.Right)
            {
                sbLabel.Append("|");
                //GUILayout.Label(BTLSyntaxHighlight.texture_insert_LeftRigth, GUILayout.ExpandWidth(false));
            }

            GUI.SetNextControlName(node.controlName);
            if (node.nodeType == NodeType.EmptyLine)
            {
                GUILayout.Label(node.label, style, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(sbLabel.ToString(), style);
            }

            
            // Selection and drag start
            if ( isMouseOver && Event.current.isMouse &&  Event.current.type == EventType.MouseDown )
            {
                if (GUIBTScript.isCTRLPressed)
                {
                    if( this.isSelected )
                        GUIBTScript.SelectionRemove(node);
                    else
                        GUIBTScript.SelectionAdd(node);
                }
                else
                {
                    if( !isSelected)
                        GUIBTScript.SelectionClear();
                    GUIBTScript.SelectionAdd(node);
                }

                GUI.FocusControl(node.controlName);

            }

            if(isMouseOver && Event.current.isMouse && Event.current.type == EventType.MouseUp && !GUIBTScript.isCTRLPressed && !GUIBTScript.isControlLocked)
            {

                GUIBTScript.SelectionClear();
                GUIBTScript.SelectionAdd(node);
                GUI.FocusControl(node.controlName);
            }
        }

        public override string ToString()
        {
            var source = new System.Text.StringBuilder();
            if( this.label != null)
                source.Append(this.label);

            if ( nodeType != NodeType.Comment && parameters.Count > 0)
                source.Append("(");

            for( int i=0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var v = p.value.Trim();
                source.Append( v );

                if (i + 1 < parameters.Count)
                    source.Append(", ");
            }

            if (nodeType != NodeType.Comment && parameters.Count > 0)
                source.Append(")");



            return source.ToString();
        }

        bool _isCollapsed = false;
        public bool isCollapse
        {
            set
            {
                if (_isCollapsed != value)
                {
                    GUIBTScript.SetDirty();
                }

                _isCollapsed = value;
            }

            get
            {
                if( _isCollapsed && line != null  && line.nodes[0] != this )
                {
                    _isCollapsed = false;
                    GUIBTScript.SetDirty();
                }
                return _isCollapsed;
            }
        }

        public GUINode Duplicate()
        {
            GUINode copy = new GUINode();
            copy.label = this.label;
            copy.nodeType = this.nodeType;

            foreach( var p in this.parameters )
            {
                var pcopy = p.Duplicate();
                copy.Parameters_Add(pcopy);
              
            }

            return copy;
        }

        public void Parameters_Add(GUINodeParameter parameter)
        {
            parameter.OnChange += OnParameterChange;
            this.parameters.Add(parameter);
        }

        void OnParameterChange()
        {
            GUIBTScript.SetSourcesDirty();
        }

    }

}