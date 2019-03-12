using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Panda
{

    public class GUIBTScript : System.IDisposable
    {
        internal static GUIBTScript _active = null;
        public static GUIBTScript active { get { return _active; } }

        static List<GUIBTScript> _instances = new List<GUIBTScript>();
        public GUIBTScript(BehaviourTree behaviourTree, TextAsset textAsset, int scriptIndex)
        {
            this._behaviourTree = behaviourTree;
            this._textAsset = textAsset;
            this._scriptIndex = scriptIndex;
            _instances.Add(this);
#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed += OnUndoRedo;
#endif
        }

        int _scriptIndex = -1;
        public int scriptIndex
        {
            get
            {
                return _scriptIndex;
            }
        }

        public enum Mode
        {
            Normal,
            InsertingNewNode,
            EditingNode,
            MouseDrag
        }

        static Mode _modeAfterRepaint = Mode.Normal;
        static Mode _mode = Mode.Normal;
        internal static Mode mode
        {
            get
            {
                if (isEventSafe)
                {
                    _mode = _modeAfterRepaint;
                }
                return _mode;
            }

            set
            {
                _modeAfterRepaint = value;
            }
        }

        BehaviourTree _behaviourTree = null;
        public BehaviourTree behaviourTree { get { return _behaviourTree; } }
        TextAsset _textAsset = null;
        public TextAsset textAsset
        {
            get
            {
                return _textAsset;
            }
        }

        bool _isModified = false;
        public bool isModified
        {
            get
            {
                if (_behaviourTree != null && scriptIndex < _behaviourTree.sourceInfos.Length)
                {
                    var btScript = _behaviourTree.sourceInfos[scriptIndex].btScript;
                    _isModified = btScript != null && btScript != "" && textAsset != null && btScript != textAsset.text;
                    //_isModified = behaviourTree.sourceInfos[scriptIndex].btScript != textAsset.text;
                }
                return _isModified;
            }
        }

        public static bool isAnyNodeEdited
        {
            get
            {
                bool isEdited = false;
                if( active != null)
                {
                    var flines = active.flattenLines;
                    foreach( var line in flines)
                    {
                        foreach( var n in line.nodes)
                        {
                            if( n.isEdited)
                            {
                                isEdited = true;
                                break;
                            }
                        }

                        if (isEdited)
                            break;
                    }
                }

                return isEdited;
            }
        }

        public static bool isEventSafe
        {
            get
            {
                return
                     Event.current  != null  
                    && Event.current.type != EventType.Repaint
                    && Event.current.type != EventType.MouseDown
                    && Event.current.type != EventType.MouseUp
                    && Event.current.type != EventType.MouseDrag
                    && Event.current.type != EventType.MouseMove
                    && Event.current.type != EventType.KeyDown
                    && Event.current.type != EventType.KeyUp
                    ;
            }
        }

        static GUINode _cursorNodeAfterRepaint = null;

        static GUINode _cursorNode = null;
        internal static GUINode cursorNode
        {
            get
            {
                if (GUIBTScript.isEventSafe)
                {
                    _cursorNode = _cursorNodeAfterRepaint;
                }
                return _cursorNode;
            }

            set
            {
                _cursorNodeAfterRepaint = value;
            }
        }


        public static GUIBTScript current = null;
        static GUINode[] _selection;

        static List<GUINode> _selectionList = new List<GUINode>();
        static GUINode _selectionLast = null;
        public static GUINode selectionLast
        {
            get
            {
                return _selectionLast;
            }
        }

        public static bool isCTRLPressed = false;
        public static bool isControlLocked = false;

        const int maxMouseButton = 3;
        static bool[] _isMouseButtonDown = new bool[maxMouseButton];
        public static bool isMouseLeftButtonDown
        {
            get
            {
                return _isMouseButtonDown[0];
            }
        }

        public static bool isMouseRightButtonDown
        {
            get
            {
                return _isMouseButtonDown[1];
            }
        }
        public static Vector2 mouseLastPosition = Vector2.zero;
        public static Vector2 mouseDownPosition = Vector2.zero;
        public static Vector3 mouseDelta = Vector2.zero;

        static bool _isMouseHoverLineNumbers = false;
        static float _isMouseHoverLineNumbers_setTime = float.NegativeInfinity;
        public static bool isMouseHoverLineNumbers
        {
            get
            {
                return _isMouseHoverLineNumbers;
            }

            set
            {
                if (!value && Time.realtimeSinceStartup - _isMouseHoverLineNumbers_setTime < 0.1f)
                    return;

                _isMouseHoverLineNumbers = value;
                _isMouseHoverLineNumbers_setTime = Time.realtimeSinceStartup;
            }
        }

        

        internal static GUILine _hoveredLine = null;
        public static GUILine hoveredLine
        {
            get
            {
                return _hoveredLine;
            }
        }


        public static GUINode[] selection
        {
            get
            {
                if (_selection == null)
                    _selection = selectionByLineNumber;
                return _selection;
            }
        }


        static int CompareNodes( GUINode a, GUINode b)
        {
            int cmp = 0;
            cmp = a.line.number.CompareTo(b.line.number);
            if( cmp == 0)
            {
                cmp = a.indexInLine.CompareTo(b.indexInLine);
            }

            return cmp;
        }

        public static GUINode[] selectionByLineNumber
        {
            get
            {
                _selectionList.RemoveAll((n) => n == null);
                var newList = new List<GUINode>(_selectionList);
                newList.Sort(CompareNodes);
                return newList.ToArray();
            }
        }
        public static GUINode[] selectionByLineNumberReversed
        {
            get
            {
                _selectionList.RemoveAll((n) => n == null);
                var newList = new List<GUINode>(_selectionList);
                newList.Sort(CompareNodes);
                newList.Reverse();
                return newList.ToArray();
            }
        }

        public static void SelectionClear()
        {
            
            foreach (var n in _selectionList)
                n.isSelected = false;

            _selectionList.Clear();
            _selection = null;
        }

        public int SelectionCount()
        {
            return selection.Length;
        }
        public static void SelectionAdd(GUINode node)
        {
            _active = node.line.script;
            if (!_selectionList.Contains(node))
            {
                node.isSelected = true;
                _selectionList.Add(node);
                _selection = null;
                _selectionLast = node;
            }
        }

        public static void SelectionRemove(GUINode node)
        {
            if (_selectionList.Contains(node))
            {
                node.isSelected = false;
                _selectionList.Remove(node);
                _selection = null;
            }
        }

        internal static string[] taskList = null;
        internal static TaskImplementation[] taskImplementations = null;

        static internal readonly string[] structuralNodes = new string[] { "tree", "sequence", "fallback", "parallel", "race", "random", "repeat", "repeat n", "while", "not", "mute" };

        static void PopulateTaskList()
        {
            var tasks = new List<TaskImplementation>(TaskImplementation.Get(active._behaviourTree.gameObject.GetComponents<MonoBehaviour>()));

            tasks.Sort((a, b) => a.ToString().CompareTo(b.ToString()));

            taskImplementations = tasks.ToArray();

            int n = tasks.Count + structuralNodes.Length;
            taskList = new string[n];

            for (int i = 0; i < structuralNodes.Length; i++)
            {
                taskList[i] = "Structural Nodes/" + structuralNodes[i].ToString();
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                taskList[i + structuralNodes.Length] = t.ToString();
            }

        }


        GUILine[] _flattenLines_OnGUI = null;
        public GUILine[] flattenLines_OnGUI
        {
            get
            {
                if( GUIBTScript.isEventSafe )
                {
                    _flattenLines_OnGUI = flattenLines;
                }
                return _flattenLines_OnGUI;
            }

        }



        List<GUILine> lines = new List<GUILine>();
        GUILine[] _flattenLines = null;
        public GUILine[] flattenLines
        {
            get
            {
                GUIBTScript.current = this;

                if (_flattenLines == null)
                {

                    var list = new List<GUILine>();
                    var stack = new Stack<GUILine>();
                    for (int i = lines.Count - 1; i >= 0; i--)
                    {
                        lines[i].depth = 0;
                        stack.Push(lines[i]);
                    }

                    while (stack.Count > 0)
                    {
                        var l = stack.Pop();
                        list.Add(l);
                        for (int i = l.children.Count - 1; i >= 0; i--)
                        {
                            l.children[i].depth = l.depth + 1;
                            stack.Push(l.children[i]);
                        }
                    }


                    _flattenLines = list.ToArray();

                    for (int i = 0; i < _flattenLines.Length; ++i)
                    {
                        _flattenLines[i].number = i+1;
                    }

                    SetDirty();
                }

                return _flattenLines;
            }
        }

        public static void Insert(GUINode node, GUINode.MouseHoverPosition mouseHoverPosition, GUINode cursorNode)
        {
            if (cursorNode == null)
                return;

            SelectionClear();

            SelectionAdd(node);
            InsertSelection(mouseHoverPosition, cursorNode);
            LinesRefresh();
        }

        public static void InsertSelection()
        {
            var mouseHoverPosition = cursorNode.mouseHoverPosition;
            InsertSelection(mouseHoverPosition, cursorNode);
            SetSourcesDirty();
        }

        public static void InsertSelection(GUINode.MouseHoverPosition mouseHoverPosition, GUINode cursorNode)
        {
            if (cursorNode == null || cursorNode.line == null)
                return;

            bool isTaskList = false;

            bool isValidInsert = true;
            if (selection.Length == 0 || cursorNode == null)
                isValidInsert = false;

            foreach (var node in selection)
            {
                if (node == cursorNode)
                {
                    isValidInsert = false;
                    break;
                }

                var parentLine = cursorNode.line.parent;
                while (parentLine != null && node == node.line.nodes[0])
                {
                    if (node.line == parentLine)
                    {
                        isValidInsert = false;
                        break;
                    }
                    parentLine = parentLine.parent;
                }

                if (
                    (mouseHoverPosition == GUINode.MouseHoverPosition.Left || mouseHoverPosition == GUINode.MouseHoverPosition.Right)
                    && node.line.children.Count > 0
                    )
                {
                    isValidInsert = false;
                    break;
                }
            }

            if (isValidInsert)
            {
                GUINode[] nodes = null;
                switch (mouseHoverPosition)
                {
                    case GUINode.MouseHoverPosition.Above:
                    case GUINode.MouseHoverPosition.Center:
                    case GUINode.MouseHoverPosition.Left:
                        nodes = selectionByLineNumber;
                        break;
                    case GUINode.MouseHoverPosition.Right:
                    case GUINode.MouseHoverPosition.Below:
                        nodes = selectionByLineNumberReversed;
                        break;
                }

                if (nodes != null)
                {
                    foreach (var node in nodes)
                        InsertNode(node, mouseHoverPosition, cursorNode);

                    foreach (var node in nodes)
                    {
                        if (node.nodeType == GUINode.NodeType.TaskList)
                        {
                            isTaskList = true;
                        }
                    }
                }
            }

            LinesRefresh();
            SetDirty();

            if( ! isTaskList )
                SetSourcesDirty();

        }

        static void InsertNode(GUINode node, GUINode.MouseHoverPosition mouseHoverPosition, GUINode cursorNode)
        {
            switch (mouseHoverPosition)
            {
                case GUINode.MouseHoverPosition.Above: InsertSelection_AboveBelow(node, true, cursorNode); break;
                case GUINode.MouseHoverPosition.Below: InsertSelection_AboveBelow(node, false, cursorNode); break;
                case GUINode.MouseHoverPosition.Center: InsertSelection_Parent(node, cursorNode); break;
                case GUINode.MouseHoverPosition.Left: InsertSelection_LeftRight(node, 0, cursorNode); break;
                case GUINode.MouseHoverPosition.Right: InsertSelection_LeftRight(node, 1, cursorNode); break;
            }
        }

        public static void InsertSelection_Parent(GUINode node, GUINode cursorNode)
        {

            var srcLine = node.line;
            bool isFirstOnTheLine = node == node.line.nodes[0];

            srcLine.Nodes_Remove(node);
            var srcScript = srcLine.script;

            var newLine = new GUILine();
            newLine.Nodes_Add(node);
            node.line = newLine;

            cursorNode.line.Children_Add(newLine);

            if (isFirstOnTheLine)
            {
                var childrens = srcLine.children.ToArray();
                foreach (var c in childrens)
                {
                    srcLine.Children_Remove(c);
                    newLine.Children_Add(c);
                }
            }


            srcScript.LineRemoveIfEmpty(srcLine);

        }

        public static void InsertSelection_AboveBelow(GUINode node, bool above, GUINode cursorNode)
        {

            var srcLine = node.line;
            var srcScript = srcLine.script;
            var dstScript = cursorNode.line.script;

            bool isFirstOnTheLine = node == node.line.nodes[0];

            if (isFirstOnTheLine)
                srcScript.LineRemove(srcLine);


            var newLine = new GUILine();
            newLine.script = dstScript;
            
            InsertLine(newLine, above, cursorNode);

            if (isFirstOnTheLine)
            {
                var childrens = srcLine.children.ToArray();
                foreach( var n in srcLine.nodes)
                {
                    srcLine.Nodes_Remove(n);
                    newLine.Nodes_Add(n);
                }

                foreach (var c in childrens)
                {
                    srcLine.Children_Remove(c);
                    newLine.Children_Add(c);
                }
            }
            else
            {
                srcLine.Nodes_Remove(node);
                newLine.Nodes_Add(node);
            }

            srcScript.LineRemoveIfEmpty(srcLine);

        }



        public static void InsertSelection_LeftRight(GUINode node, int n, GUINode cursorNode)
        {
            var dstLine = cursorNode.line;
            var srcLine = node.line;
            var scrScript = srcLine.script;
            srcLine.Nodes_Remove(node);
            for (int i = 0; i < dstLine.nodes.Length; i++)
            {
                if (cursorNode == dstLine.nodes[i])
                {
                    dstLine.Nodes_Insert(i + n, node);
                    break;
                }
            }
            node.line = dstLine;

            scrScript.LineRemoveIfEmpty(srcLine);

        }

        public void LineAdd(GUILine line)
        {
            lines.Add(line);
            line.script = this;
            _flattenLines = null;
        }

        public void LineRemove(GUILine line)
        {
            if (line.parent != null)
            {
                line.parent.Children_Remove(line);
            }
            else
            {
                lines.Remove(line);
            }
            line.script = null;
        }

        void LineRemoveIfEmpty(GUILine line)
        {
            if (line.isEmpty)
                LineRemove(line);
            _flattenLines = null;
        }


        static void InsertLine(GUILine newLine, bool above, GUINode cursorNode)
        {
            var currenLine = cursorNode.line;
            var parent = currenLine.parent;
            var dstScript = cursorNode.line.script;
            if (parent != null)
            {
                int insertIndex = 0;
                for (int i = 0; i < parent.children.Count; ++i)
                {
                    insertIndex = i;
                    if (currenLine == parent.children[i])
                        break;
                }
                if (above)
                    parent.Children_Insert(insertIndex, newLine);
                else
                    parent.Children_Insert(insertIndex + 1, newLine);

            }
            else
            {
                int insertIndex = 0;
                for (int i = 0; i < dstScript.lines.Count; ++i)
                {
                    insertIndex = i;
                    if (currenLine == dstScript.lines[i])
                        break;
                }
                if (above)
                    dstScript.lines.Insert(insertIndex, newLine);
                else
                    dstScript.lines.Insert(insertIndex + 1, newLine);

                newLine.parent = null;
            }

            newLine.script._flattenLines = null;
            SetDirty();

        }

        string _lastParsedSource = null;
        public void Parse()
        {
            current = this;
            if (textAsset == null)
                return;

            if ( !( scriptIndex < _behaviourTree.sourceInfos.Length))
                return;

            var btScript = _behaviourTree.sourceInfos[scriptIndex].btScript;
            string source = btScript != null && btScript != "" ? btScript : textAsset.text;

            if (_lastParsedSource == source)
                return;

            var bt = this._behaviourTree;
            if (bt != null)
                bt.Apply();

            _lastParsedSource = source;

            _flattenLines = null;

            lines.Clear();
            _flattenLines_OnGUI = null;


            source = BTLTokenizer.CleanBlanks(source);
            var strLines = source.Split('\n');
            var parents = new Stack<GUILine>();

            int[] depths = new int[strLines.Length];

            // Determine line tree depth
            for (int i = 0; i < strLines.Length; ++i)
            {
                var strLine = strLines[i];
                depths[i] = IndentCount(strLine);
            }

            int lastDepth = 0;
            for (int i = strLines.Length-1; i >= 0; i--)
            {
                var strLine = strLines[i];
                bool isEmptyLineOrStartsWithComment = strLine.Trim() == "" || strLine.Trim().StartsWith("//");

                if( isEmptyLineOrStartsWithComment )
                    depths[i] = lastDepth;
                lastDepth = depths[i];
            }

            for (int i = 0; i < strLines.Length; ++i)
            {
                var strLine = strLines[i];
                var l = new GUILine();
                l.depth = depths[i];
                l.number = i;
                l.content = strLine.Trim();
                l.script = this;

                bool isEmptyLineOrStartsWithComment = l.content.Trim() == "" || l.content.Trim().StartsWith("//");
    
                GUILine parent = null;
                if (parents.Count > 0)
                {
                    parent = parents.Peek();
                    while (parent != null && parent.depth >= l.depth && parents.Count > 0)
                    {
                        parent = null;
                        parents.Pop();
                        if (parents.Count > 0)
                            parent = parents.Peek();
                    }

                }

                if (parent != null)
                {
                    parent.Children_Add(l);
                }
                else
                {
                    lines.Add(l);
                }

                if (!isEmptyLineOrStartsWithComment)
                    parents.Push(l);

            }

            MapNodes();
            InitBreakpoints_CollapsedLines();
            _flattenLines = null;
            _flattenLines_OnGUI = null;
        }

        void MapNodes()
        {
            if (behaviourTree == null || behaviourTree.program == null || behaviourTree.program.codemaps == null || !(scriptIndex < behaviourTree.program.codemaps.Length))
                return;
            var codemap = behaviourTree.program.codemaps[scriptIndex];

            var flines = flattenLines;
            foreach( var line in flines)
            {
                line.btnodes.Clear();
                for ( int i=0; i < codemap.nodes.Length; i++)
                {
                    var btnode = codemap.nodes[i];
                    var loc = codemap.substringLocations[i];
                    if (loc.line == line.number)
                    {
                        line.btnodes.Add(btnode);
                    }
                }
            }
        }

        void InitBreakpoints_CollapsedLines()
        {
#if !PANDA_BT_FREE
            if (behaviourTree == null || behaviourTree.program == null || behaviourTree.program.codemaps == null || !(scriptIndex < behaviourTree.program.codemaps.Length))
                return;

            var sourceInfo = behaviourTree.sourceInfos[scriptIndex];
            var breakPoints = sourceInfo.breakPoints;
            var breakPointStatuses = sourceInfo.breakPointStatuses;
            var collapsedLines = sourceInfo.collapsedLines;

            var flines = flattenLines;

            foreach (var line in flines)
            {
                for (int i = 0; i < breakPoints.Count; i++)
                {
                    var lineNumber = breakPoints[i];
                    var s = breakPointStatuses[i];
                    if( lineNumber == line.number )
                    {
                        line.isBreakPointEnable = true;
                        line.breakPointStatus = s;
                    }
                }

                line.isCollapsed = collapsedLines.Contains(line.number);
            }
#endif
        }

        public int IndentCount(string line)
        {
            int c = 0;
            for (int i = 0; i < line.Length; ++i)
            {
                if (line[i] == '\t')
                    c++;
                else
                    break;
            }

            return c;
        }

        public static void DeleteNode(GUINode node)
        {
            var script = node.line.script;
            var flines = script.flattenLines;

            if (node.nodeType == GUINode.NodeType.EmptyLine)
            {
                script.LineRemoveIfEmpty(node.line);
            }
            else
            {
                foreach (var l in flines)
                {
                    if (l.Nodes_Contains(node))
                    {
                        l.Nodes_Remove(node);
                        node.line = null;
                        if (l.isEmpty)
                        {
                            if (l.parent != null)
                                l.parent.Children_Remove(l);
                            else
                                script.lines.Remove(l);
                        }
                    }
                }
            }
            script._flattenLines = null;
        }

        public override string ToString()
        {
            var source = new System.Text.StringBuilder();
            var flines = flattenLines;
            for(int i=0; i < flines.Length; i++)
            {
                var l = flines[i];
                source.Append(l.ToString());
                if( i + 1 < flines.Length || !l.isEmpty)
                    source.Append(System.Environment.NewLine);
            }

            return source.ToString();
        }

        internal static void SetSourcesDirty()
        {
            RecordUndo();
        }

        internal void ClearBreakpoints()
        {
#if !PANDA_BT_FREE

            if (_behaviourTree != null)
            {
                foreach( var si in  _behaviourTree.sourceInfos )
                {
                    si.breakPoints.Clear();
                    si.breakPointStatuses.Clear();
                }
            }

            var flines = flattenLines;
            foreach( var line in flines)
            {
                line.breakPointStatus = Status.Ready;
            }
#endif
        }


        private static void RecordUndo()
        {
#if UNITY_EDITOR
            var instances = _instances.ToArray();
            foreach( var script in instances)
            {
                if (script._behaviourTree == null)
                {
                    script.Dispose();
                }
            }

            instances = _instances.ToArray();
            foreach (var script in instances)
            {
                var bt = script._behaviourTree;
                if (bt == null)
                    continue;

                UnityEditor.Undo.RecordObject(bt, "BT Script Modification");

                var source = script.ToString();
                var i = script.scriptIndex;

                if ( i < bt.sourceInfos.Length &&  bt.sourceInfos[i].btScript != source )
                {
                    bt.sourceInfos[script.scriptIndex].btScript = source;
                    bt.Apply();
                    bt.Compile();
                }
                script.ClearBreakpoints();
                script.UpdateSourceInfo_CollapsedLines();
            }

            SourceDisplay.RefreshAllBehaviourTreeEditors();
            SetDirty();
#endif
        }

        void UpdateSourceInfo_CollapsedLines()
        {
            if (behaviourTree != null && behaviourTree.sourceInfos != null && scriptIndex < behaviourTree.sourceInfos.Length)
            {
                var list = behaviourTree.sourceInfos[scriptIndex].collapsedLines;
                list.Clear();
                var flines = flattenLines;
                foreach (var line in flines)
                    line.UpdateSourceInfo_CollapsedLines();
            }
        }

        public static void ParseAll()
        {
            var instances = _instances.ToArray();
            foreach (var script in instances)
            {
                script.Parse();
            }
            SourceDisplay.RefreshAllBehaviourTreeEditors();
        }

        private void OnUndoRedo()
        {
            Parse();
        }

        public void OnGUI()
        {
            current = this;
            isControlLocked = false;

            var e = Event.current;

            if (e.isMouse && e.type == EventType.MouseDown)
            {
                _isMouseButtonDown[e.button] = true;
                mouseDownPosition = e.mousePosition;
            }

            if (e.isMouse && e.type == EventType.MouseUp)
                _isMouseButtonDown[e.button] = false;

            if ( e.isMouse )
            {
                mouseDelta = e.mousePosition - mouseLastPosition;
                mouseLastPosition = e.mousePosition;
            }


            isCTRLPressed = e.modifiers == EventModifiers.Control;

            if (current == active)
            {
                if (e.type == EventType.Layout && e.type != EventType.KeyUp && e.keyCode != KeyCode.None && isCTRLPressed && !(e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)) // EventType.layout because it seems e.type == EventType.keyDown doesn work with CTRL pressed.
                {
                    if (e.keyCode == KeyCode.C)
                        Copy();

                    if (e.keyCode == KeyCode.X)
                        Cut();

                    if (e.keyCode == KeyCode.V)
                        Paste(forceAboveInsertion: false);

                    if (e.keyCode == KeyCode.D)
                        Duplicate();

                    /*
                    if (e.keyCode == KeyCode.I)
                        GUIBTEditorMenu.CreateNode();
                    */

                }


                if (e.isKey && e.type == EventType.KeyDown)
                {
                    if (e.keyCode == KeyCode.Delete)
                    {
                        SelectionDelete();
                    }

                    if (e.keyCode == KeyCode.Escape)
                    {
                        var flines = flattenLines_OnGUI;
                        foreach (var l in flines)
                        {
                            var nodes = l.nodes;
                            foreach (var n in nodes)
                            {
                                if (n.nodeType == GUINode.NodeType.TaskList)
                                {
                                    l.Nodes_Remove(n);
                                    LineRemoveIfEmpty(l);
                                }
                            }
                        }
                    }
                }
            }

            if (isMouseRightButtonDown && (mouseLastPosition - mouseDownPosition).magnitude > 2.0f && !isMouseHoverLineNumbers)
            {
                mode = Mode.InsertingNewNode;
                SelectionClear();
            }

            // Popup contextual menu on right click.
            if (mode == Mode.Normal && Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 1  )
            {
#if UNITY_EDITOR

                if (isMouseHoverLineNumbers)
                {
                    UnityEditor.EditorUtility.DisplayPopupMenu(new Rect(mouseLastPosition.x, mouseLastPosition.y, 0, 0), "Panda BT/Break Point", null);
                }
                else
                {
                    if( ! isAnyNodeEdited )
                        UnityEditor.EditorUtility.DisplayPopupMenu(new Rect(mouseLastPosition.x, mouseLastPosition.y, 0, 0), "Panda BT/Edit", null);
                }
#endif
            }

            if ( this == active && e.isMouse && e.button == 1 && e.type == EventType.MouseUp && mode == Mode.InsertingNewNode)
            {
                InsertNodeSelector();
            }

            if (e.isMouse && isMouseLeftButtonDown && cursorNode != null && !cursorNode.isEdited && cursorNode.rect.Contains(mouseDownPosition) )
            {
                mode = Mode.MouseDrag;
            }

            if (e.isMouse && e.button == 0 && e.type == EventType.MouseUp && mode == Mode.MouseDrag)
            {
                if ( !cursorNode.rect.Contains(mouseDownPosition))
                {
                    InsertSelection();
                    isControlLocked = true;
                }
                mode = Mode.Normal;
            }

            switch (GUILayout.Toolbar(-1, new string[] { "-", "+" }, GUILayout.ExpandWidth(false)))
            {
                case 0: CollapseAll(collapsed:true); break;
                case 1: CollapseAll(collapsed:false); break;
            }


            GUILayout.BeginVertical();
            var lines = flattenLines_OnGUI;
            if (lines != null)
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    var line = lines[i];
                    line.OnGUI();
                }
            }
            GUILayout.EndVertical();
            current = null;
        }

        public void CollapseAll( bool collapsed )
        {
            var flines = flattenLines;
            foreach(var l in flines )
            {
                l.isCollapsed = collapsed;
            }
            SetDirty();
        }

        public void Revert()
        {
            RecordUndo();
            _behaviourTree.sourceInfos[scriptIndex].btScript = null;
            _behaviourTree.Apply();
            _lastParsedSource = null;
            Parse();
            SourceDisplay.RefreshAllBehaviourTreeEditors();
        }

        public void SaveToFile()
        {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
            var source = this.ToString();

            // Force refreshing BT Script referencing the same textasset.

            var scripts = _instances.ToArray();
            foreach (var s in scripts)
            {
                if (s == this)
                    continue;

                if (s.textAsset == this.textAsset && !s.isModified)
                {
                    s.behaviourTree.sourceInfos[s.scriptIndex].btScript = null;
                    s.Dispose();
                    _instances.Remove(s);
                }
            }
            

            RecordUndo();
            _behaviourTree.sourceInfos[scriptIndex].btScript = null;
            string path = System.IO.Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets/".Length), UnityEditor.AssetDatabase.GetAssetPath(_textAsset));
            System.IO.File.WriteAllText(path, source);
            UnityEditor.AssetDatabase.Refresh();
            Parse();
#endif
        }

        public void InsertNodeSelector()
        {
            InsertNodeSelector(cursorNode.mouseHoverPosition, cursorNode);
        }

        public static void InsertNodeSelector(GUINode.MouseHoverPosition mouseHoverPosition, GUINode cursorNode)
        {
            PopulateTaskList();

            var dstScript = cursorNode.line.script;

            var taskList = new GUINode(GUINode.NodeType.TaskList);
            var dummyLine = new GUILine();
            dummyLine.Nodes_Add(taskList);
            taskList.line = dummyLine;
            dstScript.LineAdd(dummyLine);

            LinesRefresh();

            Insert(taskList, mouseHoverPosition, cursorNode);

            mode = Mode.Normal;
            SelectionClear();
        }

        public void InsertEmptyLine(GUINode.MouseHoverPosition mouseHoverPosition, GUINode cursorNode)
        {
            var emptyLine = new GUINode(GUINode.NodeType.EmptyLine);
            var dummyLine = new GUILine();
            dummyLine.Nodes_Add(emptyLine);
            emptyLine.line = dummyLine;
            cursorNode.line.script.LineAdd(dummyLine);

            LinesRefresh();

            Insert(emptyLine, mouseHoverPosition, cursorNode);

            mode = Mode.Normal;
            SelectionClear();
        }


        public static void SelectionComment()
        {
            foreach (var s in selection)
                Comment(s);
            SetSourcesDirty();
        }

        public static bool CommentValidate()
        {
            return GUIBTScript.active != null && GUIBTScript.cursorNode.nodeType != GUINode.NodeType.Comment;
        }

        public static void SelectionUncomment()
        {
            foreach (var s in selection)
                Uncomment(s);
            SetSourcesDirty();
        }


        public static void Comment( GUINode node )
        {
            if (node.nodeType != GUINode.NodeType.Comment)
            {
                var sb = new System.Text.StringBuilder();
                int startIndex = node.indexInLine;

                var nodes = node.line.nodes;

                int[] nodeIndices = new int[nodes.Length];
                for (int i = startIndex; i < nodes.Length; i++)
                    nodeIndices[i] = nodes[i].indexInLine;

                for (int i = startIndex; i < nodes.Length; i++)
                {
                    var n = nodes[i];
                    if (i > node.indexInLine)
                        sb.Append(" ");
                    sb.Append(n.ToString());
                    if (i > startIndex)
                    {
                        node.line.Nodes_Remove(n);
                        GUIBTScript.SelectionRemove(node);
                    }
                }

                node.parameters.Clear();
                node.label = "//";
                node.nodeType = GUINode.NodeType.Comment;
                node.Parameters_Add(new GUINodeParameter(typeof(string), sb.ToString()));
                node.SetDirty();
            }
        }

        public static void Uncomment(GUINode node)
        {
            if ( node.nodeType == GUINode.NodeType.Comment )
            {
                var comment = node.parameters[0].value;

                var line = node.line;
                line.Nodes_Remove(node);
                GUIBTScript.SelectionRemove(node);

                var content = line.ToString();
                line.content = content + " " + comment;

                foreach (var n in line.nodes)
                    SelectionAdd(n);
            }

        }


        public void SelectionDelete()
        {
            bool isEdited = false;

            foreach (var s in selection)
            {
                if ( s.isEdited )
                {
                    isEdited = true;
                    break;
                }
            }

            if (!isEdited)
            {
                foreach (var s in selection)
                {
                    DeleteNode(s);
                    if (cursorNode == s)
                        cursorNode = null;
                }
                SelectionClear();
                SetSourcesDirty();
            }
        }

        public static void LinesRefresh()
        {
            foreach (var script in _instances)
                script._flattenLines = null;
        }

        public static void SetDirty()
        {
            foreach (var script in _instances)
            {

                var flines = script.flattenLines;

                foreach (var l in flines)
                    foreach (var n in l.nodes)
                        n.SetDirty();
            }
           
        }

        public void Dispose()
        {
            _instances.Remove(this);
#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed -= OnUndoRedo;
#endif
        }

