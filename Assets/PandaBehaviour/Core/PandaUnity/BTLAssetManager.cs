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
using System.Security.Cryptography;
using System;

namespace Panda
{
    /*BT script source abstraction*/
    public abstract class BTSource
    {
        public abstract string source { get; }
        public abstract string url { get;  }
    }

    public class BTSourceTextAsset : BTSource
    {
        public BTSourceTextAsset( TextAsset textAsset )
        {
            this.textAsset = textAsset;
        }

        public override string source
        {
            get
            {
                return textAsset.text;
            }
        }

        public override string url
        {
            get
            {
                string url = null;
#if UNITY_EDITOR
                    url = UnityEditor.AssetDatabase.GetAssetPath(textAsset);
#else
                    url = textAsset.name;
#endif
                return url ;
            }
        }

        public TextAsset textAsset;
    }

    public class BTSourceString : BTSource
    {
        string _text;
        string _url;
        public BTSourceString( string text )
        {
            _text = text;
        }

        public BTSourceString(string text, string url)
        {
            _text = text;
            _url = url;
        }

        public override string source
        {
            get
            {
                return _text;
            }
        }

        public override string url
        {
            get
            {
                if (_url != null)
                    return _url;
                else
                    return GetHash();
            }
        }

        static SHA256Managed sha1 = null;
        string GetHash()
        {
            List<byte[]> hashes = new List<byte[]>();

            if( sha1 == null )
                sha1 = new SHA256Managed();

            // convert string to a byte[]
            byte[] bytes = new byte[_text.Length * sizeof(char)];
            System.Buffer.BlockCopy(_text.ToCharArray(), 0, bytes, 0, bytes.Length);

            var hashBytes = sha1.ComputeHash(bytes);
            hashes.Add(hashBytes);

            return System.Text.Encoding.UTF8.GetString( hashBytes );
        }


    }



    public class BTLAssetManager 
    {

        public static BTLTokenizer.Token[] GetBTLTokens(BTSource[] texts, BTSource text)
        {
            BTLTokenizer.Token[] tokens = null;
            BTLCache btlCache = Fetch(texts);
			for (int i = 0; i < texts.Length; i++ )
			{
				if (texts[i] == text &&  i < btlCache.tokenSets.Length)
					tokens = btlCache.tokenSets[i];
			}
            return tokens;
        }




		public static BTLParser.Node[] GetBTLRoots(BTSource[] texts, BTSource text)
        {
            BTLParser.Node[] roots = null;
			BTLCache btlCache = Fetch( texts );
			for (int i = 0; i < texts.Length; i++)
			{
				if (texts[i] == text)
					roots = btlCache.rootSets[i];
			}
			return roots;
        }


		public static BTLTokenizer.Token[][] GetBTLTokens(BTSource[] texts)
		{
			BTLTokenizer.Token[][] tokenSets = null;
			BTLCache btlCache = Fetch(texts);
			tokenSets = btlCache.tokenSets;
			return tokenSets;
		}

		public static BTLParser.Node[][] GetBTLRoots(BTSource[] texts)
		{
			BTLParser.Node[][] rootSets = null;
			BTLCache btlCache = Fetch(texts);
			rootSets = btlCache.rootSets;
			return rootSets;
		}

		public static bool HasCompilationErros(BTSource[] texts, BTSource text)
		{
			bool hasErrors = false;
			BTLCache btlCache = Fetch(texts);
			for (int i = 0; i < texts.Length; i++)
			{
				if (texts[i] == text)
					hasErrors = btlCache.exceptions[i] != null;
			}
			return hasErrors;
		}

		public static bool HasCompilationErrors(BTSource[] texts)
		{
			bool hasErrors = false;
			BTLCache btlCache = Fetch(texts);
			for (int i = 0; i < texts.Length; i++)
			{
				if (btlCache.exceptions[i] != null)
				{
					hasErrors = true;
				}
			}
			return hasErrors;
		}

        public static System.Exception[] GetCompilationExceptions(BTSource[] texts)
        {
            var exceptions = new List<System.Exception>();
            BTLCache btlCache = Fetch(texts);
            for (int i = 0; i < texts.Length; i++)
            {
                if (btlCache.exceptions[i] != null)
                {
                    exceptions.Add(btlCache.exceptions[i]);
                }
            }
            return exceptions.ToArray();
        }



        static bool isPandaInitialized = false;
        static void InitializePanda()
        {
            BTRandom.randomValue = () => UnityEngine.Random.value;
        }

        public static BTProgram CreateBehaviourProgram( BTSource[] btlSources )
        {
            if( !isPandaInitialized  )
            {
                InitializePanda();
                isPandaInitialized = true;
            }

            BTProgram program = null;

            BTLParser.Node[][] nodeSets = null;

            var cache = Fetch(btlSources);

            var exceptions = cache.exceptions;

            // A program is correct when it does not contain any exceptions.
            bool isCorrect = true;
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    isCorrect = false;
                    break;
                }
            }

            if ( isCorrect ) 
            {
                nodeSets = GetBTLRoots(btlSources);
                program = BTRuntimeBuilder.BuildProgram( nodeSets );
            }
            

