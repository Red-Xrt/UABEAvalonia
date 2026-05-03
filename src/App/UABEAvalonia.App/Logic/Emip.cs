using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssetsTools.NET;

namespace UABEAvalonia.Logic
{
    public class InstallerPackageFile
    {
        public string magic;
        public bool includesCldb;
        public string modName;
        public string modCreators;
        public string modDescription;
        public ClassDatabaseFile? addedTypes;
        public List<InstallerPackageAssetsDesc> affectedFiles;

        public bool Read(AssetsFileReader reader, bool prefReplacersInMemory = false)
        {
            reader.BigEndian = false;

            magic = reader.ReadStringLength(4);
            if (magic != "EMIP")
                return false;

            includesCldb = reader.ReadByte() != 0;

            modName = reader.ReadCountStringInt16();
            modCreators = reader.ReadCountStringInt16();
            modDescription = reader.ReadCountStringInt16();

            if (includesCldb)
            {
                addedTypes = new ClassDatabaseFile();
                addedTypes.Read(reader);
            }

            int affectedFilesCount = reader.ReadInt32();
            affectedFiles = new List<InstallerPackageAssetsDesc>();
            for (int i = 0; i < affectedFilesCount; i++)
            {
                List<object> replacers = new List<object>();
                InstallerPackageAssetsDesc desc = new InstallerPackageAssetsDesc()
                {
                    isBundle = reader.ReadByte() != 0,
                    path = reader.ReadCountStringInt16()
                };
                int replacerCount = reader.ReadInt32();
                for (int j = 0; j < replacerCount; j++)
                {
                    var wrapper = ParseReplacer(reader, prefReplacersInMemory);
                    replacers.Add(wrapper);
                }
                desc.replacers = replacers;
                affectedFiles.Add(desc);
            }

            return true;
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.BigEndian = false;

            writer.Write(Encoding.ASCII.GetBytes(magic));
            writer.Write(includesCldb);

            writer.WriteCountStringInt16(modName);
            writer.WriteCountStringInt16(modCreators);
            writer.WriteCountStringInt16(modDescription);

            if (includesCldb)
            {
                if (addedTypes == null)
                    writer.Write(0);
                else
                    addedTypes.Write(writer, AssetsTools.NET.ClassFileCompressionType.Lz4);
            }

            writer.Write(affectedFiles.Count);
            for (int i = 0; i < affectedFiles.Count; i++)
            {
                InstallerPackageAssetsDesc desc = affectedFiles[i];
                writer.Write(desc.isBundle);
                writer.WriteCountStringInt16(desc.path);
                
                writer.Write(desc.replacers.Count);
                for (int j = 0; j < desc.replacers.Count; j++)
                {
                    var wrapper = (EmipReplacerWrapper)desc.replacers[j];
                    WriteReplacer(wrapper, writer);
                }
            }
        }

