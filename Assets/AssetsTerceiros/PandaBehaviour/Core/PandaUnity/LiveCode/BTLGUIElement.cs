/*
Copyright (c) 2015 Eric Begue (ericbeg@gmail.com)

This source file is part of the Panda BT package, which is licensed under
the Unity's standard Unity Asset Store End User License Agreement ("Unity-EULA").

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Panda
{


    public class SourceDisplay : System.IDisposable
    {
        public GUISkin skin;
        public BTLGUILine[] lines;
        public BTLGUILine[] orphans;
        public BehaviourTree bt;
        public int scriptIndex;
        public static System.Action refreshAllBehaviourTreeEditors = null;

        public static void RefreshAllBehaviourTreeEditors()
        {
            if (SourceDisplay.refreshAllBehaviourTreeEditors != null)
                SourceDisplay.refreshAllBehaviourTreeEditors();
        }

        public SourceDisplay(int scriptIndex)
        {
            this.scriptIndex = scriptIndex;
            InitRendering();
        }

        public void Dispose()
        {
            var lines = this.flattenLines;

            foreach(var l in lines)
                l.Dispose();

            orphans = null; lines = null;

        }



        public static SourceDisplay current;
        public static int currentSourceIndex;


        public void SetIsFoldoutAll(bool isFoldout)
        {
             SetIsFoldoutAll(this.lines, isFoldout);
             SetIsFoldoutAll(this.orphans, isFoldout);
        }

        void SetIsFoldoutAll(BTLGUILine[] lines, bool isFoldout)
        {
            foreach (var line in flattenLines)
                line.isFoldout = isFoldout;
        }

        public BTLGUILine[] flattenLines
        {
            get
            {
                var lineList = new List<BTLGUILine>();

                var stack = new Stack<BTLGUILine>();

                for (int i = lines.Length - 1; i >= 0; i--)
                    stack.Push(lines[i]);

                while (stack.Count > 0)
                {
                    var line = stack.Pop();
                    lineList.Add(line);
                    for (int i = line.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(line.children[i]);
                    }
                }

                lineList.AddRange(orphans);

                lineList.Sort((a, b) => a.lineNumber.CompareTo(b.lineNumber));

                return lineList.ToArray();
            }
        }

        #region code rendering
        BTTask clickedTask;
        float clickedTaskFade;

        void InitRendering()
        {
            clickedTask = null;
            clickedTaskFade = Time.realtimeSinceStartup - 1.0f;
        }

        public void DisplayCode()
        {
#if UNITY_EDITOR
            isPaused = UnityEditor.EditorApplication.isPaused;
            isPlaying = UnityEditor.EditorApplication.isPlaying;
#endif

            SourceDisplay sourceDisplay = this;

            if (sourceDisplay != null)
            {
                SourceDisplay.current = sourceDisplay;
                SourceDisplay.currentSourceIndex = this.scriptIndex;
                switch (GUILayout.Toolbar(-1, new string[] { "-", "+" }, GUILayout.ExpandWidth(false)))
                {
                    case 0: sourceDisplay.SetIsFoldoutAll(true); break;
                    case 1: sourceDisplay.SetIsFoldoutAll(false); break;
                }
                RenderLines(sourceDisplay);
            }

        }

        private void RenderLines(SourceDisplay sourceDisplay )
        {
            var stack = new Stack<BTLGUILine>();
            var lines = sourceDisplay.lines;
            var orphans = sourceDisplay.orphans;
            for (int i = lines.Length - 1; i >= 0; i--)
                stack.Push(lines[i]);

            int orphanIdx = 0;
            int lastRenderedLine = 0;


            GUILayout.BeginVertical();
            while (stack.Count > 0)
            {
                var line = stack.Pop();
                while (orphanIdx < orphans.Length
                     && lastRenderedLine < orphans[orphanIdx].lineNumber && orphans[orphanIdx].lineNumber < line.lineNumber)
                {
                    RenderLine(orphans[orphanIdx]);
                    if (!orphans[orphanIdx].isFoldout)
                        foreach (var c in orphans[orphanIdx].children)
                            RenderLine(c);

                    ++orphanIdx;
                }

                RenderLine(line);
                lastRenderedLine = line.lineNumber;

                if (!line.isFoldout)
                {
                    for (int i = line.children.Count - 1; i >= 0; i--)
                        stack.Push(line.children[i]);
                }
                else
                {
                    while (orphanIdx < orphans.Length
                            && lastRenderedLine < orphans[orphanIdx].lineNumber && orphans[orphanIdx].lineNumber < line.lineNumberEnd)
                    {
                        ++orphanIdx;
                    }
                }
            }
            GUILayout.EndVertical();
        }

        void RenderLine(BTLGUILine line)
        {

            GUILayout.BeginHorizontal();
            GUI_lineNumber(line);
            GUI_indentation(line);

            if(bt.program != null && bt.program.codemaps != null && SourceDisplay.isPlaying && bt.program.exceptions.Length == 0)
                GUI_tokens_live(line);
            else
                GUI_tokens(line);

            GUILayout.EndHorizontal();
        }

        class FadeInfo
        {
            public float startTime;
            public Status status;
            public bool done;
        }
        Dictionary<BTNode, FadeInfo> nodeFadeInfos = new Dictionary<BTNode, FadeInfo>();
        private void GUI_tokens_live(BTLGUILine line)
        {
            GUIStyle style = BTLSyntaxHighlight.style_label;
            for (int i = 0; i < line.tokens.Count; ++i)
            {
                BTNode node = null;
                if (i < line.btnodes.Count)
                    node = line.btnodes[i];

                var token = line.tokens[i];
                style = BTLSyntaxHighlight.GetTokenStyle(token);

                if (node != null)
                {
                    var nodeStyle = GetNodeStyle(node);
                    if (nodeStyle != null)
                        style = nodeStyle;
                }

                if (bt.exceptions.Length > 0)
                    style = BTLSyntaxHighlight.style_comment;

                if (line.hasErrors)
                    style = BTLSyntaxHighlight.style_failed;

                GUI_token(style, node, token);

                // debug info
                bool isLastNodeToken = node != null && (i + 1 >= line.btnodes.Count || node != line.btnodes[i + 1]);
                if (isLastNodeToken && node.debugInfo != null && node.debugInfo != "")
                {
                    GUILayout.Label(string.Format("[{0}]", node.debugInfo.Replace("\t", "   ")), BTLSyntaxHighlight.style_comment);
                }
            }// for tokens
        }

        private void GUI_tokens(BTLGUILine line)
        {
            GUIStyle style = BTLSyntaxHighlight.style_label;
            for (int i = 0; i < line.tokens.Count; ++i)
            {
                BTNode node = null;
                if (i < line.btnodes.Count)
                    node = line.btnodes[i];

                var token = line.tokens[i];
                style = BTLSyntaxHighlight.GetTokenStyle(token);

                if (bt.program == null || bt.program.codemaps == null)
                    style = BTLSyntaxHighlight.style_comment;

                if (bt.exceptions.Length > 0)
                    style = BTLSyntaxHighlight.style_comment;

                if (line.hasErrors)
                    style = BTLSyntaxHighlight.style_failed;

                GUI_token(style, node, token);
            }// for tokens
        }

        private void GUI_token(GUIStyle style, BTNode node, BTLTokenizer.Token token)
        {
            var label = token.content.Replace("\t", "   ");
            if (clickedTask != null && clickedTaskFade < Time.realtimeSinceStartup)
                clickedTask = null;


#if UNITY_EDITOR
            var task = node as BTTask;

            if (task != null && task.boundState != BoundState.Bound)
                style = BTLSyntaxHighlight.style_failed;

            if (task != null && task.boundState == BoundState.Bound && token.type == BTLTokenizer.TokenType.Word)
            {
                if (GUILayout.Button(label, style) && Event.current.button == 0)
                {
                    if (clickedTask == task)
                    {
                       GUIBTScript.OpenScript(task.boundObject as MonoBehaviour, task.boundMember);
                    }

                    clickedTask = task;
                    clickedTaskFade = Time.realtimeSinceStartup + 0.5f;
                }
            }
            else
            {
                GUILayout.Label(label, style);
            }
#else
                    GUILayout.Label(label, style);
#endif
        }

        private GUIStyle GetNodeStyle(BTNode node)
        {
            GUIStyle style = null;
            if (node != null && bt.program != null && SourceDisplay.isPlaying )
            {
                style = BTLSyntaxHighlight.statusStyle[node.status];

                FadeInfo fadeInfo = null;

                if (nodeFadeInfos.ContainsKey(node))
                {
                    fadeInfo = nodeFadeInfos[node];
                }
                else
                {
                    fadeInfo = nodeFadeInfos[node] = new FadeInfo();
                    fadeInfo.done = true;
                    fadeInfo.startTime = float.NegativeInfinity;
                    fadeInfo.status = node.previousStatus;
                }
                if (node.previousStatus != Status.Running && node.status == Status.Ready && node.previousStatus != Status.Ready)
                {
                    if (fadeInfo.status != node.previousStatus && fadeInfo.done)
                    {
                        fadeInfo.status = node.previousStatus;
                        fadeInfo.startTime = Time.realtimeSinceStartup;
                        fadeInfo.done = false;
                    }

                    float t = Mathf.Clamp01((Time.realtimeSinceStartup - fadeInfo.startTime) / 0.3f);

                    if (t < 1.0f && !isPaused)
                        style = BTLSyntaxHighlight.statusStyle[node.previousStatus];
                    else
                        fadeInfo.done = true;
                }
                else
                {
                    fadeInfo.done = true;
                    fadeInfo.status = Status.Ready;
                }

            }

            return style;
        }

        private static void GUI_indentation(BTLGUILine line)
        {
            string strIndent = "";
            for (int i = 0; i < line.indent + (line.isFoldable ? 0 : 1); i++)
            {
                strIndent += "    ";
            }
            {
                //style.normal.textColor = hiddenColor;
                GUILayout.Label(strIndent, BTLSyntaxHighlight.style_label);
            }

            //style.normal.textColor = BTLSyntaxHighlight.guiColor;
            if (line.isFoldable)
            {
                // line.isFoldout = GUILayout.Toggle(line.isFoldout, line.isFoldout ? "[+]" : "[-]", style);
                if (GUILayout.Button("    ", line.isFoldout ? BTLSyntaxHighlight.style_INFoldout : BTLSyntaxHighlight.style_INFoldin))
                {
                    line.isFoldout = !line.isFoldout;
                }
            }
        }

        private void GUI_lineNumber(BTLGUILine line)
        {

            // Determine whether the lines contains a node that has been ticked on this frame.
            bool hasBeenTickedOnThisFrame = HasBeenTickedOnThisFrame(line);
            bool containsLeafNode = hasBeenTickedOnThisFrame ? ContainsLeafNodes(line) : false;

            var lineNumberStyle = BTLSyntaxHighlight.style_lineNumber;
#if !PANDA_BT_FREE
            bool isActive = hasBeenTickedOnThisFrame && (containsLeafNode || line.isFoldout) && !line.isBreakPointEnable;
#else
            bool isActive = hasBeenTickedOnThisFrame && (containsLeafNode || line.isFoldout);
#endif


            if (isActive)
                lineNumberStyle = BTLSyntaxHighlight.style_lineNumber_active;
#if !PANDA_BT_FREE
            if (line.isBreakPointEnable)
            {
                if (hasBeenTickedOnThisFrame && isPaused && line.isBreakPointActive)
                {
                    lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_active;
                }
                else
                {
                     switch( line.breakPointStatus )
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
            if (line.hasErrors)
                lineNumberStyle = BTLSyntaxHighlight.style_lineNumber_error;

            

#if UNITY_EDITOR && !PANDA_BT_FREE
            if (line.btnodes != null && line.btnodes.Count > 0)
            {
                if (GUILayout.Button(line.lineNumberText, lineNumberStyle) )
                {
                    if (Event.current.button == 0)
                        line.ToggleBreakPoint();
                    else if (Event.current.button == 1)
                        line.ClearBreakPoint();

                    var breakPoints = bt.sourceInfos[scriptIndex].breakPoints;
                    var breakPointStatuses = bt.sourceInfos[scriptIndex].breakPointStatuses;
                    bool isDirty = true;
                    while (isDirty)
                    {
                        isDirty = false;
                        for (int i = 0; i < breakPoints.Count; ++i)
                        {
                            if (breakPoints[i] == line.lineNumber)
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
                        breakPoints.Add( line.lineNumber );
                        breakPointStatuses.Add( line.breakPointStatus );
                    }
                }
            }
            else
            {
                GUILayout.Label( line.lineNumberText, lineNumberStyle);
            }
#else
			GUILayout.Label(line.lineNumberText, lineNumberStyle);
#endif
        }

        private static bool ContainsLeafNodes(BTLGUILine line)
        {
            bool containsLeafNode = false;
            if (line.btnodes != null)
            {
                foreach (var n in line.btnodes)
                {
                    if (n == null)
                        continue;
                    if (n.GetType() == typeof(BTTask) || n.GetType() == typeof(BTTreeProxy))
                    {
                        containsLeafNode = true;
                        break;
                    }
                }
            }

            return containsLeafNode;
        }

        private bool HasBeenTickedOnThisFrame(BTLGUILine line)
        {
            bool hasBeenTickedOnThisFrame = false;
            if (line.btnodes != null)
            {
                foreach (var n in line.btnodes)
                {
                    hasBeenTickedOnThisFrame = n != null && bt.program != null
                        && bt.status != Status.Ready
                        && n.lastTick == bt.program.tickCount;
                    if (hasBeenTickedOnThisFrame)
                        break;
                }
            }
            return hasBeenTickedOnThisFrame;
        }

        public static SourceDisplay[] MapGUILines(BTSource[] btlSources, BTProgram program, PandaScriptException[] pandaExceptions)
        {
            if (btlSources == null || program == null)
                return null;

            var sourceDisplays = new SourceDisplay[btlSources.Length];
            for (int i = 0; i < btlSources.Length; i++)
            {
                if (btlSources[i] == null)
                    continue;

                SourceDisplay sourceDisplay = null;
                var tokens = BTLAssetManager.GetBTLTokens(btlSources, btlSources[i]);
                sourceDisplay = BTLGUILine.Analyse(tokens, i);

                sourceDisplays[i] = sourceDisplay;

                CodeMap codemap = null;
                if (program.codemaps != null && i < program.codemaps.Length)
                    codemap = program.codemaps[i];

                if (codemap != null)
                {
                    BTLGUILine.MapNodes(sourceDisplay.lines, codemap);

                    var lines = sourceDisplay.flattenLines;
                    foreach (var line in lines)
                    {
                        foreach (var n in line.btnodes)
                        {
                            var task = n as BTTask;
                            if (task != null && task.boundState != BoundState.Bound)
                                line.hasErrors = true;
                        }
                    }
                }

                if (sourceDisplay != null)
                {
                    var lines = sourceDisplay.flattenLines;
                    foreach (var line in lines)
                    {
                        foreach (var pandaException in pandaExceptions)
                        {
                            if (pandaException != null)
                            {
                                if (pandaException.filePath == btlSources[i].url && line.lineNumber == pandaException.lineNumber)
                                {
                                    line.hasErrors = true;
                                }
                            }
                        }
                    }
                }

            }

            return sourceDisplays;
        }


#endregion

#region runtime highlighting
        public static bool isPaused = false;
        public static bool isPlaying = false;
#endregion


    }




    public class BTLGUILine : System.IDisposable
    {
        public int indent = 0;

        int _lineNumber = 0;
        public int lineNumber
        {
            get
            {
                return _lineNumber;
            }

            set
            {
                _lineNumber = value;
                _lineNumberText = string.Format("{0,5:####0} ", _lineNumber);
            }
        }

        bool _isFoldout = false;
        public bool isFoldout
        {
            get
            {
                return _isFoldout;
            }

            set
            {
                bool hasChanged = _isFoldout != value;
                 
                _isFoldout = value;

                if (hasChanged)
                {
                    BehaviourTree behaviourTree = null;
                    int scriptIndex = 0;
#if !PANDA_BT_FREE
                    var script = GUIBTScript.current;
                    if (script != null)
                    {
                        behaviourTree = script.behaviourTree;
                        scriptIndex = script.scriptIndex;
                    }
                    else if (SourceDisplay.current != null)
                    {
                        behaviourTree = SourceDisplay.current.bt;
                        scriptIndex = SourceDisplay.currentSourceIndex;
                    }
#else
                    if (SourceDisplay.current != null)
                    {
                        behaviourTree = SourceDisplay.current.bt;
                        scriptIndex = SourceDisplay.currentSourceIndex;
                    }
#endif

                    if (behaviourTree != null && behaviourTree.sourceInfos != null && scriptIndex < behaviourTree.sourceInfos.Length)
                    {
                        var list = behaviourTree.sourceInfos[scriptIndex].collapsedLines;

                        if (_isFoldout)
                        {
                            if (!list.Contains(lineNumber))
                                list.Add(lineNumber);
                        }
                        else
                        {
                            if (list.Contains(lineNumber))
                                list.Remove(lineNumber);
                        }
                    }
                }
            }
        }
        public bool isFoldable = false;
        public bool hasErrors = false;
        public List<BTLGUILine> children = new List<BTLGUILine>();
        public List<BTLTokenizer.Token> tokens    = new List<BTLTokenizer.Token>();
        public List<BTNode> btnodes = new List<BTNode>();

        string _lineNumberText;
        public string lineNumberText
        {
            get
            {
                return _lineNumberText;
            }
        }

            

#if !PANDA_BT_FREE
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

            }
        }
        public Status breakPointStatus = Status.Failed;
#endif

        public BTNode lastNode
        {
            get
            {
                BTNode node = null;
                foreach (var n in btnodes)
                {
                    if (n != null)
                    {
                        node = n;
                    }
                }
                return node;

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
            if( !isBreakPointEnable )
            {
                isBreakPointEnable = true;
                breakPointStatus = Status.Running;
            }else if(breakPointStatus == Status.Running)
            {
                breakPointStatus = Status.Succeeded;
            }else if(breakPointStatus == Status.Succeeded)
            {
                breakPointStatus = Status.Failed;
            }else if(breakPointStatus == Status.Failed)
            {
                isBreakPointEnable = false;
            }



            if (btnodes.Count == 0)
                isBreakPointEnable = false;
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
                   node.previousStatus == Status.Ready && node.status != Status.Ready  && breakPointStatus == Status.Running
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
        public int lineNumberEnd // The line number where this line block ends
        {
            get
            {
                int max  = this.lineNumber;
                foreach(var c in children)
                {
                    max = Mathf.Max(max, c.lineNumberEnd);
                }
                return max;
            }
        }

        public override string ToString()
        {
            string str = "";
            foreach (var t in this.tokens)
                str += t.content;

            return str;
        }

        public static SourceDisplay Analyse(BTLTokenizer.Token[] tokens, int sourceIndex)
        {
            if (tokens == null)
                return null;

            SourceDisplay sourceDisplay = new SourceDisplay(sourceIndex);
            int lineNumber = 1;
            var lines = new List<BTLGUILine>();
            var orphans = new List<BTLGUILine>();
            var parentStack = new Stack<BTLGUILine>();

            BTLGUILine current = new BTLGUILine();
            current.lineNumber = lineNumber;
            foreach(var t in tokens)
            {
                bool isComment = current.tokens.Count > 0 && current.tokens[0].type == BTLTokenizer.TokenType.Comment;

                if (t.type == BTLTokenizer.TokenType.EOL && current.tokens.Count == 0 )
                {// Empty line
                    orphans.Add(current);
                    ++lineNumber;
                    current = new BTLGUILine();
                    current.indent = 0;
                    current.lineNumber = lineNumber;
                }
                else if( t.type == BTLTokenizer.TokenType.EOL && isComment )
                {// End of comments
                    ++lineNumber;
                    orphans.Add(current);
                    current = new BTLGUILine();
                    current.indent = 0;
                    current.lineNumber = lineNumber;

                }
                else if (t.type == BTLTokenizer.TokenType.EOL && !isComment)
                {// End of line containing nodes
                    ++lineNumber;
                    ParentOrAddToLines(lines, parentStack, current);

                    if ( current.tokens.Count > 0 )
                        parentStack.Push(current);


                    current = new BTLGUILine();
                    current.indent = 0;
                    current.lineNumber = lineNumber;

                }
                else if(t.type == BTLTokenizer.TokenType.Indent && current.tokens.Count == 0)
                {
                   current.indent++;
                }
                else if(t.type == BTLTokenizer.TokenType.Comment)
                {
                    var commentLines = t.content.Split('\n');
                    if (commentLines.Length > 0)
                    {
                        current.tokens.Add(GenerateCommentToken(commentLines[0], current.lineNumber));
                        for (int i = 1; i < commentLines.Length; ++i)
                        {
                            ++lineNumber;
                            var commentline = commentLines[i];
                            var commentGUILine = GenerateCommentLineGui(commentline, current.indent, lineNumber);
                            current.children.Add(commentGUILine);
                        }
                    }
                }
                else
                {
                    current.tokens.Add(t);
                }
            }

            ParentOrAddToLines(lines, parentStack, current);


            sourceDisplay.lines = lines.ToArray();
            sourceDisplay.orphans =  orphans.ToArray();

            ProcessFoldable(sourceDisplay.lines);
            ProcessFoldable(sourceDisplay.orphans);

            
            return sourceDisplay;

        }

        private static void ParentOrAddToLines(List<BTLGUILine> lines, Stack<BTLGUILine> parentStack, BTLGUILine current)
        {
            if (current.tokens.Count > 0)
            {
                while (parentStack.Count > 0 && parentStack.Peek().indent >= current.indent)
                    parentStack.Pop();
            }

            if (parentStack.Count > 0 && !parentStack.Peek().children.Contains(current))
            {
                parentStack.Peek().children.Add(current);
            }
            else
            {
                if(!lines.Contains(current))
                    lines.Add(current);
            }
        }

        static BTLGUILine GenerateCommentLineGui(string lineContent, int indent, int lineNumber)
        {
            var commentGUILine = new BTLGUILine();
            commentGUILine.indent = indent;
            commentGUILine.lineNumber = lineNumber;
            string tabs= "";
            for (int i = 0; i < indent; ++i)
                tabs += "\t";

            string cleaned = lineContent;
            if( tabs != "")
              cleaned =  cleaned.Replace(tabs, "");

            cleaned = cleaned.Replace("\t", "    ");

            var token = GenerateCommentToken(cleaned, lineNumber);
            commentGUILine.tokens.Add( token );
            return commentGUILine;
        }

        private static BTLTokenizer.Token GenerateCommentToken(string lineContent, int lineNumber)
        {
            var token = new BTLTokenizer.Token(BTLTokenizer.TokenType.Comment, 0, lineContent.Length, lineContent, 0);
            token.line = lineNumber;
            return token;
        }

        static void ProcessFoldable(BTLGUILine[] lines )
        {
            var stack = new Stack<BTLGUILine>();

            for (int i = lines.Length - 1; i >= 0; i--)
                stack.Push(lines[i]);

            while (stack.Count > 0)
            {
                var line = stack.Pop();
                bool hasEmptyChildren = true;
                for (int i = line.children.Count - 1; i >= 0; i--)
                {
                    stack.Push(line.children[i]);
                    if( line.children[i].tokens.Count > 0)
                    {
                        hasEmptyChildren = false;
                    }
                }
                line.isFoldable = line.children.Count > 0 && line.tokens.Count > 0 && !hasEmptyChildren;
            }
        }

        public static void MapNodes( BTLGUILine[] lines,  CodeMap codemap )
        {
            var stack = new Stack<BTLGUILine>();

            for (int i = lines.Length - 1; i >= 0; i--)
                stack.Push(lines[i]);

            while (stack.Count > 0)
            {
                var line = stack.Pop();
                MapNodes(line, codemap);
                for (int i = line.children.Count - 1; i >= 0; i--)
                    stack.Push(line.children[i]);
            }
        }

        // Map each token to its corresponding node.
        public static void MapNodes( BTLGUILine line,  CodeMap codemap )
        {
            line.btnodes.Clear();
            for (int t = 0; t < line.tokens.Count; ++t)
                line.btnodes.Add(null);

            for (int t = 0; t < line.tokens.Count; ++t)
            {
                
                var token = line.tokens[t];

                if ( token.type == BTLTokenizer.TokenType.Comment )
                    continue;

                for (int n = 0; n < codemap.nodes.Length; ++n)
                {
                    var node = codemap.nodes[n];
                    var nodeLoc = codemap.substringLocations[n];

                    if( 
                         nodeLoc.start <= token.substring_start
                        && (token.substring_start + token.substring_length) <= (nodeLoc.start + nodeLoc.length)
                        )
                    {
                        line.btnodes[t] = node;
                        break;
                    }

                }
            }
        }

        public void Dispose()
        {
#if !PANDA_BT_FREE
            isBreakPointEnable = false;
#endif
        }
    }

}
