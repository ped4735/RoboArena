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

using ParseNode = Panda.BTLParser.Node;
using TokenType = Panda.BTLTokenizer.TokenType;

namespace Panda
{
	public class BTGeneratorDot 
	{
	
		
		public static string PrintDotGraph (ParseNode tree)
		{
			string strDot = "digraph G {\n";
			// Get all nodes
			var stack = new Stack<ParseNode> ();
			var nodes = new List<ParseNode>();
			
			stack.Push (tree);
			while (stack.Count > 0) 
			{
				var node = stack.Pop ();
				nodes.Add( node );
				for (int c = node.children.Count - 1; c >= 0; --c) 
				{
					var child = node.children [c];
					stack.Push (child);
				}
			}
			
			var nodeID = new Dictionary<ParseNode, int>();
			for(int i=0; i < nodes.Count;++i)
			{
				nodeID[nodes[i]] = i;
			}
			
			
			// Define node label
			string strlabels = "";
			foreach( var node in nodes)
			{
				string name = nodeID[node].ToString();
				string label = node.token.ToString().Replace("\"", "").Replace("[", "").Replace("]", "");
				string style = DotStyle(node);
				strlabels += string.Format("{0} [label=\"{1}\"{2}]\n", name, label, style);
			}
			strDot += strlabels;
			
			
			//Dot graph tree
			stack.Push (tree);
			string strParenting = "";
			while (stack.Count > 0) 
			{
				var node = stack.Pop ();
				foreach (var child in node.children) 
				{
					strParenting +=string.Format( "{0} -> {1}\n",
					                             nodeID[node] , nodeID[child]
					                             );
					
					stack.Push (child);
				}
			}
			strDot += strParenting;
			strDot += "\n}";
			return strDot;
		}
		
		static string DotStyle(ParseNode node )
		{
			string style = "";
	
			switch( node.token.type  )
			{
			case TokenType.Parallel: style = ",color=red,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Tree: style = ",shape=doublecircle,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Fallback: style = ",shape=parallelogram,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Sequence: style = ",color=square,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Word:
				style = ",shape=plaintext"; break;
			}
			
			return style;
		}
		
	}
}