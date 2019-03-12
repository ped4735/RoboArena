using UnityEngine;
using System.Collections.Generic;


namespace Panda
{
    public class GUILine
    {
        public string _content = "";
        public string content
        {
            get
            {
                return _content;
            }

            set
            {
                _content = value;
                Parse();
            }
        }

        public int _number = 0;
        public int number
        {
            get
            {
                return _number;
            }

            set
            {
                _number = value;
                _lineNumberText = string.Format("{0,5:####0}", _number);
                _controlName = string.Format("Panda BT Line {0}-{1}", GUIBTScript.current.textAsset.GetInstanceID(), _number);
            }
        }
        string _lineNumberText = null;
        string lineNumberText { get { return _lineNumberText; } }

        int _depth = 0;
        public int depth
        {
            get
            {
                return _depth;
            }

            set
            {
                _depth = value;
                _indentString = new string(' ', _depth * 4);
            }
        }

        string _indentString;
        public string indentString { get { return _indentString; } }

        public List<BTNode> btnodes = new List<BTNode>();

        public bool hasError
        {
            get
            {
                bool has = false;
                BTProgram program = null;

                if(this.script != null && this.script.behaviourTree != null) 
                    program =  this.script.behaviourTree.program;

                if (program != null && program.exceptions.Length > 0)
                {
                    foreach (var e in program.exceptions)
                    {
                        var pe = e as PandaScriptException;
                        if (pe != null)
                        {
                            if (pe.lineNumber == this.number && pe.filePath == this.script.behaviourTree.btSources[this.script.scriptIndex].url)
                            {
                                has = true;
                                break;
                            }
                        }
                    }
                }
                return has;
            }
        }

        string _controlName;
        public string controlName
        {
            get
            {
                return _controlName;
            }
        }
#if !PANDA_BT_FREE

        public void ClearBreakPoint()
        {
            isBreakPointEnable = false;
            breakPointStatus = Status.Running;
        }

        public void ToggleBreakPoint()
        {
            if (!isBreakPointEnable)
            {
                isBreakPointEnable = true;
                breakPointStatus = Status.Running;
            }
            else if (breakPointStatus == Status.Running)
            {
                breakPointStatus = Status.Succeeded;
            }
            else if (breakPointStatus == Status.Succeeded)
            {
                breakPointStatus = Status.Failed;
            }
            else if (breakPointStatus == Status.Failed)
            {
                breakPointStatus = Status.Ready;
            }

            if (btnodes.Count == 0)
                isBreakPointEnable = false;
        }

        internal bool _isBreakPointEnable = false;
        public bool isBreakPointEnable
        {
            get
            {
                return _isBreakPointEnable;
            }

            set
            {
                _isBreakPointEnable = value;

                if (btnodes != null)
                {
                    foreach (var n in btnodes)
                    {
                        if (n != null)
                        {
                            if (isBreakPointEnable)
                                n.OnTick += DebugBreak;
                            else
                                n.OnTick -= DebugBreak;
                        }
                    }
                }
                UpdateSourceInfo_BreakPoints();
            }
        }

        Status _breakPointStatus = Status.Ready;

        public Status breakPointStatus
        {
            get
            {
                return _breakPointStatus;
            }

            set
            {
                if (btnodes != null && btnodes.Count > 0)
                    _breakPointStatus = value;
                else
                    _breakPointStatus  = Status.Ready;

                isBreakPointEnable = _breakPointStatus != Status.Ready;
            }
        }

        bool _isBreakPointActive;
        public bool isBreakPointActive
        {
            get
            {
                return _isBreakPointActive;
            }
        }


        void DebugBreak()
        {
            var node = BTNode.currentNode;

            _isBreakPointActive = node != null &&
                (
                   node.previousStatus == Status.Ready && node.status != Status.Ready && breakPointStatus == Status.Running
                || node.status == Status.Succeeded && breakPointStatus == Status.Succeeded
                || node.status == Status.Failed && breakPointStatus == Status.Failed
                );

            if (_isBreakPointActive)
            {
                SourceDisplay.RefreshAllBehaviourTreeEditors();
                Debug.Break();
            }
        }
#endif

