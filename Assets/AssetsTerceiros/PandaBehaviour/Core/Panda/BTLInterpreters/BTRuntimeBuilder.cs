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

using TokenType = Panda.BTLTokenizer.TokenType;

namespace Panda
{

    public class BTRuntimeBuilder
	{

        public static BTProgram BuildProgram(BTLParser.Node[][] treeSets)
        {
            BTProgram program = new BTProgram();

            int n = treeSets.Length;
            program._treeSets = new BTTree[n][];
            program._codemaps = new CodeMap[n];

            for (int i = 0; i < n; ++i)
            {
                program._treeSets[i] = Build(treeSets[i], out program._codemaps[i], true);
            }

            List<BTTree> trees = new List<BTTree>();
            foreach (var set in program.treeSets)
                trees.AddRange(set);

            BTRuntimeBuilder.ResolveProxies(trees.ToArray());

            return program;
        }

		static BTTree[] Build( BTLParser.Node[] trees, out CodeMap codemap, bool createCodeMap )
		{
            var codeMapDict = new Dictionary<BTNode, SubstringLocation>();
            List<BTTree> roots = new List<BTTree>();

			BTTree root = null;
			// Get all nodes
			var parseNodes = BTLParser.GetNodes(trees);
			
			var nodeID = new Dictionary< BTLParser.Node, int>();
			for(int i=0; i < parseNodes.Length;++i)
			{
				nodeID[parseNodes[i]] = i;
			}
			
			
			// create nodes
			var nodes = new BTNode[ parseNodes.Length ];
			foreach( var n in parseNodes )
			{
				int id  = nodeID[n];

                switch (n.token.type)
				{
				case TokenType.Tree: 
					var newRoot = new BTTree();

                    if (n.parameters.Count > 0)
                    {
                        newRoot.name = n.parsedParameters[0] as string;
                    }

                    nodes[id] = newRoot;
                    root = newRoot;
                    roots.Add(root);
					
					break;
				
				case TokenType.TreeProxy: 
					var rootProxy = new BTTreeProxy();

                    if (n.parameters.Count > 0)
                    {
                        rootProxy.name = n.parsedParameters[0] as string;
                    }
					
					nodes[id] = rootProxy;
					
					break;


                case TokenType.Value:
                case TokenType.Word:
					
					var task = new BTTask();
					task.taskName = n.token.content.Trim();

					nodes[id] = task;
					
				 	break;
				 	
				case TokenType.Mute:
					nodes[id] = new BTMute();
					break;
					
				case TokenType.Not:
				 	nodes[id] = new BTNot();
				 	break;
				
				case TokenType.Repeat:
					nodes[id] = new BTRepeat();
					break;
					
				case TokenType.Fallback: 	nodes[id] = new BTFallback(); break;
				case TokenType.Sequence: 	nodes[id] = new BTSequence(); break;
				case TokenType.Parallel: 	nodes[id] = new BTParallel(); break;
				case TokenType.While: 	nodes[id] = new BTWhile(); break;
				case TokenType.Race: nodes[id] = new BTRace(); break;
				case TokenType.Random: nodes[id] = new BTRandom(); break;
				}
				


				if( nodes[id] != null)
				{
					nodes[id].i_parameters = n.parsedParameters;
				}

				if( nodes[id] != null )
				{
					var loc = new SubstringLocation();
                    loc.line = n.token.line;
					loc.start = n.token.substring_start;
					loc.length = n.token.substring_length;

					if( n.parameters.Count > 0)
					{
						loc.length = n.parseLength;
					}

					codeMapDict[nodes[id]] = loc;
				}
				
			}
			
			
			// parenting
			foreach( var n in parseNodes )
			{
				int pid = nodeID[n];
				var parent = nodes[pid];
				if( parent != null )
				{
					foreach( var c in n.children)
					{
						int cid = nodeID[c];
						var child = nodes[cid];
						if( child != null )
							parent.AddChild( child );
					}
				}
			}

            if (roots.Count > 0)
                root = roots[0];

            if (createCodeMap)
                codemap = new CodeMap(codeMapDict);
            else
                codemap = null;


			return roots.ToArray();
			
		}

        static BTTask[] GetAllTasks(BTTree root )
        {
            List<BTTask> taskList = new List<BTTask>();
            taskList.AddRange(root.tasks);

            // Retrieve tasks from proxies too
            var proxies = root.proxies;
            foreach (var proxy in proxies)
            {
                foreach (var a in proxy.target.tasks)
                {
                    if (!taskList.Contains(a))
                        taskList.Add(a);
                }
            }
            return taskList.ToArray();
        }

