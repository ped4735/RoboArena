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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using InspectorGuiData = Panda.BehaviourTree.InspectorGuiData;

namespace Panda
{

	[CustomEditor(typeof(BehaviourTree))]
	public class BehaviourTreeEditor : Editor
	{
		int size = 1;
        BehaviourTree bt = null;
        SourceDisplay[] sourceDisplays;

        GUIBTScript[] guiBTScripts;
        
        class SourceInfo
        {
            public BTSource btSource;
            public SourceInfo(BTSource a )
            {
                btSource = a;
            }

            public bool IsSame(BTSource a )
            {
                return this.btSource == a;
            }
        }

        List<SourceInfo> sourceInfos = new List<SourceInfo>();

        static List<BehaviourTreeEditor> instances = new List<BehaviourTreeEditor>();
        public void OnEnable()
		{
            Undo.undoRedoPerformed += UndoRedoPerformed;
            SourceDisplay.refreshAllBehaviourTreeEditors = RefreshAll;
            if (!instances.Contains(this))
                instances.Add(this);

            BTLSyntaxHighlight.InitializeColors();

            bt = (BehaviourTree)target;
            if (bt != null && bt.scripts != null)
            {
                size = bt.scripts.Length;
            }
            else if (bt.scripts == null &&  bt.program == null)
            {
                bt.scripts = new TextAsset[size];
            }

            if(bt != null && bt.scripts == null && bt.program != null)
            {// When 'BehaviourTree.Compile(...)' has been used.
                size = bt.btSources.Length;
            }


            if (bt != null && bt.program == null)
            {
                bt.Compile();
            }

            if ( bt != null )
            {
                bt._isInspected = true;
            }

            InitSourceInfos();
            bt.OnTicked += this.Repaint;

		}

