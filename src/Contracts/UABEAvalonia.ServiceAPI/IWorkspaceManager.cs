using System;
using System.Collections.Generic;
using AssetsTools.NET.Extra;

namespace UABEAvalonia.Services
{
    public interface IWorkspaceManager
    {
        AssetWorkspace Workspace { get; }

        // Cần di chuyển các hàm liên quan đến mở/đóng/thay đổi trạng thái ra đây
        void SetWorkspace(AssetWorkspace newWorkspace);
        void ApplyChanges();
        void ClearWorkspace();
    }
}
