using System;
using System.Collections.Generic;
using AssetsTools.NET.Extra;

namespace UABEAvalonia.Services
{
    public class WorkspaceManager : IWorkspaceManager
    {
        public AssetWorkspace Workspace { get; private set; }

        public WorkspaceManager()
        {
        }

        public void SetWorkspace(AssetWorkspace newWorkspace)
        {
            Workspace = newWorkspace;
        }

        public void ApplyChanges()
        {
            if (Workspace != null)
            {
                // Logic applying changes or clearing the state...
            }
        }

        public void ClearWorkspace()
        {
            if (Workspace != null)
            {
                // Clear state
            }
            Workspace = null;
        }
    }
}
