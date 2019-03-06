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
	public class BTTreeProxy : BTNode
	{
		public string name = "";
		
		public BTTreeProxy()
		{
		}
		
		public BTTree target;

		protected override void DoReset()
		{
            target.Reset();
		}

		protected override Status DoTick ()
		{
			if( this.m_status == Status.Ready && target.m_status == Status.Running )
            {
                if (target.lastTick == this.lastTick)
                {
                    string msg = string.Format("The tree(\"{0}\") is already running. Concurrent execution of the same subtree is not supported.", this.name);
                    this.Fail();
                    throw new System.Exception(msg);
                }
                else
                {
                    target.Reset();
                }
            }
            else if( this.m_status == Status.Ready && this.m_status != target.m_status)
            {
                target.Reset();
            }

			return target.Tick();
        }
		
		public override void AddChild (BTNode child)
		{
			throw new System.Exception("BT error: TreeProxy node does not contain child.");
		}
		

		BTNode[] _children;
		public override BTNode[] children 
		{
			get 
			{
				if(_children == null)
					_children = new BTNode[]{};
				return _children;
			}
		}
		
		public override void Dispose ()
		{
			target = null;
		}
		
	}
}

