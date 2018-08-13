using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Neo.Compiler.Debugger
{
    public static class DebugInfo
    {
        public static MyJson.JsonNode_Object ExportDebugInfo(string avmName, NeoModule module)
        {
            var outjson = new MyJson.JsonNode_Object();

            var debugMap = new List<DebugMapEntry>();
            DebugMapEntry currentDebugEntry = null;

            var fileMap = new Dictionary<string, int>();

            List<byte> bytes = new List<byte>();

            foreach (var c in module.total_Codes.Values)
            {
                if (c.debugcode != null && c.debugline > 0 && c.debugline < 2000)
                {
                    var previousDebugEntry = currentDebugEntry;

                    currentDebugEntry = new DebugMapEntry();
                    if (previousDebugEntry != null)
                    {
                        currentDebugEntry.startOfs = previousDebugEntry.endOfs + 1;
                        currentDebugEntry.endOfs = bytes.Count;
                    }
                    else
                    {
                        currentDebugEntry.startOfs = 0;
                        currentDebugEntry.endOfs = bytes.Count;
                    }
                    currentDebugEntry.url = c.debugcode;
                    currentDebugEntry.line = c.debugline;

                    if (!fileMap.ContainsKey(c.debugcode))
                    {
                        fileMap[c.debugcode] = fileMap.Count + 1;
                    }

                    debugMap.Add(currentDebugEntry);
                }
                else
                if (currentDebugEntry != null)
                {
                    currentDebugEntry.endOfs = bytes.Count;
                }
                bytes.Add((byte)c.code);
                if (c.bytes != null)
                    for (var i = 0; i < c.bytes.Length; i++)
                    {
                        bytes.Add(c.bytes[i]);
                    }

            }

            string compilerName = System.AppDomain.CurrentDomain.FriendlyName.ToLowerInvariant();
            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            var avmInfo = new MyJson.JsonNode_Object();
            avmInfo.Add("name", new MyJson.JsonNode_ValueString(avmName));
            //avmInfo.Add("hash", new MyJson.JsonNode_ValueString(hash));

            var compilerInfo = new MyJson.JsonNode_Object();
            compilerInfo.Add("name", new MyJson.JsonNode_ValueString(compilerName));
            compilerInfo.Add("version", new MyJson.JsonNode_ValueString(version));

            var fileInfo = new MyJson.JsonNode_Array();
            foreach (var entry in fileMap)
            {
                var fileEntry = new MyJson.JsonNode_Object();
                fileEntry.Add("id", new MyJson.JsonNode_ValueNumber(entry.Value));
                fileEntry.Add("url", new MyJson.JsonNode_ValueString(entry.Key));
                fileInfo.AddArrayValue(fileEntry);
            }

            var mapInfo = new MyJson.JsonNode_Array();
            foreach (var entry in debugMap)
            {
                if (!fileMap.ContainsKey(entry.url))
                {
                    continue;
                }

                var fileID = fileMap[entry.url];

                var mapEntry = new MyJson.JsonNode_Object();
                mapEntry.Add("start", new MyJson.JsonNode_ValueNumber(entry.startOfs));
                mapEntry.Add("end", new MyJson.JsonNode_ValueNumber(entry.endOfs));
                mapEntry.Add("file", new MyJson.JsonNode_ValueNumber(fileID));
                mapEntry.Add("line", new MyJson.JsonNode_ValueNumber(entry.line));
                mapInfo.AddArrayValue(mapEntry);
            }

            outjson["avm"] = avmInfo;
            outjson["compiler"] = compilerInfo;
            outjson["files"] = fileInfo;
            outjson["map"] = mapInfo;

            return outjson;
        }

    }
}
