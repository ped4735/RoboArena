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
using System.Text;

namespace Panda
{
	public class BTLSyntaxHighlight
	{
        static GUISkin skin = null;
        // edit view
        static Color keywordColor = new Color(0.5f, 0.0f, 0.5f, 1.0f);// purple
        static Color commentColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);// White
        static Color valueColor = new Color(0.0f, 0.5f, 0.5f, 1.0f); //  Turqoise
        static Color taskColor = Color.black;

        // live view
        static Color readyColor = Color.grey; // grey
        static Color failedColor = new Color(0.7f, 0.0f, 0.0f); // red
        static Color succeededColor = new Color(0.0f, 0.5f, 0.0f); // green
        static Color runningColor = new Color(0.0f, 0.0f, 0.9f); // blue

        static List<GUIStyle> styles = new List<GUIStyle>();

        static GUIStyle _style_label;
        static readonly string _style_label_name = "Panda Label";
        public static GUIStyle style_label
        {
            get
            {
                if (_style_label == null)
                {
                    _style_label = new GUIStyle( GetBuiltinGUIStyleByName("PR Label"));
                    _style_label.richText = false;
                    _style_label.wordWrap = false;
                    _style_label.stretchWidth = false;
                    _style_label.stretchHeight = false;
                    _style_label.alignment = TextAnchor.UpperLeft;
                    //_style_label.fixedHeight = 0.0f;
                    _style_label.fixedWidth = 0.0f;
                    _style_label.padding = new RectOffset(0, 0, 0, 0);
                    _style_label.overflow = new RectOffset(0, 0, 0, 0);
                    _style_label.contentOffset = Vector2.zero;
                    _style_label.margin = new RectOffset(0, 0, 0, 0);
                    _style_label.clipping = TextClipping.Clip;


                    _style_label.onActive.background = style_toolbarbutton.active.background;
                    _style_label.onHover.background = style_toolbarbutton.active.background;
                    _style_label.onFocused.background = style_toolbarbutton.active.background;
                    _style_label.onNormal.background = style_toolbarbutton.active.background;

                    _style_label.focused.background = style_toolbarbutton.active.background;
                    _style_label.active.background = style_toolbarbutton.active.background;
                    _style_label.hover.background = style_toolbarbutton.active.background;
                    _style_label.name = _style_label_name;

                }
                return _style_label;
            }
        }


        public static GUIStyle GetTokenStyle(BTLTokenizer.Token token)
        {
            var style = style_label;

            switch (token.type)
            {
                case BTLTokenizer.TokenType.Comment: style = style_comment; break;
                case BTLTokenizer.TokenType.Sequence: style = style_keyword; break;
                case BTLTokenizer.TokenType.Fallback: style = style_keyword; break;
                case BTLTokenizer.TokenType.While: style = style_keyword; break;
                case BTLTokenizer.TokenType.Repeat: style = style_keyword; break;
                case BTLTokenizer.TokenType.Tree: style = style_keyword; break;
                case BTLTokenizer.TokenType.Random: style = style_keyword; break;
                case BTLTokenizer.TokenType.TreeProxy: style = style_keyword; break;
                case BTLTokenizer.TokenType.Mute: style = style_keyword; break;
                case BTLTokenizer.TokenType.Parallel: style = style_keyword; break;
                case BTLTokenizer.TokenType.Not: style = style_keyword; break;
                case BTLTokenizer.TokenType.Race: style = style_keyword; break;
                case BTLTokenizer.TokenType.Value: style = style_value; break;
                case BTLTokenizer.TokenType.Word: style = style_task; break;
            }

            return style;
        }




        static GUIStyle _style_INFoldout;
        static readonly string _style_INFoldout_name = "Panda Foldout";

