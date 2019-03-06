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

using System;
using System.Runtime.Serialization;

namespace Panda
{
	public abstract class BTNode: System.IDisposable
	{

		public event System.Action OnTick;

        public string debugInfo = null;

        internal int m_lastTick = -1;
        public int lastTick{get{ return m_lastTick;}}

        public static BTNode currentNode
        {
            get
            {
				return BTProgram.current.currentNode;
            }
        }
        internal object[] i_parameters = null;
        public object[] parameters{get{return i_parameters;}}
        System.Type[] _parameterTypes;
        internal System.Type[] parameterTypes
        {
            get
            {
                if( _parameterTypes == null )
                {
                    _parameterTypes = new System.Type[i_parameters.Length];
                    for (int i = 0; i < i_parameters.Length; ++i)
                    {
                      _parameterTypes[i] = i_parameters[i].GetType();
                    }

                }
                return _parameterTypes;
            }
        }

        internal BTNode m_parent;
		public BTNode parent
		{
			get
			{
				return m_parent;
			}
		}
		
		internal Status m_status = Status.Running; // Initial value has to be different to Status.Ready to Reset();
        internal Status m_previousStatus = Status.Ready;
        public Status status
		{
			get
			{
				return m_status;
			}
		}

        public Status previousStatus
        {
            get
            {
                return m_previousStatus;
            }
        }

        protected abstract Status DoTick();
        protected abstract void DoReset();
		
		public abstract void AddChild( BTNode child);
		
		public Status Tick()
		{
			if (m_status == Status.Ready || m_status == Status.Running) // isTickable
			{
                BTNode previous = BTProgram.current.currentNode;
                BTProgram.current.currentNode = this;

				m_previousStatus = m_status;
                m_lastTick = BTProgram.current.tickCount;


                if (m_status != Status.Ready && m_nextReturnStatus != m_status)
                    m_status = m_nextReturnStatus;
                else
                    m_status = DoTick();

                if (m_status == Status.Ready)
                    m_status = Status.Running;


                if (OnTick != null)
                {
                    BTProgram.current.currentNode = this;
                    OnTick();
                }

                BTProgram.current.currentNode = previous;
			}
            return m_status;
		}

		
		
		public void Reset()
		{
            if ( m_status != Status.Ready ) // Ensure the node is reset only once.
            {
                BTNode previous =  BTProgram.current.currentNode;
                BTProgram.current.currentNode = this;

                debugInfo = "";

                m_previousStatus = m_status;
                m_status = Status.Ready;
                m_nextReturnStatus = Status.Running;

                DoReset();

                BTProgram.current.currentNode = previous;
            }
        }

        internal Status m_nextReturnStatus = Status.Running;
        /// <summary>
        /// Succeed the task. The task will report Status.Succeeded on the next status return.
        /// </summary>
        public void Succeed()
        {
            m_nextReturnStatus = Status.Succeeded;
			if (BTProgram.current != null && BTProgram.current.currentNode == this)
                m_status = m_nextReturnStatus;
        }

        /// <summary>
        /// Fail the task. The task will report Status.Failed on the next status return.
        /// </summary>
        public void Fail()
        {
            m_nextReturnStatus = Status.Failed;
			if (BTProgram.current != null && BTProgram.current.currentNode == this)
                m_status = m_nextReturnStatus;
        }

		public abstract void Dispose();
        public abstract BTNode[] children{get;}


        internal virtual BTNodeState state
        {
            get
            {
                return new BTNodeState(this);
            }

            set
            {
#if !PANDA_BT_FREE
                this.m_status = value.status;
                this.m_previousStatus = value.previousStatus;
                this.m_nextReturnStatus = value.nextReturnStatus;
                this.m_lastTick = value.lastTick;
#endif
            }
        }

    }
}
