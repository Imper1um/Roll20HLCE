using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output
{
    public partial class Program
    {
        public static void Main(params string[] parameters)
        {
#if DEBUG
            var xml = "";
            using (var fileStream = File.OpenRead(@"C:\PathfinderExporter\Import.xml"))
                using (var streamReader = new StreamReader(fileStream))
            {
                xml = streamReader.ReadToEnd();
            }
#else
            if (parameters == null || parameters.Length == 0) return;
            var xml = parameters[0];
#endif

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                streamWriter.Write(xml);
                streamWriter.Flush();
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                using (var xmlReader = XmlReader.Create(streamReader))
                {
                    var xmlSerializer = new XmlSerializer(typeof(HeroLabDocument));
                    var document = xmlSerializer.Deserialize(xmlReader) as HeroLabDocument;
                    if (document == null) return;
                    if (!Directory.Exists("C:\\PathfinderExporter"))
                    {
                        Directory.CreateDirectory("C:\\PathfinderExporter");
                    }
                    foreach (var character in document.Public.Characters) ProcessNpcCharacter(character);
                }
            }
        }

        private static string OneLineString(string description)
        {
            if (description == null) return "";
            return description.Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        private static string UnwindException(Exception ex)
        {
            var exceptionInfo = new StringBuilder();
            exceptionInfo.AppendLine($"{ex.GetType().FullName}: {ex.Message}--");
            exceptionInfo.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                exceptionInfo.AppendLine($"-----------------------------");
                exceptionInfo.Append($"Inner Exception: {UnwindException(ex.InnerException)}");
            }
            return exceptionInfo.ToString();
        }

        private static string CommaDelimitedList(IEnumerable<string> items, string separator = ", ")
        {
            if (items == null) return "";
            var l = "";
            foreach (var i in items)
            {
                if (!string.IsNullOrEmpty(l)) l += separator;
                l += i;
            }
            return l;
        }

        private static int GetValue(string val)
        {
            if (string.IsNullOrEmpty(val)) return 0;
            if (val.StartsWith("+"))
            {
                return int.Parse(val.Substring(1));
            }
            return int.Parse(val);
        }

        private static string PreventRollingString(string unformatted)
        {
            //return unformatted;
            return unformatted.Replace("{", "&#123;").Replace("}", "&#125;").Replace("[", "&#91;").Replace("]", "&#93;");
        }

        private static string GetAlignment(string alignmentName)
        {
            switch (alignmentName)
            {
                case "Chaotic Neutral": return "CN";
                case "Lawful Neutral": return "LN";
                case "Chaotic Good": return "CG";
                case "Neutral Good": return "NG";
                case "Lawful Good": return "LG";
                case "Lawful Evil": return "LE";
                case "Neutral Evil": return "NE";
                case "Chaotic Evil": return "CE";
                default: return "N";
            }
        }
    }
}