        static bool Locate( BehaviourTree behaviourTree, BTTask task, out string path, out int line)
        {

            bool found = false;
            path = null;
            line = 0;
            var program = behaviourTree.program;
            for(int c =0; c < program.codemaps.Length; c++ )
            {
                var cm = program.codemaps[c];
                for ( int i=0; i < cm.nodes.Length; i++)
                {
                    if( cm.nodes[i] == task)
                    {
                        path = behaviourTree.btSources[c].url;
                        line = cm.substringLocations[i].line;
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;
            }

            return found;

        }

		public static void Bind( BTProgram program, object[] implementers )
		{

            // Get all the [Task]s.
            var taskImplementations = TaskImplementation.Get( implementers );

            // Find for each task the corresponding implementations.
            var taskImplementationCandidates = new Dictionary<BTTask, List<TaskImplementation> >();
            var tasks = program.tasks;
            foreach (var task in tasks)
            {
                taskImplementationCandidates[task] = new List<TaskImplementation>();
                foreach( var ti in taskImplementations )
                {
                    if( ti.IsMatch( task ))
                    {
                        taskImplementationCandidates[task].Add(ti);
                    }
                }
            }

            // Check whether all the task are unambiguously defined.
            var unimplementedTasks = new List<BTTask>();
            var over_implementedTasks = new List<BTTask>();

            foreach( var kv  in taskImplementationCandidates)
            {
                var task = kv.Key;
                var candidates = kv.Value;

                if (candidates.Count == 0)
                    unimplementedTasks.Add(task);

                if (candidates.Count > 1)
                    over_implementedTasks.Add(task);
            }

            // Generate eventual exceptions.

            var exceptions = new List<System.Exception>();
            if ( unimplementedTasks.Count > 0 )
			{
                int line = 0;
                string path = null;
				foreach( var task in unimplementedTasks)
				{
                    var m = GetMethodSignatureString(task);
                    string msg = string.Format("The task `{0}' is not defined.\n", m);
                    msg += string.Format("\nA method as follow is expected:\n");

                    string tpl = taskTpl;
					tpl = tpl.Replace("{!methodSignature}",  m);
					tpl = tpl.Replace("{!statusType}",  "Status");
					msg += tpl;

                    if (Locate(BehaviourTree._current, task, out path, out line))
                    {
                        exceptions.Add(new PandaScriptException( msg, path, line) );
                    }
                    else
                    {
                        exceptions.Add(new System.NotImplementedException(msg));
                    }
				}
			}
			
			
			if( over_implementedTasks.Count > 0 )
			{
                int line = 0;
                string path = null;
                foreach ( var task in over_implementedTasks)
				{
                    var m = GetMethodSignatureString(task);
                    string msg = string.Format("The task `{0}' has too many definitions.\n", m);
                    msg += string.Format("The task is implemented in:\n");

					foreach( var o in taskImplementationCandidates[task] )
					{
                        msg += string.Format(" - `{0}'\n",  o.target.GetType().ToString());
					}

                    if (Locate(BehaviourTree._current, task, out path, out line))
                    {
                        exceptions.Add(new PandaScriptException(msg, path, line));
                    }
                    else
                    {
                        exceptions.Add(new System.Exception(msg));
                    }
                }
			}

            if( exceptions.Count == 0)
                exceptions.AddRange(CheckRootTree(program));

            program._exceptions = exceptions.ToArray();

            // Bind the tasks.
            foreach (var kv in taskImplementationCandidates)
            {
                var task = kv.Key;
                var candidates = kv.Value;

                if (candidates.Count == 1)
                {
                    var ti = candidates[0];
                    Bind(task, ti);
                }else
                {
                    task.Unbind();
                }
            }
		}

        static bool Bind( BTTask task, TaskImplementation implementation)
        {
            bool bindingSucceeded = false;

            var member = implementation.memberInfo;
            var taskImplementer = implementation.target;

            MethodInfo method = member as MethodInfo;
            FieldInfo field = member as FieldInfo;
            PropertyInfo property = member as PropertyInfo;

            System.Action taskAction = null;
            if (method != null)
            {
                object[] parameters = new object[task.i_parameters.Length];
                var methodParameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    var enumParameter = task.i_parameters[i] as EnumParameter;
                    if (enumParameter != null)
                    {
                        parameters[i] = enumParameter.Parse(methodParameters[i].ParameterType);
                    }
                    else
                    {
                        parameters[i] = task.i_parameters[i];
                    }
                }
                
                if (method.ReturnType == typeof(void))
                {
                    if (parameters.Length > 0)
                        taskAction = () => method.Invoke(taskImplementer, parameters);
                    else
                        taskAction = System.Delegate.CreateDelegate(typeof(System.Action), taskImplementer, method) as System.Action;
                }

                if (method.ReturnType == typeof(bool))
                {
                    if (parameters.Length > 0)
                    {
                        taskAction = () => Task.current.Complete((bool)method.Invoke(taskImplementer, parameters));
                    }
                    else
                    {
                        var del = System.Delegate.CreateDelegate(typeof(System.Func<bool>), taskImplementer, method) as System.Func<bool>;
                        taskAction = () => Task.current.Complete(del());
                    }

                }

            }

            if (field != null)
                taskAction = () => Task.current.Complete((bool)field.GetValue(taskImplementer));

            if (property != null)
                taskAction = () => Task.current.Complete((bool)property.GetValue(taskImplementer, null));

            if (taskAction != null)
            {
                task.m_boundObject = taskImplementer;
                task.m_boundMember = member;
                task.m_taskAction = taskAction;
                task.m_boundState = BoundState.Bound;

                bindingSucceeded = true;
            }

            return bindingSucceeded;
        }

        public static System.Exception[] CheckRootTree( BTProgram program )
        {
            var exceptions = new List<System.Exception>();
            var treesSets = program.treeSets;

            int mainCount = 0;

            if (treesSets != null)
            {
                for (int i = 0; i < treesSets.Length; ++i)
                {
                    var set = treesSets[i];
                    if (set != null)
                    {
                        for (int j = 0; j < set.Length; ++j)
                        {
                            var b = set[j];
                            if (b.name.ToLower() == "root")
                                ++mainCount;
                        }
                    }
                }
            }

            if( mainCount == 0)
            {
                string msg = string.Format("No root tree  found. tree \"Root\" is expected.\n");
                exceptions.Add(new System.Exception(msg));
            }

            if (mainCount > 1)
            {
                string msg = string.Format("Too many root trees found. Only one tree \"Root\" is expected.\n");
                exceptions.Add(new System.Exception(msg));
            }


            if( exceptions.Count > 0 )
            {
                program._boundState = BoundState.Failed;
                program._codemaps = null;
            }

            return exceptions.ToArray();
        }
		
		static string GetMethodSignatureString( BTTask task )
		{
			var parameterArray = task.i_parameters;
			// expected method signature 
			string sign = "void" + " " + task.taskName + "(";
			if( parameterArray != null)
			{
				for(int p=0; p < parameterArray.Length; ++p)
				{
					sign += " " + parameterArray[p].GetType().ToString();
					
					sign += " p" + p.ToString();
					
					if( p +1 < parameterArray.Length ) sign += ",";
				}
				sign += " " ;
			}
			sign += ")";
			sign = sign.Replace("System.Single", "float");
			sign = sign.Replace("System.Int32", "int");
			sign = sign.Replace("System.Boolean", "bool");
			sign = sign.Replace("System.String", "string");
			return sign;
		}
		


        public static void ResolveProxies(BTTree[] trees)
        {
            foreach (var tree in trees)
            {
                if (tree != null)
                {
                    // Resolve proxies
                    var proxies = tree.proxies;
                    foreach (var proxy in proxies)
                    {
                        foreach (var t in trees)
                        {
                            if (t.name == proxy.name)
                                proxy.target = t;
                        }
                    }
                }
            }
        }


        public static BTTree GetMain( BTTree[] _trees )
        {
            BTTree main = null;
            if (_trees != null && _trees.Length > 0)
            {
                // Search for the main tree
                foreach (var bt in _trees)
                {
                    if (bt.name.ToLower() == "root")
                    {
                        main = bt;
                    }
                }

                if (main == null)
                    main = _trees[0];

            }

            return main;
        }
		
		const string taskTpl = @"
            [Task]
            {!methodSignature}
            {
                var task = Task.current;

                if( task.isStarting )
                {// Use this for initialization 
                    
                }

                throw new System.NotImplementedException();
            }
";
	}