        internal GUIBTScript script;

        public bool isCollapsed
        {
            get
            {
                bool itIs = false;

                if (!isEmpty)
                    itIs = nodes[0].isCollapse;
                        
                return itIs;
            }

            set
            {
                if (!isEmpty)
                    nodes[0].isCollapse = value;

                UpdateSourceInfo_CollapsedLines();

            }
        }

        internal void UpdateSourceInfo_CollapsedLines()
        {
            if (script != null && script.behaviourTree != null && script.behaviourTree.sourceInfos != null && script.scriptIndex < script.behaviourTree.sourceInfos.Length)
            {
                var list = script.behaviourTree.sourceInfos[script.scriptIndex].collapsedLines;

                if (isCollapsed)
                {
                    if (!list.Contains(number))
                        list.Add(number);
                }
                else
                {
                    if (list.Contains(number))
                        list.Remove(number);
                }
            }
        }

        public bool isVisible
        {
            get
            {
                bool itIs = true;
                var p = parent;
                while( itIs && p != null )
                {
                    itIs = !p.isCollapsed;
                    p = p.parent;
                }
                return itIs;
            }
        }

        public bool isEmpty
        {
            get
            {
                return nodes.Length == 1 && nodes[0].nodeType == GUINode.NodeType.EmptyLine;
            }
        }

        public GUILine()
        {
            Clear();
        }

        void Clear()
        {
            var nodes = this.nodes;
            foreach (var n in nodes)
                Nodes_Remove(n);
            _nodeList.Clear();
            _nodes = null;

            var node = new GUINode(" ", GUINode.NodeType.EmptyLine);
            Nodes_Add(node);
        }

        public GUINode[] _nodes_OnGUI = null;
        public GUINode[] nodes_OnGUI
        {
            get
            {
                if (_nodes_OnGUI == null || GUIBTScript.isEventSafe)
                {
                    _nodes_OnGUI = _nodes;
                }
                return _nodes_OnGUI;
            }
        }


        public GUINode[] _nodes = null;
        public GUINode[] nodes
        {
            get
            {
                if( _nodes == null)
                {
                    _nodes = _nodeList.ToArray();
                }
                return _nodes;
            }
        }

        public void Nodes_Add(GUINode node)
        {
            if (isEmpty)
            {
                foreach (var n in _nodeList)
                    n.line = null;

                _nodeList.Clear();
            }

            _nodeList.Add(node);
            node.line = this;
            _nodes = null;
        }

        public void Nodes_Remove(GUINode node)
        {
            if (_nodeList.Contains(node))
            {
                _nodeList.Remove(node);

                if (_nodeList.Count == 0)
                    Clear();
                
                _nodes = null;
            }
        }

        public void Nodes_Insert(int index, GUINode node)
        {
            _nodeList.Insert(index, node);
            _nodes = null;
        }

        public bool Nodes_Contains( GUINode node)
        {
            return _nodeList.Contains( node );
        }


        public List<GUINode> _nodeList = new List<GUINode>();
        public List<GUILine> children = new List<GUILine>();
        public GUILine parent = null;


        public void Children_Add(GUILine child)
        {
            children.Add(child);
            child.parent = this;
            child.script = this.script;
        }

        public void Children_Insert(int index, GUILine child)
        {
            children.Insert(index, child);
            child.parent = this;
            child.script = this.script;
        }

        public void Children_Remove(GUILine child)
        {
            if (children.Contains(child))
            {
                children.Remove(child);
                child.parent = null;
                child.script = null;
            }
        }



