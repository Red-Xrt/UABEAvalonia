using System;
using System.Collections.Generic;
using AssetsTools.NET;
using UABEAvalonia.Models;
using UABEAvalonia.Models;
using UABEAvalonia.Models.Workspace;
using UABEAvalonia.Services;
using UABEAvalonia.Services;

namespace UABEAvalonia.Services
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