        public static GUIStyle style_INFoldout
        {
            get
            {
                if (_style_INFoldout == null)
                {
                    _style_INFoldout = new GUIStyle( GetBuiltinGUIStyleByName("IN Foldout") );
                    
                    _style_INFoldout.richText = false;
                    _style_INFoldout.wordWrap = false;
                    _style_INFoldout.stretchWidth = false;
                    _style_INFoldout.stretchHeight = false;
                    _style_INFoldout.alignment = TextAnchor.UpperLeft;
                    _style_INFoldout.fixedHeight = 0.0f;
                    _style_INFoldout.fixedWidth = 0.0f;
                    _style_INFoldout.padding = new RectOffset(0, 0, 0, 0);
                    _style_INFoldout.overflow = new RectOffset(0, 0, 0, 0);
                    _style_INFoldout.margin = new RectOffset(0, 0, 0, 0);


                    _style_INFoldout.name = _style_INFoldout_name;
                }
                return _style_INFoldout;
            }
        }

        static GUIStyle _style_INFoldin;
        static readonly string _style_INFoldin_name = "Panda Foldin";

        public static GUIStyle style_INFoldin
        {
            get
            {
                if (_style_INFoldin == null)
                {
                    _style_INFoldin = new GUIStyle(style_INFoldout);

                    _style_INFoldin.normal.background = Rotate( _style_INFoldin.normal.background );
                    _style_INFoldin.focused.background = Rotate(_style_INFoldin.focused.background);
                    _style_INFoldin.hover.background = Rotate(_style_INFoldin.hover.background);
                    _style_INFoldin.active.background = Rotate(_style_INFoldin.active.background);


                    _style_INFoldin.name = _style_INFoldin_name;
                }
                return _style_INFoldin;
            }
        }



        static GUIStyle _style_breakpoint_set_running;
        static readonly string _style_breakpoint_set_running_name = "Panda Break Point Set Running";
        public static GUIStyle style_breakpoint_set_running
        {
            get
            {
                if (_style_breakpoint_set_running == null)
                {
                    _style_breakpoint_set_running = new GUIStyle(style_lineNumber);
                    _style_breakpoint_set_running.normal.textColor = Color.yellow;
                    _style_breakpoint_set_running.hover.textColor = Color.gray;
                    _style_breakpoint_set_running.normal.background =
                    _style_breakpoint_set_running.hover.background =
                    _style_breakpoint_set_running.active.background
                    = Resources.Load("Panda_Break_Point.running") as Texture2D;

                    _style_breakpoint_set_running.name = _style_breakpoint_set_running_name;

                }
                return _style_breakpoint_set_running;
            }
        }

        static GUIStyle _style_breakpoint_set_succeeded;
        static readonly string _style_breakpoint_set_succeeded_name = "Panda Break Point Set Succeeded";
        public static GUIStyle style_breakpoint_set_succeeded
        {
            get
            {
                if (_style_breakpoint_set_succeeded == null)
                {
                    _style_breakpoint_set_succeeded = new GUIStyle(style_breakpoint_set_running);
                    _style_breakpoint_set_succeeded.normal.background =
                    _style_breakpoint_set_succeeded.hover.background =
                    _style_breakpoint_set_succeeded.active.background
                    = Resources.Load("Panda_Break_Point.succeeded") as Texture2D;

                    _style_breakpoint_set_succeeded.name = _style_breakpoint_set_succeeded_name;

                }
                return _style_breakpoint_set_succeeded;
            }
        }


        static GUIStyle _style_breakpoint_set_failed;
        static readonly string _style_breakpoint_set_failed_name = "Panda Break Point Set Failed";
        public static GUIStyle style_breakpoint_set_failed
        {
            get
            {
                if (_style_breakpoint_set_failed == null)
                {
                    _style_breakpoint_set_failed = new GUIStyle(style_breakpoint_set_running);
                    _style_breakpoint_set_failed.normal.background =
                    _style_breakpoint_set_failed.hover.background =
                    _style_breakpoint_set_failed.active.background
                    = Resources.Load("Panda_Break_Point.failed") as Texture2D;

                    _style_breakpoint_set_failed.name = _style_breakpoint_set_failed_name;

                }
                return _style_breakpoint_set_failed;
            }
        }


        static GUIStyle _style_breakpoint_active;
        static readonly string _style_breakpoint_active_name = "Panda Break Point Active";

