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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;



namespace Panda
{
	public class BTLTokenizer 
	{
		public static string filepath= "";
		
		public enum TokenType
		{
			Word,
			Coma,
			Comment,
			EOL,
			Fallback,
			Indent,
			Not,
			Mute,
			Parallel,
			Parenthesis_Open,
			Parenthesis_Closed,
			Race,
			Random,
			Tree,
			TreeProxy,
			Repeat,
			Sequence,
            Value,
			While
		}

        public enum TokenValueType{
            None,
            Boolean,
            Integer,
            Float,
            String,
            Enum
        }

        public class Token
        {
            public Token(TokenType type, int start, int length, string source, int line, TokenValueType valueType)
            {
                this.type = type;
                this.substring_start = start;
                this.substring_length = length;
                this.source = source;
                this.line = line;
                this.valueType = valueType;
            }

            public Token(TokenType type, int start, int length, string source, int line)
                : this( type, start, length, source, line, TokenValueType.None)
            {
            }

            public TokenType type;
            public TokenValueType valueType = TokenValueType.None;
            public int substring_start;
            public int substring_length;
            public string _source;
            public string source
            {
                get
                {
                    return _source;
                }
                set
                {
                    _source = value;
                    _parsedParameter = null;
                }
            }
            public int line;

            public override string ToString()
            {
                string str = "";
                if (type == TokenType.Word)
                    str = content;
                else if (type == TokenType.EOL)
                    str = "[EOL]\n";
                else if (type == TokenType.Value)
                    str = BTLTokenizer.ParseParameter(this).ToString();
                else
                    str = string.Format("[{0}]", type.ToString());
                return str;
            }

            public string content
            {
                get
                {
                    string content = null;
                    if( source != null && substring_start + substring_length <= source.Length)
                        content = source.Substring(substring_start, substring_length);
                    return content;
                }
            }

            object _parsedParameter;
            public object parsedParameter
            {
                get
                {
                    if( _parsedParameter == null)
                    {
                        var str = this.content.Trim();

                        switch( this.valueType )
                        {
                            case TokenValueType.Float:
                                _parsedParameter = float.Parse(str,  CultureInfo.InvariantCulture.NumberFormat);
                                break;
                            case TokenValueType.Integer:
                                _parsedParameter = int.Parse(str);
                                break;
                            case TokenValueType.Boolean:
                                _parsedParameter = str == "true";
                                break;
                            case TokenValueType.String:
                                _parsedParameter = str.Substring(1, str.Length - 2);
                                break;
                            case TokenValueType.Enum:
                                _parsedParameter = new EnumParameter(str);
                                break;
                        }

                    }
                    return _parsedParameter;
                }
            }

		}
		
		
		
        public static string CleanBlanks( string source)
        {
            string src = source;



            // Do some source clean up
            src = src.Replace("\r\n", "\n"); // Let's process only one kind of EOL.
            src = src.Replace("\r", "\n");


			var sb = new StringBuilder ();

			bool isIdenting = true;
			int spaceCount = 0;
			foreach (var c in src) 
			{
				if(c == '\n')
				{
					isIdenting = true;
					spaceCount = 0;
					sb.Append( c );
					continue;
				}

				if( c == ' ' )
					spaceCount++;
				else
					isIdenting = false;

				if( isIdenting  )
				{
					if( spaceCount == 2)
					{
						sb.Append('\t');
                        spaceCount = 0;
                    }
				}
				else
				{
					sb.Append( c );
				}
			}
            
            return sb.ToString();
        }



