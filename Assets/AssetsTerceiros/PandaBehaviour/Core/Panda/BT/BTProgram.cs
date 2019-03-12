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

using System.Collections;
using System.Collections.Generic;

using System.Reflection;

namespace Panda
{
	// BT Program
	public class BTProgram : System.IDisposable
	{
        internal bool isInspected = false;
        internal static BTProgram current = null;
        internal BTNode currentNode = null;

        internal BTTree[][] _treeSets;
        public BTTree[][] treeSets { get { return _treeSets; } }

        internal BTTree _root;
        int _tickCount = 0;
        public int tickCount { get { return _tickCount; } }
        
        public BTTree main 
        { 
            get
            {  
                if( _root == null && _treeSets != null )
                {
                    for(int i=0; i < _treeSets.Length; ++i)
                    {
                        var set = _treeSets[i];
                        if (set != null)
                        {
                            for (int j = 0; j < set.Length; ++j)
                            {
                                var b = set[j];
                                if (b.name.ToLower() == "root" || _root == null)
                                    _root = b;
                            }
                        }
                    }
                }
                return _root;
            } 
        }

        internal CodeMap[] _codemaps;
        public CodeMap[] codemaps { get{return _codemaps;}}

        internal System.Exception[] _exceptions = null;
        public System.Exception[] exceptions 
        { 
            get 
            { 
                if (_exceptions == null)
                    _exceptions = new System.Exception[0];

                return _exceptions; 
            } 
        }

        bool hasThrownExceptionOnTick = false;

        public Status status
        {
            get
            {
                var status = Status.Failed;
                if (main != null)
                    status = main.m_status;
                return status;

            }
        }

        public BTProgram()
        {
            hasThrownExceptionOnTick = false;
        }

        public BTProgram( System.Exception[] exceptions)
        {
            _exceptions = exceptions;
            _boundState = BoundState.Failed;
            hasThrownExceptionOnTick = false;
        }

        public void Tick()
        {
            BTProgram previous = BTProgram.current;
            BTProgram.current = this;

            if (main != null && boundState == BoundState.Bound)
            {
                ++_tickCount;
                main.Tick();
            }
            else
            {

                if (!hasThrownExceptionOnTick)
                {
                    BTProgram.current = previous;
                    hasThrownExceptionOnTick = true;
                    throw new System.Exception("Can not tick behaviour tree program. Program is invalid.");
                }
            }
            BTProgram.current = previous;
        }

        public void Reset()
        {
            BTProgram previous = BTProgram.current;
            BTProgram.current = this;
            if (treeSets != null)
            {
                foreach (var ts in treeSets)
                {
                    if (ts == null) continue;
                    foreach (var t in ts)
                        if (t != null) t.Reset();
                }
            }
            BTProgram.current = previous;
        }

        public BoundState _boundState;
        public BoundState boundState { get{ return _boundState; } }
        
        object[] m_boundObjects;
        public object[] boundObjects{ get{ return m_boundObjects;}}

        public void Bind( object[] objects )
        {
            _boundState = BoundState.Bound;
            hasThrownExceptionOnTick = false;
            m_boundObjects = objects;
            if (main != null)
            {
                BTRuntimeBuilder.Bind(this, objects);

                var tasks = this.tasks;
                foreach(var t in tasks)
                {
                    if( t.boundState != BoundState.Bound )
                    {
                        _boundState = BoundState.Failed;
                        break;
                    }
                }

                Reset();
            }
            else
            {
                _boundState = BoundState.Failed;
            }
        }

        public void Unbind()
        {
            if (main != null)
            {
                var tasks = this.tasks;
                foreach (var t in tasks)
                    t.Unbind();
            }
            _boundState = BoundState.Unbound;
        }

        BTTask[] _tasks;
        public BTTask[] tasks
        {
            get
            {
                if (_tasks == null)
                {
                    var taskList = new List<BTTask>();
                    if (treeSets != null)
                    {
                        foreach (var set in treeSets)
                        {
                            if (set == null) continue;

                            foreach (var tree in set)
                            {
                                if (tree != null)
                                {
                                    foreach (var task in tree.tasks)
                                    {
                                        if (!taskList.Contains(task))
                                            taskList.Add(task);
                                    }
                                }
                            }
                        }
                    }
                    _tasks = taskList.ToArray();
                }

                return _tasks;
            }
        }

        BTNode[] _nodes;
        public BTNode[] nodes
        {
            get
            {
                if (_nodes == null)
                {
                    var nodeList = new List<BTNode>();
                    if (treeSets != null)
                    {
                        foreach (var set in treeSets)
                        {
                            if (set == null) continue;

                            foreach (var tree in set)
                            {
                                if (tree != null)
                                {
                                    if (!nodeList.Contains(tree))
                                        nodeList.Add(tree);
                                    foreach (var node in tree.childrenRecursive)
                                    {
                                        if (!nodeList.Contains(node))
                                            nodeList.Add(node);
                                    }
                                }
                            }
                        }
                    }
                    _nodes = nodeList.ToArray();
                }

                return _nodes;
            }
        }


        public void Dispose()
        {
            _root = null;
            _treeSets = null;
            _codemaps = null;
            _tasks = null;
        }

        public int lastTickFrame
        {
            get
            {
                return main != null? main.lastTick: -1;
            }
        }


        #region serialization
        public BTSnapshot snapshot
        {
            get
            {
#if !PANDA_BT_FREE
                BTSnapshot _state = new BTSnapshot();
                _state.tickCount = _tickCount;
                var nodes = this.nodes;

                _state.nodeStates = new BTNodeState[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    _state.nodeStates[i] = nodes[i].state;
                }

                return _state;
#else
                return null;
#endif
            }

            set
            {
#if !PANDA_BT_FREE
                //this.Reset();
                BTSnapshot _state = value;
                _tickCount = _state.tickCount;

                var nodes = this.nodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i].state = _state.nodeStates[i];
                }
#endif
            }
        }
#endregion
    }
}
	