		void InitSourceInfos()
		{
            sourceInfos.Clear();
            foreach (var src in bt.btSources)
                sourceInfos.Add(new SourceInfo(src));

            var oldSourInfos = bt.sourceInfos;
            if (bt.sourceInfos == null || bt.sourceInfos.Length != size)
            {
                bt.sourceInfos = new InspectorGuiData[size];
                for (int i = 0; i < size; ++i)
                {
                    if( oldSourInfos != null && i < oldSourInfos.Length && oldSourInfos[i] != null)
                        bt.sourceInfos[i] = oldSourInfos[i];
                    else
                        bt.sourceInfos[i] = new InspectorGuiData();
                }
            }

            sourceDisplays = SourceDisplay.MapGUILines(bt.btSources, bt.program, bt.pandaExceptions);
            if (sourceDisplays != null)
            {
                foreach (var sd in sourceDisplays)
                {
                    if (sd != null)
                        sd.bt = bt;
                }
            }

            if( bt != null && bt.scripts != null)
            {

#if !PANDA_BT_FREE
                var guiBTScriptsList = new List<GUIBTScript>();
#endif
                for (int i = 0; i < bt.scripts.Length; i++)
                {

#if !PANDA_BT_FREE
                    var a = bt.scripts[i];
                    GUIBTScript b = null;
                    if (guiBTScripts != null && i < guiBTScripts.Length && guiBTScripts[i] != null)
                        b = guiBTScripts[i];
                    else
                        b = new GUIBTScript(bt, a, i);

                    if (a != null)
                        b.Parse();

                    guiBTScriptsList.Add(b);
#endif
                    // Read line collapsed state from sourceInfos
                    if (sourceDisplays != null && i < sourceDisplays.Length && sourceDisplays[i] != null)
                    {
                        var lines = sourceDisplays[i].flattenLines;
                        var list = bt.sourceInfos[i].collapsedLines;
                        foreach (var line in lines)
                            line.isFoldout = list.Contains(line.lineNumber);
                    }

                }

#if !PANDA_BT_FREE
                guiBTScripts = guiBTScriptsList.ToArray();
                InitBreakPoints();
                bt.Apply();
#else
                 bt.Apply();
#endif

            }


        }

#if !PANDA_BT_FREE
        private void InitBreakPoints()
        {
            if (sourceDisplays == null)
                return;

            for (int i = 0; i < sourceDisplays.Length; ++i)
            {
                var sourceDisplay = sourceDisplays[i];
                if (sourceDisplay == null)
                    continue;

                var lines = sourceDisplay.flattenLines;
                foreach (var line in lines)
                    line.isBreakPointEnable = false;

                if (! (i < bt.sourceInfos.Length))
                    continue;

                var breakPoints = bt.sourceInfos[i].breakPoints;
                var breakPointStatuses = bt.sourceInfos[i].breakPointStatuses;
                for (int b = 0; b < breakPoints.Count; ++b)
                {
                    var l = breakPoints[b] - 1;
                    lines[l].isBreakPointEnable = true;
                    lines[l].breakPointStatus = breakPointStatuses[b];
                }
            }
        }
#endif
        public override void OnInspectorGUI()
        {
            BTLSyntaxHighlight.InitializeStyles();

            CheckComponentChanges();

            EditorGUILayout.BeginVertical();
            
            if (bt != null && bt.exceptions.Length > 0 && Application.isPlaying)
            {
                GUILayout.BeginHorizontal();
                var style = BTLSyntaxHighlight.style_failed;
                GUILayout.Label("This program contains errors. Please check console for details.", style);
                GUILayout.EndHorizontal();
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            bt.tickOn = (BehaviourTree.UpdateOrder)EditorGUILayout.EnumPopup("Tick On:", bt.tickOn);
            bt.autoReset = GUILayout.Toggle(bt.autoReset, "Repeat Root");
            EditorGUILayout.EndHorizontal();

            DisplayStatus();

            var newSize = EditorGUILayout.IntField("Count", size);

            if (EditorGUI.EndChangeCheck())
            {
                RecordUndo();
                if( size != newSize)
                {
                    size = newSize;

                    if (size < 0) size = 0;

                    // Resize the TextAsset array
                    TextAsset[] old = new TextAsset[bt.scripts.Length];
                    bt.scripts.CopyTo(old, 0);
                    bt.scripts = new TextAsset[size];

                    for (int i = 0; i < size; ++i)
                        bt.scripts[i] = i < old.Length ? old[i] : null;

                    bt.Apply();
                    bt.Compile();
                    Clear_guiBTScripts();

                    InitSourceInfos();

                    Refresh();
                }
            }


            EditorGUILayout.EndVertical();

            if ( bt.scripts != null || bt.btSources.Length > 0 )
            {
                for (int i = 0; i < size; ++i)
                {

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(1.0f));

                    var isFoldout = EditorGUILayout.Foldout(bt.sourceInfos[i].isFoldout, "");

                    if (isFoldout != bt.sourceInfos[i].isFoldout)
                    {
                        RecordUndo();
                        bt.sourceInfos[i].isFoldout = isFoldout;
                    }

                    EditorGUILayout.EndHorizontal();

                    string label = null;
#if !PANDA_BT_FREE
                    if(guiBTScripts != null )
                        label = string.Format("BT Script {0}{1}", i, guiBTScripts[i].isModified ? "*" : "");
                    else
                        label = string.Format("BT Script {0}", i);
#else
                    label = string.Format("BT Script {0}", i);
#endif

                    GUILayout.Label(label, GUILayout.MaxWidth(100.0f));

                    if (bt.scripts != null)
                    {
                        var newScript = EditorGUILayout.ObjectField(bt.scripts[i], typeof(TextAsset), allowSceneObjects: false) as TextAsset;
                        if (bt.scripts[i] != newScript)
                        {
                            RecordUndo();
                            bt.scripts[i] = newScript;

                            if (sourceInfos != null && i < sourceInfos.Count)
                                sourceInfos[i] = null;

                            Clear_guiBTScripts();
                            bt.Apply();
                            bt.Compile();
                            InitSourceInfos();
                            Refresh();
                        }
                    }else
                    {
                        GUILayout.Label("[compiled from string]");
                    }

#if !PANDA_BT_FREE
                    if (guiBTScripts != null)
                    {
                        GUI.enabled = guiBTScripts[i].isModified;
                        switch (GUILayout.Toolbar(-1, new string[] { "Revert", "Apply" }))
                        {
                            case 0: guiBTScripts[i].Revert(); bt.Compile(); break;
                            case 1: guiBTScripts[i].SaveToFile(); break;
                        }
                        GUI.enabled = true;
                    }
#endif


                    EditorGUILayout.EndHorizontal();

                    DisplayBTScript(i);
                }
            }
        }

