using System;
using System.Collections.Generic;
using AssetsTools.NET;

namespace UABEAvalonia.Logic
{
    public class EmipReplacerWrapper
    {
        // For Assets
        public long PathId { get; set; }
        public int ClassId { get; set; }
        public ushort MonoId { get; set; }
        
        // For Bundles
        public string OriginalName { get; set; }
        public string NewName { get; set; }
        public bool HasSerializedData { get; set; }

        public IContentReplacer Replacer { get; set; }
    }
}