        public static GUIStyle style_breakpoint_active
        {
            get
            {
                if (_style_breakpoint_active == null)
                {
                    _style_breakpoint_active = new GUIStyle(style_breakpoint_set_running);
                    _style_breakpoint_active.normal.textColor = keywordColor;
                    _style_breakpoint_active.normal.background =
                    _style_breakpoint_active.hover.background =
                    _style_breakpoint_active.active.background
                    = Resources.Load("Panda_Break_Point.active") as Texture2D;

                    _style_breakpoint_active.name = _style_breakpoint_active_name;

                }
                return _style_breakpoint_active;
            }
        }



        static GUIStyle _style_lineNumber;
        static readonly string _style_lineNumber_name = "Panda Line Number";

        public static GUIStyle style_lineNumber
        {
            get
            {
                if (_style_lineNumber == null)
                {
                    _style_lineNumber = new GUIStyle( style_label );
                    _style_lineNumber.normal.textColor = BTLSyntaxHighlight.commentColor;
                    _style_lineNumber.alignment = TextAnchor.UpperRight;
                    _style_lineNumber.fixedWidth = _style_lineNumber.CalcSize( new  GUIContent(  "0000" )).x;
                    _style_lineNumber.name = _style_lineNumber_name;
                }
                return _style_lineNumber;
            }
        }

        static GUIStyle _style_lineNumber_active;
        static readonly string _style_lineNumber_active_name = "Panda Line Number Active";

        public static GUIStyle style_lineNumber_active
        {
            get
            {
                if (_style_lineNumber_active == null)
                {
                    _style_lineNumber_active = new GUIStyle(style_lineNumber);
                    _style_lineNumber_active.normal.textColor = taskColor;
                    _style_lineNumber_active.normal.background = style_toolbarbutton.active.background;
                    _style_lineNumber_active.name = _style_lineNumber_active_name;
                }
                return _style_lineNumber_active;
            }
        }

        static GUIStyle _style_lineNumber_error;
        static readonly string _style_lineNumber_error_name = "Panda Line Number Error";
        public static GUIStyle style_lineNumber_error
        {
            get
            {
                if (_style_lineNumber_error == null)
                {
                    _style_lineNumber_error = new GUIStyle(style_lineNumber);
                    _style_lineNumber_error.normal.textColor = failedColor;
                    _style_lineNumber_error.normal.background = GetBuiltinIconByName("UnityGUIClip");
                    _style_lineNumber_error.name = _style_lineNumber_error_name;
                }
                return _style_lineNumber_error;
            }
        }



        static GUIStyle _style_keyword;
        static readonly string _style_keyword_name = "Panda Keyword";
        public static GUIStyle style_keyword
        {
            get
            {
                if (_style_keyword == null)
                {
                    _style_keyword = new GUIStyle(style_label);
                    _style_keyword.normal.textColor = keywordColor;
                    _style_keyword.name = _style_keyword_name;
                }
                return _style_keyword;
            }
        }

        static GUIStyle _style_comment;
        static readonly string _style_comment_name = "Panda comment";
        public static GUIStyle style_comment
        {
            get
            {
                if (_style_comment == null)
                {
                    _style_comment = new GUIStyle(style_label);
                    _style_comment.normal.textColor = commentColor;
                    _style_comment.name = _style_comment_name;
                }
                return _style_comment;
            }
        }

        static GUIStyle _style_value;
        static readonly string _style_value_name = "Panda value";

        public static GUIStyle style_value
        {
            get
            {
                if (_style_value == null)
                {
                    _style_value = new GUIStyle(style_label);
                    _style_value.normal.textColor = valueColor;
                    _style_value.name = _style_value_name;
                }
                return _style_value;
            }
        }

        static GUIStyle _style_task;
        static readonly string _style_task_name = "Panda task";
        public static GUIStyle style_task
        {
            get
            {
                if (_style_task == null)
                {
                    _style_task = new GUIStyle(style_label);
                    _style_task.normal.textColor = taskColor;
                    _style_task.name = _style_task_name;
                }
                return _style_task;
            }
        }

