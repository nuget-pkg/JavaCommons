#pragma warning disable CS0618
using Antlr4.Runtime;
using System;
using System.Globalization;
using System.Reflection;
using JavaCommons.Parser.Json5;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace JavaCommons
{
    public partial class Util
    {
        static Util()
        {
        }

        public static string AssemblyName(Assembly assembly)
        {
            return System.Reflection.AssemblyName.GetAssemblyName(assembly.Location).Name!;
        }

        public static int FreeTcpPort()
        {
            // https://stackoverflow.com/questions/138043/find-the-next-tcp-port-in-net
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static DateTime? AsDateTime(dynamic x)
        {
            if (x == null) return null;
            string fullName = Util.FullName(x);
            if (fullName == "Newtonsoft.Json.Linq.JValue")
            {
                return ((DateTime)x);
            }
            else if (fullName == "System.DateTime")
            {
                return (System.DateTime)x;
            }
            else if (fullName == "System.String")
            {
                if (((string)x) == "") return null;
                return DateTime.Parse((string)x);
            }
            else
            {
                throw new ArgumentException("x");
            }
        }

        /*
        public static string ShortenString(string s, int limit, string ellipsis = "...")
        {
            var enc = new UTF32Encoding();
            byte[] byteUtf32 = enc.GetBytes(s);
            if (byteUtf32.Length <= limit * 4) return s;
            ArraySegment<byte> segment = new ArraySegment<byte>(byteUtf32, 0, limit * 4);
            byteUtf32 = segment.ToArray();
            string decodedString = enc.GetString(byteUtf32);
            return decodedString + ellipsis;
        }
        public static string GetFileNameFromUrl(string url)
        {
            var list = url.Split('/');
            var fileName = list[list.Length - 1];
            return fileName;
        }
        public static string GetFileBaseNameFromUrl(string url)
        {
            var fileName = GetFileNameFromUrl(url);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            return baseName;
        }
        public static string GetStringFromUrl(string url)
        {
            HttpWebRequest? request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = (HttpWebResponse)request!.GetResponse();
            WebHeaderCollection header = response.Headers;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        public static void DownloadBinaryFromUrl(string url, string destinationPath)
        {
            string parent = Util.ParentDirectoryPath(destinationPath);
            Directory.CreateDirectory(parent);
            WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
            var objResponse = objRequest.GetResponse();
            byte[] buffer = new byte[32768];
            using (Stream input = objResponse.GetResponseStream())
            {
                using (FileStream output = new FileStream(destinationPath, FileMode.CreateNew))
                {
                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
        public static string ParentDirectoryPath(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.Parent.FullName;
        }
        */
        public static string FullName(dynamic x)
        {
            if (x == null) return "null";
            string fullName = ((object)x).GetType().FullName!;
            return fullName;
        }

        public static string ToJson(dynamic x, bool indent = false)
        {
            return JsonConvert.SerializeObject(x, indent ? Formatting.Indented : Formatting.None);
        }

        public static dynamic? FromJson(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });
        }

        public static T? FromJson<T>(string json, T? fallback = default(T))
        {
            //if (String.IsNullOrEmpty(json)) return default(T);
            if (String.IsNullOrEmpty(json)) return fallback;
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static byte[] ToBson(dynamic x)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, x);
            }

            return ms.ToArray();
        }

        public static dynamic? FromBson(byte[] bson)
        {
            if (bson == null) return null;
            MemoryStream ms = new MemoryStream(bson);
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }

        public static T? FromBson<T>(byte[] bson)
        {
            if (bson == null) return default(T);
            MemoryStream ms = new MemoryStream(bson);
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }

        public static dynamic? FromObject(dynamic x)
        {
            if (x == null) return null;
            var o = (dynamic)JObject.FromObject(new { x = x },
                new JsonSerializer
                {
                    DateParseHandling = DateParseHandling.None
                });
            return o.x;
        }

        public static T? FromObject<T>(dynamic x)
        {
#if false
            string json = ToJson(x);
            return FromJson<T>(json);
#else
            dynamic? o = FromObject(x);
            if (o == null) return default(T);
            return (T)(o.ToObject<T>());
#endif
        }

        public static string? ToXml(dynamic x)
        {
            if (x == null) return null;
            if (FullName(x) == "System.Xml.Linq.XElement")
            {
                return ((XElement)x).ToString();
            }

            XDocument? doc;
            if (FullName(x) == "System.Xml.Linq.XDocument")
            {
                doc = (XDocument)x;
            }
            else
            {
                string json = ToJson(x);
                doc = JsonConvert.DeserializeXmlNode(json)?.ToXDocument();
                //return "<?>";
            }

            return doc == null ? "null" : doc.ToStringWithDeclaration();
        }

        public static XDocument? FromXml(string xml)
        {
            if (xml == null) return null;
            XDocument doc = XDocument.Parse(xml);
            return doc;
        }

        public static string ToString(dynamic x)
        {
            if ((x as string) != null)
            {
                var s = (string)x;
                return s;
            }

            if (FullName(x) == "Newtonsoft.Json.Linq.JValue")
            {
                var value = (Newtonsoft.Json.Linq.JValue)x;
                try
                {
                    x = (DateTime)value;
                }
                catch (Exception)
                {
                }
            }

            if (FullName(x) == "System.Xml.Linq.XDocument" || FullName(x) == "System.Xml.Linq.XElement")
            {
                string xml = ToXml(x);
                return xml;
            }
            else if (FullName(x) == "System.DateTime")
            {
                return x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
            }
            else
            {
                try
                {
                    string json = ToJson(x, true);
                    return json;
                }
                catch (Exception)
                {
                    return x.ToString();
                }
            }
        }

        public static void Print(dynamic x, string? title = null)
        {
            if (title != null) Console.Write(title + ": ");
            Console.WriteLine(Util.ToString(x));
        }

        public static void Log(dynamic x, string? title = null)
        {
            if (title != null) Console.Error.Write(title + ": ");
            Console.Error.WriteLine(Util.ToString(x));
        }

        public static XDocument ParseXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            return doc;
        }

        public static string[] ResourceNames(Assembly assembly)
        {
            return assembly.GetManifestResourceNames();
        }

        public static Stream? ResourceAsStream(Assembly assembly, string name)
        {
            Stream? stream = assembly.GetManifestResourceStream($"{AssemblyName(assembly)}.{name}");
            return stream;
        }

        public static string StreamAsText(Stream stream)
        {
            if (stream == null) return "";
            var streamReader = new StreamReader(stream);
            var text = streamReader.ReadToEnd();
            return text;
        }

        public static string? ResourceAsText(Assembly assembly, string name)
        {
            Stream? stream = assembly.GetManifestResourceStream($"{AssemblyName(assembly)}.{name}");
            if (stream == null) return null;
            return StreamAsText(stream);
        }

        public static byte[] StreamAsBytes(Stream stream)
        {
            if (stream == null) return new byte[] { };
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return bytes;
        }

        public static byte[]? ResourceAsBytes(Assembly assembly, string name)
        {
            Stream? stream = assembly.GetManifestResourceStream($"{AssemblyName(assembly)}.{name}");
            if (stream == null) return null;
            return StreamAsBytes(stream);
        }

        public static dynamic? StreamAsJson(Stream stream)
        {
            string json = StreamAsText(stream);
            return FromJson(json);
        }

        public static dynamic? ResourceAsJson(Assembly assembly, string name)
        {
            string? json = ResourceAsText(assembly, name);
            if (json == null) return null;
            return FromJson(json);
        }

        public static string FirstPart(string s, params char[] separator)
        {
            string[] split = s.Split(separator);
            if (split.Length == 0) return "";
            return split[0];
        }

        public static string LastPart(string s, params char[] separator)
        {
            string[] split = s.Split(separator);
            if (split.Length == 0) return "";
            return split[split.Length - 1];
        }

        public static string GetRidirectUrl(string url)
        {
            Task<string> task = GetRidirectUrlTask(url);
            task.Wait();
            return task.Result;
        }

        private static async Task<string> GetRidirectUrlTask(string url)
        {
            HttpClient client;
            HttpResponseMessage response;
            try
            {
                client = new HttpClient();
                response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return url;
            }

            string result = response.RequestMessage!.RequestUri!.ToString();
            response.Dispose();
            return result;
        }

        public static byte[]? ToUtf8Bytes(string? s)
        {
            if (s == null) return null;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
            return bytes;
        }
    private static dynamic? JSON5ToObject(ParserRuleContext x)
    {
        //Log(FullName(x), "FullName(x)");
        string fullName = FullName(x);
        switch (fullName)
        {
            case "JavaCommons.Parser.Json5.JSON5Parser+Json5Context":
            {
                for (int i = 0; i < x.children.Count; i++)
                {
                    //Print("  " + FullName(x.children[i]));
                    //Print("    " + JSON5Terminal((x.children[i])));
                    if (x.children[i] is Antlr4.Runtime.Tree.ErrorNodeImpl)
                    {
                        return null;
                    }
                }
                return JSON5ToObject((ParserRuleContext)x.children[0]);
            }
            case "JavaCommons.Parser.Json5.JSON5Parser+ValueContext":
            {
#if false
                for (int i = 0; i < x.children.Count; i++)
                {
                    Print("  " + FullName(x.children[i]));
                    Print("    " + JSON5Terminal((x.children[i])));
                }
#endif
                if (x.children[0] is Antlr4.Runtime.Tree.TerminalNodeImpl)
                {
                    string t = JSON5Terminal(x.children[0])!;
                    if (t.StartsWith("\""))
                    {
                        return (string)Newtonsoft.Json.JsonConvert.DeserializeObject(t, new JsonSerializerSettings
                        {
                            DateParseHandling = DateParseHandling.None
                        })!;
                    }

                    switch (t)
                    {
                        case "true":
                            return true;
                        case "false":
                            return false;
                        case "null":
                            return null;
                    }

                    //throw new Exception($"Unexpected JSON5Parser+ValueContext: {t}");
                    return t;
                }

                return JSON5ToObject((ParserRuleContext)x.children[0]);
            }
            case "JavaCommons.Parser.Json5.JSON5Parser+ArrContext":
            {
                var result = new Newtonsoft.Json.Linq.JArray();
                for (int i = 0; i < x.children.Count; i++)
                {
                    //Print("  " + FullName(x.children[i]));
                    if (x.children[i] is JSON5Parser.ValueContext value)
                    {
                        result.Add(JSON5ToObject(value));
                    }
                }

                return result;
            }
            case "JavaCommons.Parser.Json5.JSON5Parser+ObjContext":
            {
                var result = new Newtonsoft.Json.Linq.JObject();
                for (int i = 0; i < x.children.Count; i++)
                {
                    //Print("  " + FullName(x.children[i]));
                    if (x.children[i] is JSON5Parser.PairContext pair)
                    {
                        var pairObj = JSON5ToObject(pair)!;
                        result[(string)pairObj["key"]] = pairObj["value"];
                    }
                }

                return result;
            }
            case "JavaCommons.Parser.Json5.JSON5Parser+PairContext":
            {
                var result = new Newtonsoft.Json.Linq.JObject();
                for (int i = 0; i < x.children.Count; i++)
                {
                    //Print("  " + FullName(x.children[i]));
                    if (x.children[i] is JSON5Parser.KeyContext key)
                    {
                        result["key"] = JSON5ToObject(key);
                    }

                    if (x.children[i] is JSON5Parser.ValueContext value)
                    {
                        result["value"] = JSON5ToObject(value);
                    }
                }

                return result;
            }
            case "JavaCommons.Parser.Json5.JSON5Parser+KeyContext":
            {
#if false
                for (int i = 0; i < x.children.Count; i++)
                {
                    Print("  " + FullName(x.children[i]), "AAA");
                    Print("    " + JSON5Terminal((x.children[i])), "BBB");
                }
#endif
                string t = JSON5Terminal(x.children[0])!;
                if (t.StartsWith("\""))
                {
                    return (string)Newtonsoft.Json.JsonConvert.DeserializeObject(t, new JsonSerializerSettings
                    {
                        DateParseHandling = DateParseHandling.None
                    })!;
                }

                return t;
            }
            case "JavaCommons.Parser.Json5.JSON5Parser+NumberContext":
            {
#if false
                for (int i = 0; i < x.children.Count; i++)
                {
                    Print("  " + FullName(x.children[i]));
                    Print("    " + JSON5Terminal((x.children[i])));
                }
#endif
                return decimal.Parse(JSON5Terminal(x.children[0])!, CultureInfo.InvariantCulture);
            }
            default:
                throw new Exception($"Unexpected: {fullName}");
        }

        return null;
    }

    private static string? JSON5Terminal(Antlr4.Runtime.Tree.IParseTree x)
    {
        if (x is Antlr4.Runtime.Tree.TerminalNodeImpl t)
        {
            return t.ToString();
        }

        return null;
    }
    }
}