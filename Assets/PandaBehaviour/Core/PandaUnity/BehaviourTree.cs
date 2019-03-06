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
using System;
using System.Collections;
using System.Collections.Generic;

namespace Panda
{
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    public class BehaviourTree : MonoBehaviour
    {
        [Serializable]
        public class InspectorGuiData
        {
            [SerializeField]
            public bool isFoldout = true; // whether the source file appears folded.

            [SerializeField]
            public List<int> breakPoints = new List<int>(); // List of line numbers where a bp is set.

            [SerializeField]
            public List<Status> breakPointStatuses = new List<Status>();

            public List<int> collapsedLines = new List<int>(); // List of collapsed lines.

            [SerializeField]
            public string btScript; // Modified script or scripts compile from string (using PandaBehaviour.Compile(...)).
        }

        /// <summary>
        /// BT scripts
        /// </summary>
        public TextAsset[] scripts;

        [NonSerialized]
        public bool _isInspected = false;

        BTSource[] _btSources;
        public BTSource[] btSources
        {
            get
            {
                /* If there are scripts defined in the source info, return them.
                 * Otherwise, return the TextAssets assigned as scripts  */
                if( _btSources == null )
                {
                    var list = new List<BTSource>();

                    if (scripts != null)
                    {
                        for (int i = 0; i < scripts.Length; i++)
                        {
                            var a = scripts[i];
                            var info = sourceInfos != null && i < sourceInfos.Length ? sourceInfos[i] : null;

                            if (a != null && info != null && info.btScript != null && info.btScript.Trim() != "")
                            {// There is a TextAsset and it has been modified from the Inspector (Pro Only).
#if UNITY_EDITOR
                                var url = string.Format("(InstanceID:{0} BTScript:{1}) {2}*", this.GetInstanceID(), i, UnityEditor.AssetDatabase.GetAssetPath(a));
                                list.Add(new BTSourceString(info.btScript, url));
#else
                                list.Add(new BTSourceString(info.btScript));
#endif
                            }
                            else if (a != null)
                            {// There is a TextAsset with no modification.
                                list.Add(new BTSourceTextAsset(a));
                            }
                        }// for
                    }
                    else if (sourceInfos != null)
                    {
                        for (int i = 0; i < sourceInfos.Length; i++)
                        {
                            var info =  sourceInfos[i];
                            if (info != null && info.btScript.Trim() != "")
                            {// There is no TextAsset but there is a script assigned with Compile(string[]).
                                list.Add(new BTSourceString(info.btScript));
                            }
                        }
                    }
                    _btSources = list.ToArray();
                }
                return _btSources;
            }
        }

        public void Apply()
        {
            _btSources = null;

            if( _program == null )
                _requiresRecompile = true;
        }

        /// <summary>
        /// Whether the root node is automatically reset when completed.
        /// </summary>
        public bool autoReset = true;

        /// <summary>
        /// On which update function the BT is ticked.
        /// </summary>
		public UpdateOrder tickOn = UpdateOrder.Update;

        public InspectorGuiData[] sourceInfos;

        /// <summary>
        /// Callback triggered when the BT is initialized.
        /// </summary>
        public System.Action OnInitialized;

        /// <summary>
        /// Callback triggered when the BT is ticked.
        /// </summary>
        public System.Action OnTicked;

        System.Exception[] _exceptions;
        public System.Exception[] exceptions
        {
            get
            {
                if (_exceptions == null)
                    _exceptions = new System.Exception[0];

                return _exceptions;
            }
        }

        PandaScriptException[] _pandaExceptions;
        public PandaScriptException[] pandaExceptions
        {
            get
            {
                if (_pandaExceptions == null)
                {
                    var pandaExceptions = new List<PandaScriptException>();
                    if (_exceptions != null)
                    {
                        foreach (var e in _exceptions)
                        {
                            var p = e as PandaScriptException;
                            if (p != null)
                                pandaExceptions.Add(p);
                        }
                    }
                    _pandaExceptions = pandaExceptions.ToArray();
                }
                return _pandaExceptions;
            }
        }

        bool _isInitialized = false;
        bool _requiresRecompile = false;
        /// <summary>
        /// The BT current status.
        /// </summary>
        public Status status
        {
            get
            {
                return _program != null? _program.status: Status.Failed;
            }
        }

		public enum UpdateOrder
		{
			Update,
			LateUpdate,
			FixedUpdate,
			Manual
		}

		public BTProgram program 
        { 
            get
            {
                return _program; 
            } 
        }

        /// <summary>
        /// Tick the BT.
        /// </summary>
		public virtual void Tick()
		{
                if (program != null && this.enabled)
                {
                    Task._isInspected = this._isInspected;
                    if (program.status != Status.Running && program.status != Status.Ready && autoReset)
                        program.Reset();

                    program.Tick();

                    if (OnTicked != null)
                        OnTicked();

                    Task._isInspected = false;
                }


        }

        /// <summary>
        /// Compile a BT from the \p source.
        /// </summary>
        /// <param name="source"></param>
        public void Compile(string source)
        {
            Compile(new string[] { source });
        }
        /// <summary>
        /// Compile a BT from \p sources.
        /// </summary>
        /// <param name="sources"></param>
        public void Compile(string[] sources)
        {
            scripts = null;
            _btSources = null;
            var list = new List<BTSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if (source != null)
                {
                    list.Add(new BTSourceString(source));
                }
                
            }
            _btSources = list.ToArray();

            sourceInfos = new InspectorGuiData[_btSources.Length];
            for(int i=0; i < sourceInfos.Length; ++i)
            {
                sourceInfos[i] = new InspectorGuiData();
                sourceInfos[i].btScript = _btSources[i].source;
            }
            Compile();
        }