        static GUIStyle _style_ready;
        static readonly string _style_ready_name = "Panda ready";

        public static GUIStyle style_ready
        {
            get
            {
                if (_style_ready == null)
                {
                    _style_ready = new GUIStyle(style_label);
                    _style_ready.normal.textColor = readyColor;
                    _style_ready.name = _style_ready_name;
                }
                return _style_ready;
            }
        }


        static GUIStyle _style_failed;
        static readonly string _style_failed_name = "Panda failed";
        public static GUIStyle style_failed
        {
            get
            {
                if (_style_failed == null)
                {
                    _style_failed = new GUIStyle(style_label);
                    _style_failed.normal.textColor = failedColor;
                    _style_failed.name = _style_failed_name;
                }
                return _style_failed;
            }
        }

        static GUIStyle _style_succeeded;
        static readonly string _style_succeeded_name = "Panda succeeded";

        public static GUIStyle style_succeeded
        {
            get
            {
                if (_style_succeeded == null)
                {
                    _style_succeeded = new GUIStyle(style_label);
                    _style_succeeded.normal.textColor = succeededColor;
                    _style_succeeded.name = _style_succeeded_name;
                }
                return _style_succeeded;
            }
        }

        static GUIStyle _style_running;
        static readonly string _style_running_name = "Panda running";

        public static GUIStyle style_running
        {
            get
            {
                if (_style_running == null)
                {
                    _style_running = new GUIStyle(style_label);
                    _style_running.normal.textColor = runningColor;
                    _style_running.name = _style_running_name;
                }
                return _style_running;
            }
        }


        static GUIStyle _style_toolbarbutton;
        public static GUIStyle style_toolbarbutton
        {
            get
            {
                if (_style_toolbarbutton == null)
                    _style_toolbarbutton = GetBuiltinGUIStyleByName("toolbarbutton");
                return _style_toolbarbutton;
            }
        }

        static GUIStyle _style_selected;
        public static GUIStyle style_selected
        {
            get
            {
                if (_style_selected == null)
                {
                    _style_selected = new GUIStyle(style_label);

                    _style_selected.normal.textColor =
                    _style_selected.focused.textColor =
                    _style_selected.hover.textColor =
                    _style_selected.active.textColor = Color.white;

                    _style_selected.normal.background =
                    _style_selected.focused.background =
                    _style_selected.hover.background =
                    _style_selected.active.background = GetBuiltinIconByName("MenuItemHover");
                }
                return _style_selected;
            }
        }

        static GUIStyle _style_insert_above;
        public static GUIStyle style_insert_above
        {
            get
            {
                if (_style_insert_above == null)
                    _style_insert_above = GetBuiltinGUIStyleByName("PR Insertion Above");
                return _style_insert_above;
            }
        }

        static GUIStyle _style_insert_below;
        public static GUIStyle style_insert_below
        {
            get
            {
                if (_style_insert_below == null)
                    _style_insert_below = GetBuiltinGUIStyleByName("PR Insertion");
                return _style_insert_below;
            }
        }

        static GUIStyle _style_insert_in;
        public static GUIStyle style_insert_in
        {
            get
            {
                if (_style_insert_in == null)
                {
                    _style_insert_in = new GUIStyle(style_label);
                    _style_insert_in.normal.background =
                    _style_insert_in.focused.background =
                    _style_insert_in.hover.background =
                    _style_insert_in.active.background = GetBuiltinIconByName("PR DropHere");
                }
                return _style_insert_in;
            }
        }

        static Texture _texture_insert_LeftRigth;
        public static Texture texture_insert_LeftRigth
        {
            get
            {
                if(_texture_insert_LeftRigth == null)
                {
                    //_texture_insert_LeftRigth = Rotate( GetBuiltinIconByName("PR DropHere2"));
                    _texture_insert_LeftRigth = GetBuiltinIconByName("dragdot");
                }
                return _texture_insert_LeftRigth;
            }
        }


