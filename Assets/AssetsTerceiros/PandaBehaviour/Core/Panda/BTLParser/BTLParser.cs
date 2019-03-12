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

using System.IO;
using System.Collections;
using System.Collections.Generic;


using TokenType = Panda.BTLTokenizer.TokenType;
namespace Panda
{

	public class PandaScriptException : System.Exception
	{
        string _message = null;
        string _filePath = null;
        int _line = 0;
        public string filePath { get { return _filePath; } }
        public int lineNumber { get { return _line; } }

		public PandaScriptException(string message, string filepath, int lineNumber)
			: base(message)
		{
            _message = message;
            _filePath = filepath;
            _line = lineNumber;
		}

        public override string Message
        {
            get
            {
                string str = null;
                if (_line != -1)
                    str = string.Format("{0} in file '{1}': line {2}", this._message, this.filePath, this.lineNumber);
                else
                    str = this._message;

                return str;
            }
        }


	}
	
	public class BTLParser
	{
	
		static Stack<Node> indentParents = new Stack<Node>();
        static Stack<Node> lineParents = new Stack<Node>();
        static Stack<int> indents = new Stack<int>();
		static int indent = 0;
		
		public class Node
		{
			public BTLTokenizer.Token token;
			public List<Node> children = new List<Node>();
			public List<BTLTokenizer.Token> parameters = new List<BTLTokenizer.Token>();
			public int parseLength = 0;

            Node[] _flattenChildren;
            public Node[] flattenChildren
            {
                get
                {
                    if(_flattenChildren == null)
                    {
                        // Get all nodes
                        var stack = new Stack<Node>();
                        var nodes = new List<Node>();
                        stack.Push(this);
                        while (stack.Count > 0)
                        {
                            var node = stack.Pop();
                            if (node == null)
                                continue;

                            nodes.Add(node);
                            for (int c = node.children.Count - 1; c >= 0; --c)
                            {
                                var child = node.children[c];
                                stack.Push(child);
                            }
                        }
                        _flattenChildren = nodes.ToArray();
                    }

                    return _flattenChildren;
                }
            }


            public object[] _parsedParameters;
            public object[] parsedParameters
            {
                get
                {
                    if( _parsedParameters == null)
                    {
                        List<object> parameters = new List<object>();
                        foreach (var p in this.parameters)
                            parameters.Add(BTLTokenizer.ParseParameter(p));
                        _parsedParameters = parameters.ToArray();
                    }
                    return _parsedParameters;
                }
            }