		public static Token[] Tokenize( string source )
		{
            string src = CleanBlanks(source);

			List<Token> tokens = new List<Token>();
			int start = 0;
			Token token = null;
			int line = 1;
			for(int i=0; i < src.Length; ++i)
			{
				int len = i - start;
				char c = src[i];
				
				if( c == '"' )
				{
					do
					{
						++i;
						if( i < src.Length )
						{
							len = i - start;
							c = src[i];
						}
					}while(c != '"' && i < src.Length && c != '\n');
					
					if( c == '\n')
					{
						token = new Token( TokenType.EOL, start, len, src, line);
						throw new PandaScriptException("Expected double-quotes before end-of-lines", BTLTokenizer.filepath, token.line);
					}
					
				}
                bool EOF = !(i + 1 < src.Length);

                if( EOF )
                    ++len;


				string word = src.Substring(start, len);


				// Inlines comments
				if (word.EndsWith( "//" ))
				{
					do
					{
						if (i < src.Length)
						{
							len = i - start;
							c = src[i];
                            if(c != '\n')
                                ++i;
						}
					} while (i < src.Length && c != '\n');

                    if (c != '\n')
                        ++len; 


					token = new Token(TokenType.Comment, start, len, src, line);
					tokens.Add(token);
                    start = i;

					if( c == '\n')
                        i--;
					
                    token = null;
					continue;

				}

				// block comments	
				if (word.EndsWith("/*"))
				{
					bool hasHitEOC = false; // EOC = End Of Comment '*/'
					do
					{
						++i;

                        if (i < src.Length)
                        {
                            c = src[i];
                            if (c == '\n')
                                ++line;
                        }

						len = i - start;
						word = src.Substring(start, len);

						if (word.Length > 2)
						{
							hasHitEOC = word.EndsWith( "*/" );
						}

					} while (i < src.Length && !hasHitEOC);

					token = new Token(TokenType.Comment, start, len, src, line);

					if (!hasHitEOC)
					{
						throw new PandaScriptException("End-of-file found. Expected '*/'", BTLTokenizer.filepath, token.line);
					}
					tokens.Add(token);
					token = null;
					start = i;
					continue;

				}

                
                if (len > 0 && (c == '\t' || c == '\n' || c == ' ' || c == '(' || c == ')' || c == ',' || EOF ))
				{
                    char lastChar = word[word.Length - 1];

                    if (word.Length > 0 && (lastChar == '\n' || lastChar == ')'))
                    {
                        --len;
                        word = src.Substring(start, len);
                    }

					string lc_word = word.Trim().ToLower();
					
					if( lc_word.Trim() != "")
					{
						switch( lc_word )
						{
						case "fallback": token = new Token( TokenType.Fallback, start, len, src, line); break;
						case "not": token = new Token( TokenType.Not, start, len, src, line); break;
						case "parallel": token = new Token( TokenType.Parallel, start, len, src, line); break;
						case "repeat": token = new Token( TokenType.Repeat, start, len, src, line); break;
						case "race": token = new Token(TokenType.Race, start, len, src, line); break;
						case "random": token = new Token(TokenType.Random, start, len, src, line); break;
						case "tree": token = new Token(TokenType.Tree, start, len, src, line); break;
						case "sequence": token = new Token( TokenType.Sequence, start, len, src, line); break;
						case "while": token = new Token( TokenType.While, start, len, src, line); break;
						case "mute": token = new Token( TokenType.Mute, start, len, src, line); break;
						
						default:
                                var tokenType = GetValueType(word);
                                if( tokenType != TokenValueType.None ) 
                                    token = new Token(TokenType.Value, start, len, src, line, tokenType);
                                else 
                                    token = new Token(TokenType.Word, start, len, src, line);
                            break;
						}

                        if (token != null)
                        {
                            tokens.Add(token);
                            token = null;
                        }
						start = i;
					}
				}


				if (token == null)
				{
					len = i - start + 1;

					switch (c)
					{
						case '\t': token = new Token(TokenType.Indent, start, len, src, line); break;
						case '\n': token = new Token(TokenType.EOL, start, len, src, line); ++line; break;
						case ' ': token = null;  start = i; break;
						case ',': token = new Token(TokenType.Coma, start, len, src, line); break;
						case '(': token = new Token(TokenType.Parenthesis_Open, start, len, src, line); break;
						case ')': token = new Token(TokenType.Parenthesis_Closed, start, len, src, line); break;
					}
				}

                if (c == ' ' || c== '\t' || c == '\n' )
					 start = i;


				
				if( token != null )
				{
					tokens.Add( token );
					token = null;
					start = i+1;
				}
			
			}


			
			return tokens.ToArray();
		}
		

        public static TokenValueType GetValueType(string content)
        {
            TokenValueType valueType = TokenValueType.None;
            string str = content.Trim();

            float f;
            int i;
            // (float.TryParse(comboCurrencyValue.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,out currency)&& float.TryParse(txtYourValue.Text,out inputValue))
            if (str == "true" || str == "false") valueType = TokenValueType.Boolean;
            else if (str.Contains(".") && float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) valueType = TokenValueType.Float;
            else if (int.TryParse(str, out i)) valueType = TokenValueType.Integer;
            else if (str.StartsWith("\"") && str.EndsWith("\"")) valueType = TokenValueType.String;
            else if (str.Contains(".") && enumRegex.Match(str).Success) valueType = TokenValueType.Enum;

            return valueType;
        }


		public static string ToString(Token[] tokens)
		{
			var sb = new StringBuilder();
			foreach(var t in tokens)
			{
				sb.Append(t.ToString());
			}
			return sb.ToString();
		}

		public static string ToString(Token[][] tokenSets)
		{
			var sb = new StringBuilder();
			foreach (var s in tokenSets)
			{
				sb.Append(ToString(s));
				sb.Append("-----------------------------\n");
			}
			return sb.ToString();
		}

        public static object ParseParameter(BTLTokenizer.Token token)
        {
            object o = null;
            o = token.parsedParameter;
            return o;
        }

        static Regex enumRegex   = new Regex(@"^[a-zA-Z]+[a-zA-Z0-9_.]*\.[a-zA-Z0-9_.]+$");

    }
}