        /// <summary>
        /// Compile the BT from the attached scripts.
        /// </summary>
        public void Compile()
        {
            _exceptions = null;
            _pandaExceptions = null;
            _requiresRecompile = false;

            if (_program != null)
            {
                _program.Dispose();
                _program = null;
            }

            if ( btSources.Length > 0)
            {
                bool hasSource = false;
                foreach (var t in btSources)
                {
                    if (t != null)
                    {
                        hasSource = true;
                        break;
                    }
                }

                if (hasSource)
                {
                    bool hasCompiled = false;
                    hasCompiled = !BTLAssetManager.HasCompilationErrors(btSources);
                    if (hasCompiled)
                        CreateProgramAndBind();
                    else
                        DebugLogCompilationExceptions();
                }
                
                if (OnInitialized != null)
                    OnInitialized();
            }

        }

        private void CreateProgramAndBind()
        {
            BehaviourTree._current = this;

            _program = BTLAssetManager.CreateBehaviourProgram(btSources);

            var objects = this.GetComponents<Component>();
            try
            {
                _program.Bind(objects);
                _exceptions = _program.exceptions;
                
                foreach (var ex in _program.exceptions)
                    Debug.LogException(ex, this);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e, this);
                _program = null;
            }

            if (_program != null)
                _program.Reset();
        }

        private void DebugLogCompilationExceptions()
        {
            _exceptions = BTLAssetManager.GetCompilationExceptions(btSources);
            _pandaExceptions = null;
            _program = new BTProgram(pandaExceptions);
            
            if (Application.isPlaying)
                DebugLogErrors();
        }

        public void DebugLogErrors()
        {
            if (_program != null)
            {
                foreach (var ex in _program.exceptions)
                    Debug.LogException(ex, this);
            }
        }

        /// <summary>
        /// Reset the BT.
        /// </summary>
        public void Reset()
        {
            if (program != null)
                program.Reset();
        }


#region internals

        BTProgram _program;

#endregion

        void Initialize()
        {
            _btSources = null;
            Compile();
            _isInitialized = true;
        }

        protected virtual void Awake()
        {
            Initialize();
        }

		protected virtual void Update()
		{
            if(!_isInitialized)
            {
                Initialize();
            }

            if( _requiresRecompile){
                Compile();
            }

            if (tickOn == UpdateOrder.Update && Application.isPlaying)
                Tick();
		}

		protected virtual void FixedUpdate()
		{
			if (tickOn == UpdateOrder.FixedUpdate && Application.isPlaying)
				Tick();
		}

		protected virtual void LateUpdate()
		{
			if (tickOn == UpdateOrder.LateUpdate && Application.isPlaying)
				Tick();
		}

        protected virtual void OnDestroy()
        {
            OnInitialized = null;
        }

        internal static BehaviourTree _current;

        /// <summary>
        /// (Serializable) The current BT execution state.
        /// </summary>
        public BTSnapshot snapshot
        {
            get
            {
                BTSnapshot _state = null;
#if !PANDA_BT_FREE
                if (program != null)
                    _state = program.snapshot;
#else
                Debug.LogWarning("PandaBehaviour.snapshot is Pro feature.");
#endif
                return _state;
            }

            set
            {
#if !PANDA_BT_FREE
                if (program != null)
                    program.snapshot = value;
#endif
            }
        }


        private List<Panda.PandaTree> getTreeCache = new List<PandaTree>();
        /// <summary>
        /// Returns the tree with the given .
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PandaTree GetTree(string name)
        {
            PandaTree wantedTree = null;

            foreach(var tree in getTreeCache)
            {
                if( tree.name == name)
                {
                    wantedTree = tree;
                    break;
                }
            }

            if (this.program != null && wantedTree == null)
            {
                var treeSets = this.program.treeSets;
                foreach (var treeSet in treeSets)
                {
                    foreach (var tree in treeSet)
                    {
                        if (tree.name == name)
                        {
                            wantedTree = new Panda.PandaTree();
                            var proxy = new BTTreeProxy();
                            proxy.target = tree;
                            proxy.name = name;
                            wantedTree._tree = proxy;
                            getTreeCache.Add(wantedTree);
                            break;
                        }
                    }
                }
            }

            return wantedTree;
        }
    }
}