#region editing
        static GUILine[] copies;
        public static void Copy()
        {
            if (isAnyNodeEdited)
                return;

            var lines = new List<GUILine>();
            foreach (var s in GUIBTScript.selection)
            {
                if (s.indexInLine == 0)
                {
                    if (!lines.Exists((l) => l.number == s.line.number))
                    {
                        var line = s.line.Duplicate();
                        lines.Add(line);
                    }
                }
                else
                {
                    var line = new GUILine();
                    var copy = s.Duplicate();
                    line.Nodes_Add(copy);
                    lines.Add(line);
                }


            }
            copies = lines.ToArray();
        }

        public static void Paste(bool forceAboveInsertion)
        {
            if (isAnyNodeEdited)
                return;

            if (copies == null)
                return;

            var newLines = new List<GUILine>();
            foreach (var l in copies)
            {
                var cl = l.Duplicate();
                newLines.Add(cl);
                GUIBTScript.active.LineAdd(cl);
            }

            var lastSelection = GUIBTScript.selectionLast;
            GUIBTScript.SelectionClear();
            foreach (var l in newLines)
            {
                GUIBTScript.SelectionAdd(l.nodes[0]);
            }
            GUIBTScript.InsertSelection(lastSelection.line != null && lastSelection.line.isEmpty || lastSelection != null && lastSelection.nodeType != GUINode.NodeType.Structural || forceAboveInsertion ? GUINode.MouseHoverPosition.Above : GUINode.MouseHoverPosition.Center, lastSelection);
        }

        public static bool PasteValidate()
        {
            return copies != null && copies.Length > 0;
        }

        public static void Cut()
        {
            if (isAnyNodeEdited)
                return;

            Copy();
            Delete();
        }

        public static void Delete()
        {
            if (isAnyNodeEdited)
                return;

            if (GUIBTScript.active != null)
                GUIBTScript.active.SelectionDelete();
        }

        public static void Duplicate()
        {
            if (isAnyNodeEdited)
                return;

            var saveCopies = copies != null ? new List<GUILine>(copies) : null;
            Copy();
            Paste(forceAboveInsertion: true);

            copies = saveCopies != null ? saveCopies.ToArray() : null;
        }

        public static void CreateNode()
        {
            var lastSelection = GUIBTScript.selectionLast;
            GUIBTScript.InsertNodeSelector(lastSelection.line.isEmpty || lastSelection.nodeType != GUINode.NodeType.Structural ? GUINode.MouseHoverPosition.Above : GUINode.MouseHoverPosition.Center, lastSelection);
        }


        public static bool UncommentValidate()
        {
            return GUIBTScript.active != null && GUIBTScript.cursorNode.nodeType == GUINode.NodeType.Comment;
        }

        public static void EmptyLine()
        {
            var lastSelection = GUIBTScript.selectionLast;
            GUIBTScript.active.InsertEmptyLine(lastSelection.line.isEmpty || lastSelection.nodeType != GUINode.NodeType.Structural ? GUINode.MouseHoverPosition.Above : GUINode.MouseHoverPosition.Center, lastSelection);
        }

        public static void OpenTask()
        {
            var task = GUIBTScript.cursorNode;
            GUIBTScript.OpenScript(task);
        }

        public static bool OpenTaskValidate()
        {
            return GUIBTScript.active != null && GUIBTScript.cursorNode.nodeType == GUINode.NodeType.Task;
        }