        static Texture2D GetBuiltinIconByName(string name)
        {
            Texture2D icon = null;
            var icons = Resources.FindObjectsOfTypeAll(typeof(Texture2D));
            foreach (var i in icons)
            {
                if (i.name == name)
                {
                    icon = i as Texture2D;
                    break;
                }
            }
            return icon;
        }

        static GUISkin GetSkinByName(string name)
        {
            GUISkin skin = null;
            var skins = Resources.FindObjectsOfTypeAll(typeof(GUISkin));

            foreach (var o in skins)
            {
                var s = o as GUISkin;
                if (s.name == name)
                    skin = s;
            }

            return skin;
        }
            
        static GUIStyle GetBuiltinGUIStyleByName(string name)
        {
           GUIStyle style = null;

           GUISkin skin =  GetSkinByName("LightSkin");

            var styles = skin != null? skin.customStyles: null;
            if (styles != null)
            {
                foreach (var s in styles)
                {
                    if (s.name == name)
                    {
                        style = s;
                        break;
                    }
                }
            }

            if (style == null)
            {
                Debug.LogWarning(string.Format("No style named '{0}' found.", name));
                style = new GUIStyle();
            }

            return style;
        }

        static GUIStyle GetGUIStyleByName(string name)
        {
            GUIStyle style = null;

            GUISkin skin = BTLSyntaxHighlight.skin;

            var styles = skin != null ? skin.customStyles : null;
            if (styles != null)
            {
                foreach (var s in styles)
                {
                    if (s.name == name)
                    {
                        style = s;
                        break;
                    }
                }
            }
            return style;
        }



        static Texture2D Rotate( Texture2D inTex )
        {

            if (inTex == null)
                return null;

            var format = TextureFormat.ARGB32;

            Texture2D readTex = new Texture2D(inTex.width, inTex.height, format, true);
            var rdrTex = new RenderTexture(inTex.width, inTex.height, 0, RenderTextureFormat.Default);
            var old = RenderTexture.active;
            RenderTexture.active = rdrTex;
            Graphics.Blit(inTex, rdrTex);
            readTex.ReadPixels(new Rect(0, 0, rdrTex.width, rdrTex.height), 0, 0);
            RenderTexture.active = old;

            readTex.Apply();

            var inPixels = readTex.GetPixels();
            var outPixels = new Color[inTex.width*inTex.height];
            for (int j = 0; j < inTex.height; j++)
            {
                for (int i = 0; i < inTex.width; i++)
                {
                    int x = j;
                    int y = inTex.width - 1 - i;
                    outPixels[inTex.height * y +  x] = inPixels[inTex.width * j + i];
                }
            }

            Texture2D outTex = new Texture2D(inTex.height, inTex.width, format, true);

            outTex.SetPixels(outPixels);
            outTex.Apply();
            return outTex;

        }
        static Dictionary<Status, GUIStyle> _statusStyle;
        public static Dictionary<Status, GUIStyle> statusStyle
        {
            get
            {
                if (_statusStyle == null)
                {
                    _statusStyle = new Dictionary<Status, GUIStyle>()
                    {
                        { Status.Ready , BTLSyntaxHighlight.style_ready },
                        { Status.Running, BTLSyntaxHighlight.style_running },
                        { Status.Succeeded , BTLSyntaxHighlight.style_succeeded },
                        { Status.Failed, BTLSyntaxHighlight.style_failed },
                    };
                }

                return _statusStyle;
            }
        }

        public static void InitializeColors()
        {
#if UNITY_EDITOR
            if ( UnityEditor.EditorGUIUtility.isProSkin)
                DarkSkinTheme();
            else
                LightSkinTheme();
#else
                LightSkinTheme();
#endif
            _style_breakpoint_active = null;
            _style_breakpoint_set_running = null;
            _style_comment = null;
            _style_failed = null;
            _style_INFoldin = null;
            _style_INFoldout = null;
            _style_keyword = null;
            _style_label = null;
            _style_lineNumber = null;
            _style_lineNumber_active = null;
            _style_lineNumber_error = null;
            _style_ready = null;
            _style_running = null;
            _style_succeeded = null;
            _style_task = null;
            _style_toolbarbutton = null;
            _style_value = null;

        }

