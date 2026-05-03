using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace UABEAvalonia
{
    public class BundleWorkspace
    {
        public BundleFileInstance? BundleInst { get; set; }
        public AssetsManager am { get; }
        
        public ObservableCollection<BundleWorkspaceItem> Files { get; }
        public Dictionary<string, BundleWorkspaceItem> FileLookup { get; }
        public HashSet<string> RemovedFiles { get; }

        public BundleWorkspace()
        {
            BundleInst = null;
            am = new AssetsManager();

            Files = new ObservableCollection<BundleWorkspaceItem>();
            FileLookup = new Dictionary<string, BundleWorkspaceItem>();
            RemovedFiles = new HashSet<string>();
        }

        public void Reset(BundleFileInstance? bundleInst)
        {
            BundleInst = bundleInst;

            Files.Clear();
            FileLookup.Clear();
            RemovedFiles.Clear();

            if (bundleInst != null)
                PopulateFilesList();
        }

        private void PopulateFilesList()
        {
            var dirInfs = BundleInst.file.BlockAndDirInfo.DirectoryInfos;
            foreach (var dirInf in dirInfs)
            {
                string name = dirInf.Name;
                long startAddress = dirInf.Offset;
                long length = dirInf.DecompressedSize;
                SegmentStream stream = new SegmentStream(BundleInst.file.DataReader.BaseStream, startAddress, length);
                BundleWorkspaceItem wsItem = new BundleWorkspaceItem(name, name, false, (dirInf.Flags & 0x04) != 0, false, stream);
                Files.Add(wsItem);
                FileLookup[name] = wsItem;
            }
        }

        public void AddOrReplaceFile(Stream stream, string name, bool isSerialized, string? prevName = null)
        {
            if (prevName == null)
                prevName = name;

            if (FileLookup.ContainsKey(prevName))
            {
                BundleWorkspaceItem wsItem;

                int fileListIndex = Files.IndexOf(FileLookup[prevName]);
                if (fileListIndex != -1)
                {
                    wsItem = new BundleWorkspaceItem(name, Files[fileListIndex].OriginalName, false, isSerialized, true, stream);
                    Files[fileListIndex] = wsItem;
                }
                else
                {
                    wsItem = new BundleWorkspaceItem(name, prevName, false, isSerialized, true, stream);
                }

                if (FileLookup[prevName].IsNew)
                {
                    FileLookup[prevName].Stream.Close();
                }

                FileLookup.Remove(prevName);
                FileLookup[name] = wsItem;
            }
            else
            {
                BundleWorkspaceItem wsItem = new BundleWorkspaceItem(name, name, true, isSerialized, true, stream);

                Files.Add(wsItem);
                FileLookup[name] = wsItem;
            }
        }

        public void RenameFile(string origName, string newName)
        {
            if (FileLookup.ContainsKey(origName))
            {
                BundleWorkspaceItem item = FileLookup[origName];
                item.Name = newName;
                FileLookup.Remove(origName);
                FileLookup[newName] = item;
            }
        }

        public void ApplyChanges()
        {
            if (BundleInst == null) return;

            var dirInfosList = BundleInst.file.BlockAndDirInfo.DirectoryInfos.ToList();
            
            // Mark removed
            foreach (string name in RemovedFiles)
            {
                var dir = dirInfosList.FirstOrDefault(d => d.Name == name);
                if (dir != null)
                {
                    dir.Replacer = new ContentRemover();
                }
            }

            // Handle modified/renamed/new
            foreach (BundleWorkspaceItem item in FileLookup.Values)
            {
                if (item.IsRemoved) continue;

                if (item.IsNew)
                {
                    var newDir = new AssetBundleDirectoryInfo
                    {
                        Name = item.Name,
                        Offset = 0,
                        DecompressedSize = item.Stream.Length,
                        Flags = item.IsSerialized ? 4u : 0u,
                        Replacer = new ContentReplacerFromStream(item.Stream, 0, -1)
                    };
                    dirInfosList.Add(newDir);
                }
                else
                {
                    var targetDir = dirInfosList.FirstOrDefault(d => d.Name == item.OriginalName);
                    if (targetDir != null)
                    {
                        targetDir.Name = item.Name; // Apply rename if any
                        if (item.IsModified)
                        {
                            targetDir.Replacer = new ContentReplacerFromStream(item.Stream, 0, -1);
                        }
                    }
                }
            }
            
            BundleInst.file.BlockAndDirInfo.DirectoryInfos = dirInfosList;
        }
    }
    
    public class BundleWorkspaceItem
    {
        public string Name { get; set; }
        public string OriginalName { get; }
        public bool IsNew { get; }
        public bool IsSerialized { get; }
        public bool IsRemoved { get; set; }
        public bool IsModified { get; }
        public Stream Stream { get; }

        private static SolidColorBrush AssetsBrushDark = new SolidColorBrush(Avalonia.Media.Color.Parse("#b17fd7"));
        private static SolidColorBrush AssetsBrushLight = new SolidColorBrush(Avalonia.Media.Color.Parse("#642c8f"));
        private static SolidColorBrush RessBrushDark = new SolidColorBrush(Avalonia.Media.Color.Parse("#569cd6"));
        private static SolidColorBrush RessBrushLight = new SolidColorBrush(Avalonia.Media.Color.Parse("#0000ff"));
        private static SolidColorBrush ResourceBrushDark = new SolidColorBrush(Avalonia.Media.Color.Parse("#ffd700"));
        private static SolidColorBrush ResourceBrushLight = new SolidColorBrush(Avalonia.Media.Color.Parse("#9e7e00"));
        private static SolidColorBrush EtcBrushDark = new SolidColorBrush(Avalonia.Media.Color.Parse("#ee00ee"));
        private static SolidColorBrush EtcBrushLight = new SolidColorBrush(Avalonia.Media.Color.Parse("#ee00ee"));
        
        private static SolidColorBrush AssetsBrush => ThemeHandler.UseDarkTheme ? AssetsBrushDark : AssetsBrushLight;
        private static SolidColorBrush RessBrush => ThemeHandler.UseDarkTheme ? RessBrushDark : RessBrushLight;
        private static SolidColorBrush ResourceBrush => ThemeHandler.UseDarkTheme ? ResourceBrushDark : ResourceBrushLight;
        private static SolidColorBrush EtcBrush => ThemeHandler.UseDarkTheme ? EtcBrushDark : EtcBrushLight;

        public IBrush Color
        {
            get
            {
                if (IsSerialized)
                    return AssetsBrush;
                else if (Name.EndsWith(".resS"))
                    return RessBrush;
                else if (Name.EndsWith(".resource"))
                    return ResourceBrush;
                else
                    return EtcBrush;
            }
        }

        public BundleWorkspaceItem(
            string name, string originalName, bool isNew,
            bool isSerialized, bool isModified, Stream stream
        )
        {
            Name = name;
            OriginalName = originalName;
            IsNew = isNew;
            IsSerialized = isSerialized;
            IsModified = isModified;
            Stream = stream;

            IsRemoved = false;
        }

        public override string ToString()
        {
            return Name + (IsModified ? "*" : "");
        }
    }
}