        private static EmipReplacerWrapper ParseReplacer(AssetsFileReader reader, bool prefReplacersInMemory)
        {
            short replacerType = reader.ReadInt16();
            byte fileType = reader.ReadByte();
            if (fileType == 0) //BundleReplacer
            {
                string oldName = reader.ReadCountStringInt16();
                string newName = reader.ReadCountStringInt16();
                bool hasSerializedData = reader.ReadByte() != 0; //guess
                long replacerCount = reader.ReadInt64();
                List<IContentReplacer> replacers = new List<IContentReplacer>();
                for (int i = 0; i < replacerCount; i++)
                {
                    var assetReplacer = ParseReplacer(reader, prefReplacersInMemory);
                    replacers.Add(assetReplacer.Replacer);
                }

                if (replacerType == 4) //BundleReplacerFromAssets
                {
                    //we have to null the assetsfile here and call init later
                    IContentReplacer replacer = new ContentReplacerFromAssets((AssetsFile)null);
                    return new EmipReplacerWrapper { Replacer = replacer };
                }
            }
            else if (fileType == 1) //AssetsReplacer
            {
                byte unknown01 = reader.ReadByte(); //always 1
                int fileId = reader.ReadInt32();
                long pathId = reader.ReadInt64();
                int classId = reader.ReadInt32();
                ushort monoScriptIndex = reader.ReadUInt16();

                List<AssetPPtr> preloadDependencies = new List<AssetPPtr>();
                int preloadDependencyCount = reader.ReadInt32();
                for (int i = 0; i < preloadDependencyCount; i++)
                {
                    int pFileId = reader.ReadInt32();
                    long pPathId = reader.ReadInt64();
                    preloadDependencies.Add(new AssetPPtr(pFileId, pPathId));
                }

                int propertiesHashCount = reader.ReadInt32();
                if (propertiesHashCount == 4)
                {
                    uint pHash0 = reader.ReadUInt32();
                    uint pHash1 = reader.ReadUInt32();
                    uint pHash2 = reader.ReadUInt32();
                    uint pHash3 = reader.ReadUInt32();
                }

                int scriptIdHashCount = reader.ReadInt32();
                if (scriptIdHashCount == 4)
                {
                    uint sHash0 = reader.ReadUInt32();
                    uint sHash1 = reader.ReadUInt32();
                    uint sHash2 = reader.ReadUInt32();
                    uint sHash3 = reader.ReadUInt32();
                }

                int unknown2 = reader.ReadInt32();
                if (unknown2 == 1)
                {
                    int num6 = reader.ReadInt32();
                    long num7 = reader.ReadInt64();
                    int num8 = reader.ReadInt32();
                    int num9 = reader.ReadInt32();
                    long num10 = reader.ReadInt64();
                }
                
                if (replacerType == 0) //AssetsRemover
                {
                    IContentReplacer replacer = new ContentRemover();
                    return new EmipReplacerWrapper { PathId = pathId, ClassId = classId, MonoId = monoScriptIndex, Replacer = replacer };
                }
                else if (replacerType == 1 || replacerType == 2) //AssetsReplacer
                {
                    int bufLength = reader.ReadInt32();

                    IContentReplacer replacer;
                    if (prefReplacersInMemory)
                    {
                        byte[] buf = reader.ReadBytes(bufLength);
                        replacer = new ContentReplacerFromBuffer(buf);
                    }
                    else
                    {
                        replacer = new ContentReplacerFromStream(reader.BaseStream, reader.Position, bufLength, false);
                        reader.Position += bufLength;
                    }
                    
                    return new EmipReplacerWrapper { PathId = pathId, ClassId = classId, MonoId = monoScriptIndex, Replacer = replacer };
                }
            }
            return null;
        }
        
        private static void WriteReplacer(EmipReplacerWrapper wrapper, AssetsFileWriter writer)
        {
            if (wrapper.Replacer is ContentRemover)
            {
                writer.Write((short)0); // replacer type
                writer.Write((byte)1);  // fileType
                writer.Write((byte)1);
                writer.Write((int)0);
                writer.Write(wrapper.PathId);
                writer.Write(wrapper.ClassId);
                writer.Write(wrapper.MonoId);
                
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Write((int)0);
            }
            else if (wrapper.Replacer is ContentReplacerFromBuffer bufRep)
            {
                writer.Write((short)2); // replacer type
                writer.Write((byte)1);  // fileType
                writer.Write((byte)1);
                writer.Write((int)0);
                writer.Write(wrapper.PathId);
                writer.Write(wrapper.ClassId);
                writer.Write(wrapper.MonoId);
                
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Write((int)0);
                
                long size = bufRep.GetSize();
                writer.Write((int)size);
                bufRep.Write(writer, false);
            }
            else if (wrapper.Replacer is ContentReplacerFromStream strRep)
            {
                writer.Write((short)2); // replacer type
                writer.Write((byte)1);  // fileType
                writer.Write((byte)1);
                writer.Write((int)0);
                writer.Write(wrapper.PathId);
                writer.Write(wrapper.ClassId);
                writer.Write(wrapper.MonoId);
                
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Write((int)0);
                
                long size = strRep.GetSize();
                writer.Write((int)size);
                strRep.Write(writer, false);
            }
        }
    }

    public class InstallerPackageAssetsDesc
    {
        public bool isBundle;
        public string path;
        public List<object> replacers;
    }
}