    public class TaskImplementation
    {
        public object target;
        public MemberInfo memberInfo;

        System.Type[] _parameterTypes;

        public System.Type[] parameterTypes
        {
            get
            {
                if (_parameterTypes == null)
                {
                    var list = new List<System.Type>();
                    var methodInfo = memberInfo as MethodInfo;
                    if (methodInfo != null)
                    {
                        var pars = methodInfo.GetParameters();
                        foreach (var p in pars)
                        {
                            list.Add(p.ParameterType);
                        }
                    }
                    _parameterTypes = list.ToArray();
                }
                return _parameterTypes;
            }
        }

        
        public bool IsMatch(BTTask task)
        {
            if (task.name != memberInfo.Name)
                return false;

            bool itIs = true;



            var method = memberInfo as MethodInfo;
            var field = memberInfo as FieldInfo;
            var property = memberInfo as PropertyInfo;


            if (method != null)
            {
                var thisTypes = parameterTypes;
                var otherTypes = task.parameterTypes;
                if (thisTypes.Length == otherTypes.Length)
                {
                    for (int i = 0; i < thisTypes.Length; i++)
                    {
                        if(thisTypes[i].IsEnum && otherTypes[i] == typeof(EnumParameter))
                        {
                            EnumParameter enumParameter = (EnumParameter)task.parameters[i];
                            itIs = enumParameter.Matches(thisTypes[i]);
                        }else
                        if (thisTypes[i] != otherTypes[i])
                        {
                            itIs = false;
                            break;
                        }
                    }
                }
                else
                {
                    itIs = false;
                }
            }
            else
            if (field != null)
            {
                if (field.FieldType != typeof(bool))
                    itIs = false;
            }
            else
            {
                if (field != null && property.PropertyType != typeof(bool))
                {
                    if (property.PropertyType != typeof(bool))
                        itIs = false;
                }
            }

            return itIs;
        }


