using System.Collections.Generic;
using System.Threading.Tasks;

namespace UABEAvalonia.Services
{
    public interface IDialogService
    {
        Task<string[]> OpenFileDialog(string title, bool allowMultiple, List<string> patterns);
        Task<string?> SaveFileDialog(string title, string suggestedFileName, List<string> patterns = null);
        Task<string[]> OpenFolderDialog(string title);
        Task<MessageBoxResult> ShowMessageBox(string title, string message, MessageBoxType type = MessageBoxType.OK);
        Task<string> ShowCustomMessageBox(string title, string message, params string[] options);
        Task<string?> AskLoadSplitFile(string fileToSplit);
        Task<string> AskForVersion(string currentVersion);
        Task<bool> AskForImportSerialized();
        Task<string> AskForRename(string currentName);
    }
}