        public static void Load( GUISkin skin )
        {
            if (BTLSyntaxHighlight.skin != skin)
            {
                BTLSyntaxHighlight.skin = skin;
                _style_breakpoint_active = GetGUIStyleByName(_style_breakpoint_active_name);
                _style_breakpoint_set_running = GetGUIStyleByName(_style_breakpoint_set_running_name); ;
                _style_comment = GetGUIStyleByName(_style_comment_name); ;
                _style_failed = GetGUIStyleByName(_style_failed_name); ;
                _style_INFoldin = GetGUIStyleByName(_style_INFoldin_name); ;
                _style_INFoldout = GetGUIStyleByName(_style_INFoldout_name); ;
                _style_keyword = GetGUIStyleByName(_style_keyword_name); ;
                _style_label = GetGUIStyleByName(_style_label_name); ;
                _style_lineNumber = GetGUIStyleByName(_style_lineNumber_name); ;
                _style_lineNumber_active = GetGUIStyleByName(_style_lineNumber_active_name); ;
                _style_lineNumber_error = GetGUIStyleByName(_style_lineNumber_error_name); ;
                _style_ready = GetGUIStyleByName(_style_ready_name); ;
                _style_running = GetGUIStyleByName(_style_running_name); ;
                _style_succeeded = GetGUIStyleByName(_style_succeeded_name); ;
                _style_task = GetGUIStyleByName(_style_task_name); ;
                _style_value = GetGUIStyleByName(_style_value_name);
            }

        }


#if UNITY_EDITOR && EXTRACT_STYLES
		static string skinFolder = "Assets/Skins";
#endif

		public static void InitializeStyles()
        {
            styles.Clear();
            GUIStyle s = null;
            s = style_breakpoint_active; styles.Add(s);
            s = style_breakpoint_set_running; styles.Add(s);
            s = style_comment; styles.Add(s);
            s = style_failed; styles.Add(s);
            s = style_INFoldin; styles.Add(s);
            s = style_INFoldout; styles.Add(s);
            s = style_keyword; styles.Add(s);
            s = style_label; styles.Add(s);
            s = style_lineNumber; styles.Add(s);
            s = style_lineNumber_active; styles.Add(s);
            s = style_lineNumber_error; styles.Add(s);
            s = style_ready; styles.Add(s);
            s = style_running; styles.Add(s);
            s = style_succeeded; styles.Add(s);
            s = style_task; styles.Add(s);
            s = style_toolbarbutton; styles.Add(s);
            s = style_value; styles.Add(s);

#if UNITY_EDITOR && EXTRACT_STYLES
			if (UnityEditor.AssetDatabase.FindAssets("t:GUISkin PandaLightSkin").Length == 0 )
            {

				var wd =  System.IO.Directory.GetCurrentDirectory();
				var fulldirpath = System.IO.Path.Combine(wd, skinFolder );
				if( !System.IO.Directory.Exists(  fulldirpath ) )
				{
					System.IO.Directory.CreateDirectory( fulldirpath );
				}

                var skin = ScriptableObject.CreateInstance(typeof(GUISkin)) as GUISkin;
                skin.name = "PandaLightSkin";
                skin.customStyles = styles.ToArray();
                skin.hideFlags = HideFlags.None;

                foreach(var st in styles)
                {
                    SaveStyleBackground(st, skin);
                }

                UnityEditor.AssetDatabase.CreateAsset(skin, string.Format("{0}/PandaLightSkin.guiskin", skinFolder));
            }
#endif
		}

#if UNITY_EDITOR && EXTRACT_STYLES
        static void SaveStyleBackground( GUIStyle s, GUISkin skin )
        {
			s.normal.background = CreateStyleTextureAsset( skin, s, s.normal.background, "normal");
			s.focused.background = CreateStyleTextureAsset( skin,  s, s.focused.background, "focused");
			s.hover.background = CreateStyleTextureAsset( skin,  s, s.hover.background, "hover");
			s.active.background = CreateStyleTextureAsset( skin,  s, s.active.background, "active");

            s.onNormal.background = CreateStyleTextureAsset(skin, s, s.onNormal.background, "onNormal");
            s.onFocused.background = CreateStyleTextureAsset(skin, s, s.onFocused.background, "onFocused");
            s.onHover.background = CreateStyleTextureAsset(skin, s, s.onHover.background, "onHover");
            s.onActive.background = CreateStyleTextureAsset(skin, s, s.active.background, "onActive");



        }

