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

namespace Panda
{

	public class BTRepeat : BTCompositeNode
	{
		
		public BTRepeat()
		{
		}

        public BTRepeat( int count, BTNode child )
		{
            i_parameters = new object[] { count };
            m_limit = count;
			m_child = child;
		}
		
		internal int m_counter = 0;
		int m_limit   = 0;
		
        protected override void DoReset()
        {
            m_child.Reset();
            m_limit = parameters.Length > 0 ? System.Convert.ToInt32( parameters[0] ) : 0;
            m_counter = 0;
        }

		protected override Status DoTick ()
		{

			if (this.m_status == Status.Running && m_child.m_status == Status.Succeeded && (m_counter < m_limit || m_limit == 0))
				m_child.Reset();

            Status status = m_child.Tick();

			if (status == Status.Succeeded)
				++m_counter;


            if ( (m_counter < m_limit || m_limit == 0) && status == Status.Succeeded)
				status = Status.Running;

            if( Task._isInspected )
                this.debugInfo = m_limit != 0? string.Format("i={0}", m_counter) : "";
			
			return status;
		}
		
		#region child management
		BTNode m_child;
		
		public override void AddChild (BTNode child)
		{
			if(child != null)
			{
				if( m_child == null  )
				{
					m_child = child;
				}
				else
				{
					throw new System.Exception("BT error: Repeat node can have only one child.");
				}
			}
			_children = null;
		}
		
		public override void Dispose ()
		{
			m_child = null;
		}

		BTNode[] _children;
		public override BTNode[] children 
		{
			get 
			{
				if(_children == null)
					_children = new BTNode[]{m_child};
				return _children;
			}
		}
        #endregion

        internal override BTNodeState state
        {
            get
            {
                return new BTRepeatState(this);
            }

            set
            {
                var _state = value as BTRepeatState;
                base.state = _state;
                this.m_counter = _state.counter;
            }
        }
    }


}