#endregion

#region edit_breakpoints
        public static void BreakPoint_Set_Start()
        {
#if !PANDA_BT_FREE

            if (hoveredLine != null)
                hoveredLine.breakPointStatus = Status.Running;
#endif
        }

        public static void BreakPoint_Set_Succeed()
        {
#if !PANDA_BT_FREE
            if (hoveredLine != null)
                hoveredLine.breakPointStatus = Status.Succeeded;
#endif
        }


        public static void BreakPoint_Set_Fail()
        {
#if !PANDA_BT_FREE

            if (hoveredLine != null)
                hoveredLine.breakPointStatus = Status.Failed;
#endif
        }

        public static void BreakPoint_Clear()
        {
#if !PANDA_BT_FREE
            if (hoveredLine != null)
                hoveredLine.breakPointStatus = Status.Ready;
#endif
        }

        public static void BreakPoint_ClearAll()
        {
#if !PANDA_BT_FREE

            if( active != null )
            {
                var flines = active.flattenLines;
                foreach (var line in flines)
                    line.breakPointStatus = Status.Ready;
            }
#endif
        }
#endregion


#region open in external editor helpers
        public static string GetScriptPath(MonoBehaviour monoBehaviour)
        {
            string scriptPath = null;
#if UNITY_EDITOR
            var classname = monoBehaviour.GetType().Name;
            var filter = string.Format("t:script {0}", classname);
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets(filter))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                var end = string.Format("/{0}.cs", classname);
                if (path.EndsWith(end))
                    scriptPath = path;
            }
