using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Compiler.Debugger
{
    public class DebugMapEntry
    {
        public string url;
        public int line;

        public int startOfs;
        public int endOfs;

        public override string ToString()
        {
            return "Line "+this.line+" at "+url;
        }
    }

    public class NeoMapFile
    {
        private List<DebugMapEntry> _entries = new List<DebugMapEntry>();
        public IEnumerable<DebugMapEntry> Entries { get { return _entries; } }

        public string contractName { get; private set; }

        public IEnumerable<string> FileNames => _fileNames;
        private HashSet<string> _fileNames = new HashSet<string>();

        /// <summary>
        /// Calculates the source code line that maps to the specificed script offset.
        /// </summary>
        public int ResolveLine(int ofs, out string filePath)
        {
            foreach (var entry in this.Entries)
            {
                if (ofs >= entry.startOfs && ofs <= entry.endOfs)
                {
                    filePath = entry.url;
                    return entry.line;
                }
            }

            throw new Exception("Offset cannot be mapped");
        }

        /// <summary>
        /// Calculates the script offset that maps to the specificed source code line 
        /// </summary>
        public int ResolveStartOffset(int line, string filePath)
        {
            foreach (var entry in this.Entries)
            {
                if (entry.line == line && entry.url == filePath)
                {
                    return entry.startOfs;
                }
            }

            throw new Exception("Line cannot be mapped");
        }

        public int ResolveEndOffset(int line, string filePath)
        {
            foreach (var entry in this.Entries)
            {
                if (entry.line == line && entry.url == filePath)
                {
                    return entry.endOfs;
                }
            }

            throw new Exception("Line cannot be mapped");
        }

    }

}