        private void DisplayStatus()
        {
            var style = BTLSyntaxHighlight.style_label;
            if (bt.program != null)
            {
                if (bt.program._boundState != BoundState.Bound)
                {
                    style = BTLSyntaxHighlight.style_failed;
                    if (GUILayout.Button("Status: Error", style))
                    {
                        bt.DebugLogErrors();
                    }
                }
                else
                {
                    GUILayout.Button(string.Format("Status:{0}", bt.status.ToString()), style);
                }
            }
            else
            {
                GUILayout.Button(string.Format("Status:{0}", "N/A"), style);
            }
        }

        private void DisplayBTScript(int i)
        {
            GUIBTScript.isMouseHoverLineNumbers = false;
            bool isSourceAvailable = false;
            if (bt.scripts != null && i < bt.scripts.Length && bt.scripts[i] != null)
                isSourceAvailable = true;
            else if ( bt.sourceInfos != null && i < bt.sourceInfos.Length && bt.sourceInfos[i] != null && bt.sourceInfos[i].btScript != null && bt.sourceInfos[i].btScript.Trim() != "" )
                isSourceAvailable = true;
                   

            if (isSourceAvailable && bt.sourceInfos[i].isFoldout)
            {

#if !PANDA_BT_FREE
                if (!EditorApplication.isPlaying)
                {
                    guiBTScripts[i].OnGUI();
                }
                else
                {
#endif
                    SourceDisplay sourceDisplay = null;
                    if (sourceDisplays != null && i < sourceDisplays.Length)
                        sourceDisplay = sourceDisplays[i];

                    if (sourceDisplay != null)
                    {
                        sourceDisplay.DisplayCode();
                    }else
                    {
                        GUILayout.Label("Error parsing script. See console for details.", BTLSyntaxHighlight.style_failed);
                    }
#if !PANDA_BT_FREE
                }
#endif
            }
            else if (bt.scripts != null && bt.scripts[i] != null && !bt.sourceInfos[i].isFoldout)
            {
                if (bt.sourceInfos[i].breakPoints.Count > 0)
                {
                    bt.sourceInfos[i].breakPoints.Clear();
#if !PANDA_BT_FREE
                    InitBreakPoints();
#endif
                }
            }
        }

        private void RecordUndo()
        {
            Undo.RecordObject(bt, "Undo Inspector");
        }

        private void UndoRedoPerformed()
        {
            if (bt != null)
                bt.Compile();
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }


        public void Refresh()
        {
            Clear_guiBTScripts();
            InitSourceInfos();
            EditorUtility.SetDirty(bt);
            Repaint();
        }

        public static void RefreshAll()
        {
            foreach (var ed in instances)
            {
                ed.Refresh();
            }
        }

        static Dictionary<Behaviour, int> _componentCount = new Dictionary<Behaviour, int>();
        void CheckComponentChanges()
        {
            var bt = target as BehaviourTree;

            if(bt != null )
            {

                int n = bt.gameObject.GetComponents<Component>().Length;
                int prev_n = n;

                if (_componentCount.ContainsKey(bt))
                    prev_n = _componentCount[bt];

                if ( n != prev_n )
                {
                    bt.Apply();
                    bt.Compile();
                    Clear_guiBTScripts();
                    InitSourceInfos();
                }

                _componentCount[bt] = n;
            }
        }


        void Clear_guiBTScripts()
        {
            if (guiBTScripts != null)
            {
                foreach (var b in guiBTScripts)
                {
                    if (b != null)
                        b.Dispose();
                }
                guiBTScripts = null;
            }
        }

        void OnDestroy()
        {
            Clear_guiBTScripts();

            if ( sourceDisplays != null)
            {
                foreach(var sd in sourceDisplays)
                {
                    if (sd != null)
                        sd.Dispose();
                }
            }
            sourceDisplays = null;

            while (instances.Contains(this))
                instances.Remove(this);

            while (instances.Contains(null))
                instances.Remove(null);

            if (instances.Count == 0)
                SourceDisplay.refreshAllBehaviourTreeEditors = null;

            if (bt != null)
            {
                bt._isInspected = false;
                bt.OnInitialized -= OnEnable;
                bt.OnTicked -= this.Repaint;
            }
        }

        bool haveSourcesChanged()
        {
            bool haveChanged = false;

            if (bt.btSources.Length == sourceInfos.Count)
            {
                for (int i = 0; i < bt.scripts.Length; ++i)
                {
                    if (!sourceInfos[i].IsSame(bt.btSources[i]))
                    {
                        haveChanged = true;
                        break;
                    }
                }
            }
            else
            {
                haveChanged = true;
            }
            return haveChanged;
        }
    }

}