            public override string ToString()
            {
                string strParams = "";
                if(parameters.Count > 0)
                {
                    strParams += "(";
                    for(int i=0; i < parameters.Count; i++)
                    {
                        var p = parameters[i];
                        strParams += string.Format("{0}", p.ToString());
                        if (i + 1 < parameters.Count)
                            strParams += ", ";
                    }
                    strParams += ")";
                }
                return  token + strParams;
            }
			
		}


		
		public static Node[] ParseTokens( BTLTokenizer.Token[] tokens )
		{
            if( tokens== null || tokens.Length == 0)
            {
                string msg = string.Format("Invalid panda script.");
                throw new PandaScriptException(msg, null, -1);
            }

			Clear();
			List< Node> roots = new List<Node>();
			Node root = null;
			Node lastNode = null;
			bool parenthesis_opened = false;
			for( int i=0; i < tokens.Length; ++i)
			{
				var t = tokens[i];
				var node = new Node();
				node.token = t;

				switch( t.type )
				{
				// Indentation control
				case TokenType.Indent:
					++indent;
					break;
				case TokenType.EOL:
					indent=0;
                    lineParents.Clear();
                    lastNode = null;
					break;
				
                case TokenType.Value:
                        //lastNode.parameters.Add(t);
                   break;
				
				case TokenType.Parenthesis_Open:
					if( !parenthesis_opened )
					{
						parenthesis_opened = true;
					}else
					{
						string msg = "Unexpected open parenthesis";
						throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
					}
					break;
					
				case TokenType.Parenthesis_Closed:
					if( parenthesis_opened )
					{
						parenthesis_opened = false;
					}else
					{
						string msg = "Unexpected closed parenthesis";
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					lastNode.parseLength = t.substring_start - lastNode.token.substring_start + t.substring_length;
					break;

                // Tree node
                case TokenType.Tree:
                    if (indent == 0 && lineParents.Count == 0)
                    {
                        root = node;
                        indentParents.Clear();
                        indents.Clear();
                        roots.Add(root);
                        PushParent(node);
                    }
                    else
                    {
                        node.token.type = TokenType.TreeProxy;
                        // push to parent to detect parenting error.
                        PushParent(node);
                    }
                    break;

                // structural nodes
                case TokenType.Fallback:
                case TokenType.Sequence:
                case TokenType.Parallel:
                case TokenType.Random:
                case TokenType.Race:
                case TokenType.While:
                case TokenType.Repeat:
                case TokenType.Mute:
                case TokenType.Not:
                case TokenType.Word:// push to parent to detect parenting error.
                    PushParent(node);
                    break;

                }// switch


                // Skip blanks
                if ( t.type == TokenType.EOL || t.type == TokenType.Indent)
					continue;
					
				// Ignore comments
				if( t.type == TokenType.Comment )
				 continue;
				
				if ( t.type == TokenType.Parenthesis_Open || t.type == TokenType.Parenthesis_Closed || t.type == TokenType.Coma )
					continue;
					
				if( parenthesis_opened )
				{
					if(lastNode != null)
					{
						lastNode.parameters.Add( t );
					}	
				}
				else
				{

                    if (t.type == TokenType.Value)
                    {
                        if (lastNode != null)
                        {
                            lastNode.parameters.Add(t);
                            lastNode.parseLength = t.substring_start - lastNode.token.substring_start + t.substring_length;
                            continue;
                        }
                        else
                        {
                            string msg = "Unexpected parameter value.";
                            throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                        }
                    }


                    // Determine the parent of the current node
                    Node parent = null;
                    foreach ( var lineParent in  lineParents )
                    {
                        if ( lineParent != node )
                        {
                            parent = lineParent;
                            break;
                        }
                    }

                    if ((indentParents.Count > 0 && indentParents.Peek() != node) && lineParents.Count == 0)
                        PopParentToIndent(indent);

       
					if( parent == null)
					{
                        foreach (var indentParent in indentParents.ToArray())
                        {
                            if (indentParent != node)
                            {
                                parent = indentParent;
                                break;
                            }
                        }
					}

					// Add the node to the parent
					if( parent != null && !parent.children.Contains(node) )
						parent.children.Add( node );

                    if( parent == null && node.token.type != TokenType.Tree)
                    {
                        string msg = "Unexpected node type. Only tree nodes are expected on top hierarchy. (Indentation typo?)";
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					
					lastNode = node;
				}
			}
			

			foreach( var r in roots)
				CheckTree( r );

			return roots.ToArray();
		}
		
		
		static void Clear()
		{
			indentParents.Clear();
			indents.Clear();
            lineParents.Clear();
			indent = 0;
		}
		
		static void PopParentToIndent(int indent)
		{
			// Pop nodes from the stack until the indentations get aligned.
			while( indents.Count > 0 && indents.Peek() >= indent )
			{
				indents.Pop();
				indentParents.Pop();
			}
		}
		
		static void PushParent(Node parent)
		{
            if (lineParents.Count == 0) 
			{
				PopParentToIndent (indent);	
			
				// Push the node on the parent stack
				indentParents.Push (parent);
				indents.Push (indent);
			}

            switch( parent.token.type )
            {
                // structural nodes
                case TokenType.Fallback:
                case TokenType.Sequence:
                case TokenType.Parallel:
                case TokenType.Random:
                case TokenType.Race:
                case TokenType.While:
                case TokenType.Repeat:
                case TokenType.Mute:
                case TokenType.Not:
                    lineParents.Push(parent);
                    break;
            }

        }
		
        public static Node[] GetNodes(Node[] trees)
        {
            int count = 0;
            int i = 0;
            if (trees != null)
            {
                foreach (var tree in trees)
                {
                    if (tree != null)
                       count += tree.flattenChildren.Length;
                }
            }


            Node[] nodes = new Node[count];
            if (trees != null)
            {
                foreach (var tree in trees)
                {
                    if (tree != null)
                    {
                        var children = tree.flattenChildren;
                        foreach (var n in children)
                        {
                            nodes[i] = n;
                            i++;
                        }
                    }
                }
            }
            return nodes;
        }



        public static Node[] GetProxies(Node tree)
        {
            // Get all proxies
            var stack = new Stack<Node>();
            var nodes = new List<Node>();
            stack.Push(tree);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node == null)
                    continue;

                if( node.token.type == TokenType.TreeProxy)
                    nodes.Add(node);

                for (int c = node.children.Count - 1; c >= 0; --c)
                {
                    var child = node.children[c];
                    stack.Push(child);
                }
            }
            return nodes.ToArray();
        }