        public static readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.GetField;
        static Dictionary<System.Type, Task[]> taskCache = new Dictionary<System.Type, Task[]>();
        public static TaskImplementation[] Get(object[] implementers)
        {
            var tasks = new List<TaskImplementation>();

            foreach (var implementer in implementers)
            {

                Task[] typeTasks = null;

                var type = implementer.GetType();

                if (taskCache.ContainsKey(type))
                {
                    typeTasks = taskCache[type];
                }
                else
                {
                    var typeTaskList = new List<Task>();
                    var members = implementer.GetType().GetMembers(bindingFlags);
                    foreach (var m in members)
                    {
                        var attributes = m.GetCustomAttributes(typeof(Task), true);
                        foreach (var attr in attributes)
                        {
                            var implementationAttribute = attr as Task;
                            if (implementationAttribute != null)
                            {
                                implementationAttribute._memberInfo = m;
                                typeTaskList.Add(implementationAttribute);
                            }
                        }
                    }

                    typeTasks = typeTaskList.ToArray();
                    taskCache[type] = typeTasks;
                }

                if (typeTasks != null)
                {
                    foreach (var t in typeTasks)
                    {
                        var ti = new TaskImplementation();
                        ti.target = implementer;
                        ti.memberInfo = t._memberInfo;
                        tasks.Add(ti);
                    }
                }

            }

            return tasks.ToArray();
        }

        string _toString = null;
        public override string ToString()
        {
            if (_toString == null)
            {
                var typeName = this.target.GetType().Name;
                var t = this;
                var sb = new System.Text.StringBuilder();
                sb.Append(typeName);
                sb.Append("/");
                


                sb.AppendFormat("{0}", t.memberInfo.Name);
                if (t.parameterTypes.Length > 0)
                    sb.Append("( ");
                for(int i=0; i <  t.parameterTypes.Length; i++)
                {
                    var p = parameterTypes[i];
                    if( i > 0)
                        sb.Append(", ");
                    if (p.IsEnum)
                    {
                        sb.AppendFormat("{0}", p.FullName.Replace("+", "."));
                    }
                    else
                        sb.AppendFormat("{0}", p.Name);
                }
                if (t.parameterTypes.Length > 0)
                    sb.Append(" )");

                _toString = sb.ToString();

                _toString = _toString.Replace("System.Single", "float");
                _toString = _toString.Replace("System.Int32", "int");
                _toString = _toString.Replace("System.Boolean", "bool");
                _toString = _toString.Replace("System.String", "string");

                _toString = _toString.Replace("Single", "float");
                _toString = _toString.Replace("Int32", "int");
                _toString = _toString.Replace("Boolean", "bool");
                _toString = _toString.Replace("String", "string");

            }
            return _toString;
        }
    }

}