#endif

            return scriptPath;
        }


        static readonly Dictionary<System.Type, string> typeregex = new Dictionary<System.Type, string>()
            {
                {typeof(System.Boolean), @"\b(bool|(System\.)?Boolean)\b"},
                {typeof(System.Int32)  , @"\b(int|(System\.)?Int32)\b"},
                {typeof(System.Single) , @"\b(float|(System\.)?Single)\b"},
                {typeof(System.String) , @"\b(string|(System\.)?String)\b"},
            };


        static string GetMemberRegexPattern(System.Reflection.MemberInfo member)
        {
            string pattern = "";

            var method = member as System.Reflection.MethodInfo;
            var field = member as System.Reflection.FieldInfo;
            var property = member as System.Reflection.PropertyInfo;

            if (method != null)
            {
                string regparams = @"\s*";
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; ++i)
                {
                    var param = parameters[i];
                    if(typeregex.ContainsKey(param.ParameterType))
                        regparams += typeregex[param.ParameterType];
                    else
                        regparams += "\\b" + param.ParameterType.Name + "\\b";

                    regparams += @"[^,]*";
                    if (i + 1 < parameters.Length)
                    {
                        regparams += @",\s*";
                    }
                }

                pattern = @"\b((Panda\.)?Status|bool|void|Void|(System\.)?Boolean)\b\s+" + method.Name + @"\s*\(" + regparams + @"\)";
            }

            if (field != null)
            {
                pattern = @"\b(bool|(System\.)?Boolean)\b\s+" + field.Name + @"\s*";
            }

            if (property != null)
            {
                pattern = @"\b(bool|(System\.)?Boolean)\b\s+" + property.Name + @"\s*";
            }


            return pattern;
        }

        public static void GetMemberLocation(MonoBehaviour monobeHaviour, System.Reflection.MemberInfo memberInfo, out string path, out int lineNumber)
        {
            path = null;
            lineNumber = -1;
#if UNITY_EDITOR
            path = GetScriptPath(monobeHaviour);
            var text = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;

            var source = text.text;

            var ptrn = GetMemberRegexPattern(memberInfo);

            var regex = new System.Text.RegularExpressions.Regex(ptrn);
            var matches = regex.Matches(source);

            if (matches.Count > 0)
            {
                var pos = matches[0].Index;
                lineNumber = 1;
                for (int i = 0; i < pos; ++i)
                {
                    if (source[i] == '\n')
                        ++lineNumber;
                }
            }
#endif
        }

        public static void OpenScript(MonoBehaviour monobeHaviour, System.Reflection.MemberInfo memberInfo)
        {
#if UNITY_EDITOR
            string path;
            int lineNumber;
            GetMemberLocation(monobeHaviour, memberInfo, out path, out lineNumber);

            var text = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            UnityEditor.AssetDatabase.OpenAsset(text, lineNumber);
#endif
        }

        public static void OpenScript(GUINode task)
        {
            TaskImplementation match = null;

            match = GetTaskImplementation(task);

            if (match != null)
            {
                GUIBTScript.OpenScript(match.target as MonoBehaviour, match.memberInfo);
                GUIBTScript.mode = Mode.Normal;
                for (int i = 0; i < maxMouseButton; i++)
                    _isMouseButtonDown[i] = false;
            }
        }

        public static TaskImplementation GetTaskImplementation(GUINode task)
        {
            TaskImplementation match = null;

            var taskImplementations = TaskImplementation.Get(GUIBTScript.active.behaviourTree.gameObject.GetComponents<MonoBehaviour>());

            foreach (var ti in taskImplementations)
            {
                bool isMatch = true;

                if (ti.memberInfo.Name != task.label) isMatch = false;

                var methodInfo = ti.memberInfo as System.Reflection.MethodInfo;

                if (methodInfo != null)
                {
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length == task.parameters.Count)
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].ParameterType.IsEnum && task.parameters[i].type == typeof(EnumParameter))
                            {
                                EnumParameter enumParameter = new EnumParameter(task.parameters[i].value);
                                isMatch = enumParameter.Matches(parameters[i].ParameterType);
                            }
                            else
                            if (parameters[i].ParameterType != task.parameters[i].type)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isMatch = false;
                    }
                }

                if (isMatch)
                {
                    match = ti;
                    break;
                }
            }

            return match;
        }


        #endregion
    }

}