        void Parse()
        {
            btnodes.Clear();
            Clear();

            BTLTokenizer.Token[] tokens = null;

            try
            {
                tokens = BTLTokenizer.Tokenize(content);
            }
            catch ( PandaScriptException e)
            {
                Debug.LogException(e);
            }

            GUINode node = null;
            if (tokens != null)
            {
                foreach (var t in tokens)
                {
                    if (t.content == null)
                        continue;

                    string trimmed = t.content.Trim();
                    if (trimmed == "")
                        continue;
                    if (trimmed.StartsWith("//"))
                    {
                        node = new GUINode(GUINode.NodeType.Comment);
                        node.label = "//";
                        string comment = trimmed.Substring(2, trimmed.Length - 2);
                        node.Parameters_Add(new GUINodeParameter(typeof(string), comment));
                        Nodes_Add(node);
                    }
                    else
                    if (IsLabel(t))
                    {
                        node = new GUINode(trimmed);
                        Nodes_Add(node);
                        node.line = this;
                    }
                    else
                    if (t.valueType != BTLTokenizer.TokenValueType.None)
                    {
                        if (node != null)
                        {
                            node.Parameters_Add(new GUINodeParameter(t));
                        }
                    }
                }
            }

        }

        bool IsLabel(BTLTokenizer.Token token)
        {
            bool itIs = false;
            switch (token.type)
            {
                case BTLTokenizer.TokenType.Comment:
                case BTLTokenizer.TokenType.Fallback:
                case BTLTokenizer.TokenType.Mute:
                case BTLTokenizer.TokenType.Not:
                case BTLTokenizer.TokenType.Parallel:
                case BTLTokenizer.TokenType.Sequence:
                case BTLTokenizer.TokenType.Race:
                case BTLTokenizer.TokenType.Random:
                case BTLTokenizer.TokenType.Tree:
                case BTLTokenizer.TokenType.Repeat:
                case BTLTokenizer.TokenType.While:
                    itIs = true;
                    break;

                case BTLTokenizer.TokenType.Word:
                    itIs = token.valueType == BTLTokenizer.TokenValueType.None;
                    break;
            }
            return itIs;
        }

        public void OnGUI()
        {
            if (!isVisible)
                return;

            var line = this;
            
            GUILayout.BeginHorizontal();

            OnGUI_LineNumber();
            var prect = GUILayoutUtility.GetLastRect();
            var mpos = Event.current.mousePosition;

            if (prect.Contains(mpos))
            {
                GUIBTScript._hoveredLine = line;
                GUIBTScript.isMouseHoverLineNumbers = true;
                GUIBTScript._active = script;
            }



            if (!line.isEmpty)
            {
                GUILayout.Label(_indentString, BTLSyntaxHighlight.style_label);
                var foldingButton = "    ";
                if (children.Count > 0)
                {
                    if (GUILayout.Button(foldingButton, isCollapsed ? BTLSyntaxHighlight.style_INFoldout : BTLSyntaxHighlight.style_INFoldin))
                    {
                        isCollapsed = !isCollapsed;
                    }
                }
                else
                {
                    GUILayout.Label(foldingButton, BTLSyntaxHighlight.style_comment);
                }
            }
                
            for (int j = 0; j < line.nodes_OnGUI.Length; j++)
            {
                var n = line.nodes_OnGUI[j];
                n.OnGUI();
                if( j + 1 < line.nodes_OnGUI.Length )
                    GUILayout.Label(" ", BTLSyntaxHighlight.style_comment);
            }
            GUILayout.EndHorizontal();
        }

