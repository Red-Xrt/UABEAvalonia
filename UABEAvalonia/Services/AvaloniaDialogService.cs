using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using System.Linq;

namespace UABEAvalonia.Services
{
    public class AvaloniaDialogService : IDialogService
    {
        private Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }

        public async Task<string[]> OpenFileDialog(string title, bool allowMultiple, List<string> patterns)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return new string[0];

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Files") { Patterns = patterns }
                }
            };

            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(options);
            return result.Select(x => x.Path.LocalPath).ToArray();
        }

        public async Task<string?> SaveFileDialog(string title, string suggestedFileName, List<string> patterns = null)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;

            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedFileName
            };

            if (patterns != null && patterns.Count > 0)
            {
                options.FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Files") { Patterns = patterns }
                };
            }

            var result = await mainWindow.StorageProvider.SaveFilePickerAsync(options);
            return result?.Path.LocalPath;
        }

        public async Task<string[]> OpenFolderDialog(string title)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return new string[0];

            var options = new FolderPickerOpenOptions
            {
                Title = title
            };

            var result = await mainWindow.StorageProvider.OpenFolderPickerAsync(options);
            return result.Select(x => x.Path.LocalPath).ToArray();
        }

        public async Task<MessageBoxResult> ShowMessageBox(string title, string message, MessageBoxType type = MessageBoxType.OK)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return MessageBoxResult.OK;

            return await MessageBoxUtil.ShowDialog(mainWindow, title, message, type);
        }

        public async Task<string> ShowCustomMessageBox(string title, string message, params string[] options)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return string.Empty;

            return await MessageBoxUtil.ShowDialogCustom(mainWindow, title, message, options);
        }

        public async Task<string?> AskLoadSplitFile(string fileToSplit)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return fileToSplit;

            MessageBoxResult splitRes = await MessageBoxUtil.ShowDialog(mainWindow,
                "Split file detected", "This file ends with .split0. Create merged file?\n",
                MessageBoxType.YesNoCancel);

            if (splitRes == MessageBoxResult.Yes)
            {
                var selectedFilePath = await SaveFileDialog("Select location for merged file", System.IO.Path.GetFileName(fileToSplit[..^".split0".Length]));

                if (selectedFilePath == null)
                    return null;

                using (System.IO.FileStream mergeFile = System.IO.File.Open(selectedFilePath, System.IO.FileMode.Create))
                {
                    int idx = 0;
                    string thisSplitFileNoNum = fileToSplit.Substring(0, fileToSplit.Length - 1);
                    string thisSplitFileNum = fileToSplit;
                    while (System.IO.File.Exists(thisSplitFileNum))
                    {
                        using (System.IO.FileStream thisSplitFile = System.IO.File.OpenRead(thisSplitFileNum))
                        {
                            thisSplitFile.CopyTo(mergeFile);
                        }

                        idx++;
                        thisSplitFileNum = $"{thisSplitFileNoNum}{idx}";
                    };
                }
                return selectedFilePath;
            }
            else if (splitRes == MessageBoxResult.No)
            {
                return fileToSplit;
            }
            else //if (splitRes == MessageBoxResult.Cancel)
            {
                return null;
            }
        }

        public async Task<string> AskForVersion(string currentVersion)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return "0.0.0";
            VersionWindow window = new VersionWindow(currentVersion);
            return await window.ShowDialog<string>(mainWindow);
        }

        public async Task<string> AskForRename(string currentName)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return string.Empty;
            RenameWindow window = new RenameWindow(currentName);
            return await window.ShowDialog<string>(mainWindow);
        }

        public async Task<bool> AskForImportSerialized()
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return false;
            ImportSerializedDialog dialog = new ImportSerializedDialog();
            return await dialog.ShowDialog<bool>(mainWindow);
        }
    }
}
