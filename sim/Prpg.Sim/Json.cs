using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Prpg.Sim
{
    /// <summary>
    /// A small JSON reader with no dependencies, so the simulation needs nothing from Unity or NuGet.
    /// Objects keep their keys in file order. Numbers are doubles parsed with the invariant culture,
    /// which rounds the same way JavaScript does.
    /// </summary>
    public sealed class JsonObject
    {
        public readonly List<string> Keys = new List<string>();
        readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public void Set(string key, object value)
        {
            if (!values.ContainsKey(key)) Keys.Add(key);
            values[key] = value;
        }
        public bool Has(string key) => values.ContainsKey(key);
        public object Get(string key) => values.TryGetValue(key, out var v) ? v : null;
        public double Number(string key)
        {
            var v = Get(key);
            if (v is double d) return d;
            if (v is bool b) return b ? 1 : 0;
            throw new FormatException("Expected a number for \"" + key + "\"");
        }
        public string String(string key) => Get(key) as string;
        public JsonObject Object(string key) => Get(key) as JsonObject;
        public List<object> Array(string key) => Get(key) as List<object>;
    }

    public static class Json
    {
        public static object Parse(string text)
        {
            int i = 0;
            var v = Value(text, ref i);
            Skip(text, ref i);
            if (i != text.Length) throw Error(text, i, "unexpected text after the value");
            return v;
        }

        static FormatException Error(string s, int i, string what) => new FormatException("JSON: " + what + " at character " + i);

        static void Skip(string s, ref int i)
        {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
        }

        static object Value(string s, ref int i)
        {
            Skip(s, ref i);
            if (i >= s.Length) throw Error(s, i, "unexpected end");
            char c = s[i];
            if (c == '{') return Obj(s, ref i);
            if (c == '[') return Arr(s, ref i);
            if (c == '"') return Str(s, ref i);
            if (Word(s, ref i, "true")) return true;
            if (Word(s, ref i, "false")) return false;
            if (Word(s, ref i, "null")) return null;
            return Num(s, ref i);
        }

        static bool Word(string s, ref int i, string w)
        {
            if (string.CompareOrdinal(s, i, w, 0, w.Length) != 0) return false;
            i += w.Length;
            return true;
        }

        static JsonObject Obj(string s, ref int i)
        {
            var o = new JsonObject();
            i++;
            Skip(s, ref i);
            if (i < s.Length && s[i] == '}') { i++; return o; }
            while (true)
            {
                Skip(s, ref i);
                if (i >= s.Length || s[i] != '"') throw Error(s, i, "expected a key");
                string k = Str(s, ref i);
                Skip(s, ref i);
                if (i >= s.Length || s[i] != ':') throw Error(s, i, "expected ':'");
                i++;
                o.Set(k, Value(s, ref i));
                Skip(s, ref i);
                if (i < s.Length && s[i] == ',') { i++; continue; }
                if (i < s.Length && s[i] == '}') { i++; return o; }
                throw Error(s, i, "expected ',' or '}'");
            }
        }

        static List<object> Arr(string s, ref int i)
        {
            var a = new List<object>();
            i++;
            Skip(s, ref i);
            if (i < s.Length && s[i] == ']') { i++; return a; }
            while (true)
            {
                a.Add(Value(s, ref i));
                Skip(s, ref i);
                if (i < s.Length && s[i] == ',') { i++; continue; }
                if (i < s.Length && s[i] == ']') { i++; return a; }
                throw Error(s, i, "expected ',' or ']'");
            }
        }

        static string Str(string s, ref int i)
        {
            var sb = new StringBuilder();
            i++;
            while (i < s.Length)
            {
                char c = s[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (i >= s.Length) break;
                char e = s[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        i += 4;
                        break;
                    default: throw Error(s, i, "bad escape");
                }
            }
            throw Error(s, i, "unterminated string");
        }

        static double Num(string s, ref int i)
        {
            int start = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            if (i == start) throw Error(s, i, "unexpected character '" + s[i] + "'");
            return double.Parse(s.Substring(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        // ---- Writing, for the tuning panel to save numbers back. ----

        public static string Write(object v, int indent = 0)
        {
            var sb = new StringBuilder();
            WriteValue(sb, v, indent, 0);
            return sb.ToString();
        }

        static void WriteValue(StringBuilder sb, object v, int indent, int depth)
        {
            switch (v)
            {
                case null: sb.Append("null"); break;
                case bool b: sb.Append(b ? "true" : "false"); break;
                case double d: sb.Append(d.ToString("R", CultureInfo.InvariantCulture)); break;
                case int n: sb.Append(n.ToString(CultureInfo.InvariantCulture)); break;
                case string str: WriteString(sb, str); break;
                case JsonObject o:
                    sb.Append('{');
                    for (int k = 0; k < o.Keys.Count; k++)
                    {
                        if (k > 0) sb.Append(',');
                        NewLine(sb, indent, depth + 1);
                        WriteString(sb, o.Keys[k]);
                        sb.Append(indent > 0 ? ": " : ":");
                        WriteValue(sb, o.Get(o.Keys[k]), indent, depth + 1);
                    }
                    if (o.Keys.Count > 0) NewLine(sb, indent, depth);
                    sb.Append('}');
                    break;
                case List<object> a:
                    sb.Append('[');
                    for (int k = 0; k < a.Count; k++)
                    {
                        if (k > 0) sb.Append(indent > 0 ? ", " : ",");
                        WriteValue(sb, a[k], indent, depth + 1);
                    }
                    sb.Append(']');
                    break;
                default: throw new ArgumentException("Cannot write " + v.GetType().Name + " as JSON");
            }
        }

        static void NewLine(StringBuilder sb, int indent, int depth)
        {
            if (indent <= 0) return;
            sb.Append('\n');
            sb.Append(' ', indent * depth);
        }

        static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"') sb.Append("\\\"");
                else if (c == '\\') sb.Append("\\\\");
                else if (c == '\n') sb.Append("\\n");
                else if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4"));
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }
}