        static Texture2D CreateStyleTextureAsset(GUISkin skin, GUIStyle style, Texture2D tex, string styleState  )
		{
			Texture2D newTex = null;
			if( tex != null )
			{
				newTex = Duplicate( tex );
                newTex.hideFlags = HideFlags.None;
				var png = newTex.EncodeToPNG();

				string filename = string.Format("{0}.{1}.{2}.png", skin.name,style.name  ,styleState  ).Replace(" ", "_");
				//UnityEditor.AssetDatabase.CreateAsset(newTex, string.Format("{0}/{1}", skinFolder, filename));
				var fullpath = Application.dataPath.Replace("Assets", "") + skinFolder + "/" + filename;
				System.IO.File.WriteAllBytes(fullpath, png);
				UnityEditor.AssetDatabase.Refresh();

				newTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>( skinFolder + "/" + filename );
                newTex.hideFlags = HideFlags.None;

				Debug.Log( fullpath );

			}
			return newTex;
		}

		static Texture2D Duplicate( Texture2D tex )
		{
			Texture2D newTex = null;
			if( tex != null)
			{
				var rdrTex = new RenderTexture( tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
				Graphics.Blit( tex, rdrTex);
				newTex = new Texture2D(rdrTex.width, rdrTex.height, TextureFormat.ARGB32, false);
				var old = RenderTexture.active;
				RenderTexture.active = rdrTex;
				newTex.ReadPixels( new Rect(0,0, rdrTex.width, rdrTex.height), 0,0);
				newTex.Apply();
				RenderTexture.active = old;

			}
			return newTex;

		}
#endif
        static void DarkSkinTheme()
        {
            // static
            BTLSyntaxHighlight.keywordColor = HexCol(0xd381ceff);
            BTLSyntaxHighlight.commentColor = HexCol(0xc2c2c2ff);
            BTLSyntaxHighlight.valueColor = HexCol(0xda9051ff);
            BTLSyntaxHighlight.taskColor = HexCol(0xffffffff);

            // liveview
            BTLSyntaxHighlight.readyColor = HexCol(0x686868ff);
            BTLSyntaxHighlight.failedColor = HexCol(0xd67979ff);
            BTLSyntaxHighlight.succeededColor = HexCol(0x7fd48dff);
            BTLSyntaxHighlight.runningColor = HexCol(0x9d9dffff);

        }


        static void LightSkinTheme()
        {
            // static
            BTLSyntaxHighlight.keywordColor = HexCol(0x8b007eff);
            BTLSyntaxHighlight.commentColor = HexCol(0x5a5a5aff);
            BTLSyntaxHighlight.valueColor = HexCol(0x9b440aff);
            BTLSyntaxHighlight.taskColor = HexCol(0x000000ff);

            // liveview
            BTLSyntaxHighlight.readyColor = HexCol(0x8b8b8bff);
            BTLSyntaxHighlight.failedColor = HexCol(0x981414ff);
            BTLSyntaxHighlight.succeededColor = HexCol(0x2a8828ff);
            BTLSyntaxHighlight.runningColor = HexCol(0x3841b6ff);


        }

        static Color HexCol(uint hex)
        {
            Color col = new Color();
            col.r = ((float)((0xFF000000 & hex) >> 8 * 3)) / 255.0f;
            col.g = ((float)((0x00FF0000 & hex) >> 8 * 2)) / 255.0f;
            col.b = ((float)((0x0000FF00 & hex) >> 8 * 1)) / 255.0f;
            col.a = ((float)((0x000000FF & hex) >> 8 * 0)) / 255.0f;
            return col;
        }


    }

}