        private void OnGUI_LineNumber()
        {
            var line = this;

            // Determine whether the lines contains a node that has been ticked on this frame.

            // TODO: implement
            bool hasBeenTickedOnThisFrame = false; /* HasBeenTickedOnThisFrame(line)*/;
            //bool containsLeafNode = hasBeenTickedOnThisFrame ? ContainsLeafNodes(line) : false;
            bool containsLeafNode = false;

            var lineNumberStyle = hasError? BTLSyntaxHighlight.style_lineNumber_error : BTLSyntaxHighlight.style_lineNumber;
#if !PANDA_BT_FREE
            bool isActive = hasBeenTickedOnThisFrame && (containsLeafNode || line.isCollapsed) && !line.isBreakPointEnable;
#else
            bool isActive = hasBeenTickedOnThisFrame && (containsLeafNode || line.isCollapsed);
#endif
            isActive = false;

            if (isActive)
                lineNumberStyle = BTLSyntaxHighlight.style_lineNumber_active;
#if !PANDA_BT_FREE
            if (line.isBreakPointEnable)
            {
                if (hasBeenTickedOnThisFrame && /*isPaused &&*/ line.isBreakPointActive )
                {
                    lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_active;
                }
                else
                {
                    switch (line.breakPointStatus)
                    {
                        case Status.Running:
                            lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_running;
                            break;
                        case Status.Succeeded:
                            lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_succeeded;
                            break;
                        case Status.Failed:
                            lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_failed;
                            break;
                        default:
                            lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_running;
                            break;
                    }
                }
            }
#endif
            if (line.hasError)
                lineNumberStyle = BTLSyntaxHighlight.style_lineNumber_error;

            GUI.SetNextControlName(controlName);
#if UNITY_EDITOR && !PANDA_BT_FREE
            if (line.btnodes.Count > 0 && !line.hasError)
            {
                if (GUILayout.Button(lineNumberText, lineNumberStyle))
                {
                    if (Event.current.button == 0)
                        line.ToggleBreakPoint();
                    else if (Event.current.button == 1)
                        line.ClearBreakPoint();
                    line.UpdateSourceInfo_BreakPoints();
                }
            }
            else
            {
                if (line.hasError)
                {
                    if (GUILayout.Button(lineNumberText, lineNumberStyle))
                    {
                        var exceptions = script.behaviourTree.exceptions;
                        foreach( var e in exceptions)
                        {
                            var pe = e as PandaScriptException;
                            if( pe != null && pe.lineNumber == number)
                            {
                                Debug.LogError(pe);
                            }
                        }

                    }
                }
                else
                {
                    GUILayout.Label(lineNumberText, lineNumberStyle);
                }
                
            }
#else
			GUILayout.Label(lineNumberText, lineNumberStyle);
#endif
        }

        private void UpdateSourceInfo_BreakPoints()
        {
#if !PANDA_BT_FREE

            GUILine line = this;
            var sourceInfo = script.behaviourTree.sourceInfos[script.scriptIndex];
            var breakPoints = sourceInfo.breakPoints;
            var breakPointStatuses = sourceInfo.breakPointStatuses;
            bool isDirty = true;
            while (isDirty)
            {
                isDirty = false;
                for (int i = 0; i < breakPoints.Count; ++i)
                {
                    if (breakPoints[i] == line.number)
                    {
                        breakPoints.RemoveAt(i);
                        breakPointStatuses.RemoveAt(i);
                        isDirty = true;
                        break;
                    }
                }
            }

            if (line.isBreakPointEnable)
            {
                breakPoints.Add(line.number);
                breakPointStatuses.Add(line.breakPointStatus);
            }
#endif
        }

        public override string ToString()
        {
            var source =  new System.Text.StringBuilder();
            if(!isEmpty )
            {
                source.Append(new string('\t', depth));
                for( int i=0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    source.Append(node.ToString());
                    if (i + 1 < nodes.Length)
                        source.Append(" ");
                }
            }
            return source.ToString();
        }

        public GUILine Duplicate()
        {
            var copy = new GUILine();
            copy.number = this.number;
            foreach (var n in nodes)
                copy.Nodes_Add(n.Duplicate());

            foreach (var c in children)
                copy.Children_Add(c.Duplicate());

            return copy;
        }
    }

}