            return program;
        }


        static BTLCache Fetch(BTSource[] texts)
        {
            BTLCache btlCache = null;

            bool updateCache = true;
			string key = GetKey(texts);

            if( cache.ContainsKey( key ) )
            {
                btlCache = cache[key];
                var newHashes = GetHashes(texts);
                if( AreSameHashes( newHashes, btlCache.hashes  ) )
                {
                    updateCache = false;
                }
            }

            if( updateCache )
            {
                Cache( texts );
                btlCache = cache[ GetKey(texts) ];
            }

            return btlCache;
        }

        static void Cache(BTSource[] sources)
		{
		    BTLCache btlCache = new BTLCache();

			int n = sources.Length;
			string[] strSources = new string[n];
			string[] filepaths  = new string[n];
			btlCache.hashes = new byte[n][];
			btlCache.exceptions = new PandaScriptException[n];

			for(int i=0; i < n; i++)
			{
                if (sources[i] != null)
                {
                    strSources[i] = sources[i].source;
                    filepaths[i] = sources[i].url;
                    btlCache.hashes[i] = GetHash(strSources[i]);
                    btlCache.exceptions[i] = null;
                }
                else
                {
                    strSources[i] = null;
                    filepaths[i] = null;
                    btlCache.hashes[i] = null;
                    btlCache.exceptions[i] = null;

                }
			}

			btlCache.tokenSets = Tokenize(strSources, filepaths, ref btlCache.exceptions);
            btlCache.rootSets = ParseTokens(btlCache.tokenSets, filepaths, ref btlCache.exceptions);

            cache[GetKey(sources)] = btlCache;
        }

		static string GetKey(BTSource[] sources)
		{
			string key = "";

            /*
			List<int> ids = new List<int>();
			foreach (var s in sources)
			{
                var id = s != null?s.GetInstanceID(): 0;
				ids.Add( id );
			}

			foreach (var id in ids)
				key += string.Format("{0}_", id);
            */
            foreach (var s in sources)
                key += string.Format("{0}_", s.url);

            return key;
		}

        static Dictionary<string, BTLCache> cache = new Dictionary<string, BTLCache>();
    
        class BTLCache
        {
            public byte[][] hashes;
            public BTLTokenizer.Token[][] tokenSets;
            public BTLParser.Node[][] rootSets;
			public PandaScriptException[] exceptions = null;
        }


		public static BTLTokenizer.Token[][] Tokenize(string[] sources, string[] filepaths, ref PandaScriptException[] exceptions)
		{
			List<BTLTokenizer.Token[]> tokenSets = new List<BTLTokenizer.Token[]>();
			for (int i = 0; i < sources.Length; i++)
			{
				if (sources[i] == null)
                {
                    tokenSets.Add(null);
                    continue;
                }

				if (filepaths != null && i < filepaths.Length)
					BTLTokenizer.filepath = filepaths[i];
				try
				{
					tokenSets.Add(BTLTokenizer.Tokenize(sources[i]));
				}
                catch( PandaScriptException e )
				{
                    tokenSets.Add( null );
                    exceptions[i] = e;
					Debug.LogError(e);
				}
			}

			return tokenSets.ToArray();
		}

		public static BTLParser.Node[][] ParseTokens(BTLTokenizer.Token[][] tokenSets, string[] filepaths,  ref PandaScriptException[] hasErrors)
		{
			List<BTLParser.Node[]> nodeSets = new List<BTLParser.Node[]>();
			for (int i = 0; i < tokenSets.Length; i++)
			{
				var tokens = tokenSets[i];
                if (tokens == null)
                {
                    nodeSets.Add( null );
                    continue;
                }

				try
				{
                    BTLTokenizer.filepath = filepaths[i];
                    var tree = BTLParser.ParseTokens(tokens);
					nodeSets.Add(tree);
				}
                catch( Panda.PandaScriptException e)
                {
                    hasErrors[i] = e;
                    nodeSets.Add( null );
                }
			}

			// Check whether all behaviour are defined.
			for (int i = 0; i < nodeSets.Count; i++)
			{
				var roots = nodeSets[i];
				try
				{
                    BTLTokenizer.filepath = filepaths[i];
                    BTLParser.CheckMains(roots, nodeSets.ToArray());
                    BTLParser.CheckTreeNames(roots, nodeSets.ToArray());
					BTLParser.CheckProxies(roots, nodeSets.ToArray());
				}
				catch (PandaScriptException e)
				{
					hasErrors[i] = e;
                }
			}


			return nodeSets.ToArray();
		}

        internal static byte[] GetHash(string str)
        {

            // convert string to a byte[]
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

            var sha1 = new SHA256Managed();
            var hashBytes =  sha1.ComputeHash(bytes);

            return hashBytes;
        }

		internal static byte[][] GetHashes(BTSource[] texts)
		{
			List<byte[]> hashes = new List<byte[]>();
			var sha1 = new SHA256Managed();

			foreach (var t in texts)
			{
				if (t != null)
				{
					string str = t.source;
					// convert string to a byte[]
					byte[] bytes = new byte[str.Length * sizeof(char)];
					System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

					var hashBytes = sha1.ComputeHash(bytes);
					hashes.Add(hashBytes);
				}else
				{
					hashes.Add(null);
				}
			}

			return hashes.ToArray();
		}

		static bool AreSameHashes(byte[][] a, byte[][] b)
		{
			bool areSame = true;
			if (a != null && b != null && a.Length == b.Length)
			{
				for (int i = 0; i < a.Length; i++)
				{
					if ( !IsSameHash(a[i], b[i]) )
					{
						areSame = false;
						break;
					}
				}
			}
			else if( a != null && b != null)
			{
				areSame = false;
			}
			return areSame;

		}



        static bool IsSameHash( byte[] a, byte[] b)
        {
            bool isSame = true;
			if ( a != null && b != null && a.Length == b.Length )
            {
                for(int i=0; i < a.Length; i++)
                {
                    if( a[i] != b[i])
                    {
                        isSame = false;
                        break;
                    }
                }
			}
			else if (a != null && b != null)
            {
                isSame = false;
            }
            return isSame;
        }


    }

}