        public static Node[] GetProxies(Node[] trees)
        {
            var nodes = new List<Node>();
            if (trees != null)
            {
                foreach (var b in trees)
                    nodes.AddRange(GetProxies(b));
            }
            return nodes.ToArray();
        }
		

		public static void CheckProxies( Node[] roots, Node[][] rootSets )
		{
            CheckProxyDefinitions(roots, rootSets);
            CheckCircularDefinition(roots, rootSets);
		}

        public static void CheckTreeNames( Node[] trees, Node[][] treeSets )
        {
            if (trees == null || treeSets == null)
                return;

           for (int r = 0; r < trees.Length; ++r )
            {
                var tree = trees[trees.Length - r - 1];
                var treeName = GetTreeName(tree);

                for (int k = 0; k < treeSets.Length; ++k)
                {
                    var i = treeSets.Length - k - 1;
                    if (treeSets[i] == null)
                        continue;

                    for (int l = 0; l < treeSets[i].Length; ++l)
                    {
                        var j = treeSets[i].Length - l - 1;

                        var other = treeSets[i][j];
                        if (tree == other)
                            continue;
                        var otherName = GetTreeName(other);
                        if (otherName == treeName)
                        {
                            string msg = string.Format("Tree \"{0}\" is already defined.", treeName);
                            throw new PandaScriptException(msg, BTLTokenizer.filepath, tree.token.line);
                        }
                    }
                }
            }
        }
        public static void CheckMains(Node[] trees, Node[][] treeSets)
        {
            if (trees == null || treeSets == null)
                return;

            for (int r = 0; r < trees.Length; ++r)
            {
                int mainCount = 0;
                var tree = trees[trees.Length - r - 1];
                var treeName = GetTreeName(tree);

                if (treeName.ToLower() != "root")
                    continue;

                ++mainCount;

                for (int k = 0; k < treeSets.Length; ++k)
                {
                    var i = treeSets.Length - k - 1;
                    if (treeSets[i] == null)
                        continue;

                    for (int l = 0; l < treeSets[i].Length; ++l)
                    {
                        var j = treeSets[i].Length - l - 1;

                        var other = treeSets[i][j];
                        if (tree == other)
                            continue;

                        var otherName = GetTreeName(other);
                        if (otherName.ToLower() == "root")
                        {
                            string msg = string.Format("Tree \"{0}\" is already defined.", treeName);
                            throw new PandaScriptException(msg, BTLTokenizer.filepath, tree.token.line);
                        }
                    }
                }
            }
        }

        private static Node ResolveProxy(string proxyName, Node[][] rootSets)
        {
            Node proxy = null;
            foreach(var set in rootSets)
            {
                if( set != null )
                {
                    foreach(var bh in set)
                    {
                        if(bh != null )
                        {
                            string name = GetTreeName(bh);
                            if(name == proxyName)
                            {
                                proxy = bh;
                                break;
                            }
                        }
                    }
                }

                if (proxy != null)
                    break;
            }

            return proxy;
        }
        private static void CheckProxyDefinitions(Node[] trees, Node[][] treeSets)
        {
            // Check whether all sub trees are defined
            var proxies = BTLParser.GetProxies(trees);
            foreach (var proxy in proxies)
            {

                bool isDefined = false;
                string proxyName = GetTreeName(proxy);

                Node resolved = ResolveProxy(proxyName, treeSets);
                isDefined = resolved != null;

                if (!isDefined)
                {
                    string msg = string.Format("Tree \"{0}\" is not defined.", proxyName);
                    throw new PandaScriptException(msg, BTLTokenizer.filepath, proxy.token.line);
                }
            }
        }

        private static void CheckCircularDefinition(Node[] roots, Node[][] rootSets)
        {
            
            if (roots == null)
                return;

            for (int i = 0; i < roots.Length; ++i)
            {
                var root = roots[i];
                CheckCircularDefinition(root, rootSets);
            }


        }

