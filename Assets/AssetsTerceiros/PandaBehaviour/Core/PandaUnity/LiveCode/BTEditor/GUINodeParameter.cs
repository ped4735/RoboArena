using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace Panda
{
    public class GUINodeParameter
    {

        public event System.Action OnChange;
        System.Type _type;
        public System.Type type { get { return _type; } }

        public Rect rect;
        public string value = null;
        string oldValue = null;

        public bool _isEdited;
        public bool _isEditedAfterRepaint;
        public System.Type _enumType;
        public bool isEdited
        {
            get
            {
                if (GUIBTScript.isEventSafe)
                    _isEdited = _isEditedAfterRepaint;
                return _isEdited;
            }

            set
            {
                if (_isEditedAfterRepaint == value)
                    return;

                if (!_isEditedAfterRepaint && value)
                {
                    oldValue = this.value;
                }


                if (_isEditedAfterRepaint && !value)
                {
                    if (oldValue != this.value || oldValue == null)
                        DoOnChange();
                    oldValue = this.value;

                }
                _isEditedAfterRepaint = value;

            }
        }

        public GUINodeParameter(System.Type type, string value)
        {
            this._type = type;
            this.value = value;
        }

        public GUINodeParameter(BTLTokenizer.Token token)
        {
            var v = BTLTokenizer.ParseParameter(token);
            value = token.content;

            _type = v.GetType();
        }

        public GUINodeParameter(System.Type type)
        {
            if (type == typeof(bool)) value = "false";
            if (type == typeof(int)) value = "0";
            if (type == typeof(float)) value = "0.0";
            if (type == typeof(string)) value = "\"\"";

            if ( type.IsEnum )
            {
                var vals = System.Enum.GetNames(type);
                value = type.FullName.Replace("+", ".") + "." + vals[0];
            }

            this._type = type;
        }

        readonly string[] boolOptions = new string[] { "false", "true" };
        public void OnGUI(bool isComment)
        {
            var p = this;
            var style = isComment? BTLSyntaxHighlight.style_comment: BTLSyntaxHighlight.style_value;
            if (p.isEdited)
            {
#if UNITY_EDITOR
                var size = style.CalcSize(new GUIContent(p.value));

                if (p._type == typeof(bool))
                {
                    var v = p.value.ToLower() == "true" ? 1 : 0;
                    v = UnityEditor.EditorGUILayout.Popup(v, boolOptions, style, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(60.0f) );
                    p.value = v == 1 ? "true" : "false";
                }
                else
                if (p._type == typeof(int))
                {
                    var v = UnityEditor.EditorGUILayout.IntField(int.Parse(p.value), style, GUILayout.ExpandWidth(false), GUILayout.Width(size.x));
                    p.value = v.ToString();
                }
                else
                if (p._type == typeof(float))
                {
                    var v =  UnityEditor.EditorGUILayout.FloatField(float.Parse(p.value), style, GUILayout.ExpandWidth(false), GUILayout.Width(size.x));
                    p.value = string.Format("{0:0.0#############}", v);
                }
                else
                if (p._type == typeof(string))
                {
                    if (isComment)
                    {
                        p.value = UnityEditor.EditorGUILayout.TextField(p.value, style, GUILayout.ExpandWidth(false), GUILayout.MinWidth(10.0f), GUILayout.Width(size.x));
                    }
                    else
                    {

                        var v = p.value.Trim();
                        size = style.CalcSize(new GUIContent(v));

                        v = v.Substring(1, v.Length - 2);
                        GUILayout.Label("\"", style);
                        p.value = '"' +
                           UnityEditor.EditorGUILayout.TextField(v, style, GUILayout.ExpandWidth(false), GUILayout.Width(size.x))
                            + '"';
                        GUILayout.Label("\"", style);
                    }
                }
                if (p._type == typeof(Panda.EnumParameter))
                {
                    if( p._enumType == null)
                    {
                        var ti = GUIBTScript.GetTaskImplementation(GUINode._current);
                        if( ti != null )
                            p._enumType = ti.parameterTypes[GUINode._currentParameterIndex];
                    }

                    if (p._enumType != null)
                    {
                        var enumVals = System.Enum.GetNames(p._enumType);
                        string[] fulls = new string[enumVals.Length];
                        int v = 0;
                        for (int i = 0; i < enumVals.Length; i++)
                        {
                            var full = p._enumType.FullName.Replace("+", ".") + "." + enumVals[i];
                            fulls[i] = full;
                            if ( full.EndsWith( value.Trim() ))
                                v = i;

                        }
                        size = style.CalcSize(new GUIContent(fulls[v])); 
                        v = UnityEditor.EditorGUILayout.Popup(v, fulls, style, GUILayout.ExpandWidth(false), GUILayout.Width(size.x));
                        p.value = fulls[v];
                    }
                    else
                    {
                        GUILayout.Label("????", style);
                    }
                }

#endif
            }
            else
            {
                var label = isComment && p.value.Trim() == "" ? "..." : p.value;
                GUILayout.Label(label, style);
            }

        }

        public GUINodeParameter Duplicate()
        {
            var copy = new GUINodeParameter(this._type, this.value);
            return copy;
        }

        void DoOnChange()
        {
            if (OnChange != null)
                OnChange();
        }
    }

}