        private static void CheckCircularDefinition(Node root, Node[][] rootSets)
        {
            var stack = new Stack<Node>();
            var depths = new Stack<int>();
            var path = new Stack<Node>();

            if (root != null)
            {
                stack.Push(root);
                depths.Push(0);
            }

            while (stack.Count > 0)
            {
                var tree = stack.Pop();
                var depth = depths.Pop();

                while( path.Count > depth )
                    path.Pop();

                path.Push(tree);
                

                var proxies = GetProxies(tree);
                foreach (var proxy in proxies)
                {
                    // resolve
                    var proxyName = proxy.parameters[0].ToString().Trim();
                    Node subTree = ResolveProxy(proxyName, rootSets);

                    if (!path.Contains(subTree))
                    {
                        stack.Push(subTree);
                        depths.Push(depth + 1);
                    }
                    else
                    {

                        path.Push(subTree);

                        var treeName = GetTreeName(subTree);
                        string msg = string.Format("Tree \"{0}\" is circularly defined. Circular tree definition is invalid.\n", treeName);

                        string callPath = "";
                        var pathArray = path.ToArray();
                        for (int i = 0; i < pathArray.Length; ++i)
                        {
                            var j = pathArray.Length - i - 1;

                            var n = pathArray[j];

                            var name = GetTreeName(n);
                            callPath += string.Format("/{0}", name);
                        }
                        msg += string.Format("call path: \"{0}\" ", callPath);

                        throw new PandaScriptException(msg, BTLTokenizer.filepath, subTree.token.line);
                    }
                }
            }

        }

        private static string GetTreeName(Node tree)
        {
            return BTLTokenizer.ParseParameter(tree.parameters[0]).ToString();
        }

        static void CheckTree(Node root)
		{
			var nodes =  root.flattenChildren;
            foreach( var n in nodes)
			{
                var t = n.token;

				switch( n.token.type )
				{
				case TokenType.Word:
                        // Action has no child
                        if (n.children.Count != 0)
                        {
                            string msg = string.Format("Task node has {0} children. None is expected", n.children.Count);
                            throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                        }

					break;
				
				case TokenType.While:
					if (n.children.Count != 2)
					{
						string msg = string.Format("While node has {0} children. Two are expected", n.children.Count);
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					break;
	
				case TokenType.Parallel:
					// Parallel node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Parallel node has no child. One or more is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }			
					break;
					
				case TokenType.Tree:

					if (n.parameters.Count != 1)
					{
						string msg = string.Format("Tree naming error. Tree name is expected as parameter of type string");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }

					// Root node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
                        string msg = string.Format("Tree node has no child. One is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					
					// Root node must have one child and it must be a task.
					if( n.children.Count > 1 )
					{
                        string msg = string.Format("Tree node has too many children. Only one is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
					}
                    break;
				
				case TokenType.TreeProxy:

					if (n.parameters.Count != 1)
					{
						string msg = string.Format("Tree naming error. Tree name is expected as parameter of type string");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }

					// Root node must have one child and it must be a task.
					if (n.children.Count > 0)
					{
						string msg = string.Format("Tree reference has children. None is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }

				break;
					
				case TokenType.Fallback:
					// Fallback node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Fallback node has no child. One or more is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }			
					break;
					
				case TokenType.Sequence:
					// Sequence node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Sequence node has no child. One or more is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					break;

				case TokenType.Race:
					// Race node must have one child and it must be a task.
					if (n.children.Count == 0)
					{
						string msg = string.Format("Race node has no child. One or more is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					break;

				case TokenType.Random:
					// Random node must have one child and it must be a task.
					if (n.children.Count == 0)
					{
						string msg = string.Format("Random node has no child. One or more is expected");
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
					break;


                case TokenType.Repeat:
                    // Repeat node must have one child and it must be a task.
                    if (n.children.Count != 1)
                    {
                        string msg = string.Format("Repeat node has {0} children. One is expected", n.children.Count);
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
                    break;

                case TokenType.Not:
                    // Not node must have one child.
                    if (n.children.Count != 1)
                    {
                        string msg = string.Format("Not node has {0} children. One is expected", n.children.Count);
                        throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                    }
                    break;
                case TokenType.Mute:
                        // Mute node must have one child.
                        if (n.children.Count != 1)
                        {
                            string msg = string.Format("Mute node has {0} children. One is expected", n.children.Count);
                            throw new PandaScriptException(msg, BTLTokenizer.filepath, t.line);
                        }
                        break;


                }
            }
			
	
			
		}
		
		public static string ToString (Node tree)
		{
			string strout = "";
			// ASCII Tree
			var fifo = new Stack<Node> ();
			var indents = new Stack<int> ();
			int i = 0;
			fifo.Push (tree);
			indents.Push (i);
			
			while (fifo.Count > 0) 
			{
				var node = fifo.Pop ();
				int indent = indents.Pop ();
				string line = "";
				for (int t = 0; t < indent; ++t)
					line += "-";
				line += node.ToString ();
				strout += line + "\n";
				for (int c = node.children.Count - 1; c >= 0; --c) 
				{
					var child = node.children [c];
					fifo.Push (child);
					indents.Push (indent + 1);
				}
			}
			return strout;
		}
		
		
	
	}
}
