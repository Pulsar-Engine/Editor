using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Editeur
{
    public partial class MainWindow : Window
    {
        private string? lastSelectedFolder = null;
        private Grid? RightPanel;

        public MainWindow()
        {
            InitializeComponent();
            RightPanel = this.FindControl<Grid>("RightPanel");
            ShowNoFolderSelectedMessage();
        }

        private void ToggleRightPanel(object? sender, RoutedEventArgs e)
        {
            if (RightPanel is not null)
            {
                RightPanel.IsVisible = !RightPanel.IsVisible;
            }
        }

        private void AppendToConsole(string text)
        {
            ConsoleOutput.Text += text + Environment.NewLine;
            ConsoleOutput.CaretIndex = ConsoleOutput.Text.Length;
            ConsoleOutput.BringIntoView();
        }

        private void OnConsoleCommandSubmit(object? sender, RoutedEventArgs e)
        {
            ExecuteCommand();
        }

        private void OnConsoleInputKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteCommand();
            }
        }

        private void ExecuteCommand()
        {
            string filePath = ConsoleInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            ConsoleInput.Text = "";
            AppendToConsole("> Running: " + filePath);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "neutron",
                    Arguments = filePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                        AppendToConsole(output.TrimEnd());

                    if (!string.IsNullOrWhiteSpace(error))
                        AppendToConsole("Erreur : " + error.TrimEnd());
                }
            }
            catch (Exception ex)
            {
                AppendToConsole("Erreur d'exécution : " + ex.Message);
            }
        }

        private async void OnOpenFileClicked(object? sender, RoutedEventArgs e)
        {
            var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
            if (storageProvider == null)
                return;

            var files = await storageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Choisir un fichier Neutron",
                    AllowMultiple = false,
                }
            );

            if (files.Count > 0)
            {
                ConsoleInput.Text = files[0].Path.LocalPath;
                ExecuteCommand();
            }
        }

        private void OnExitClicked(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnOpenFolderClicked(object? sender, RoutedEventArgs e)
        {
            var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
            if (storageProvider == null)
                return;

            var options = new FolderPickerOpenOptions
            {
                Title = "Choisir un dossier",
                AllowMultiple = false
            };

            if (!string.IsNullOrEmpty(lastSelectedFolder))
            {
                var parent = Directory.GetParent(lastSelectedFolder);
                if (parent != null)
                {
                    options.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(parent.FullName);
                }
            }
            else
            {
                string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                options.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(defaultPath);
            }

            var folders = await storageProvider.OpenFolderPickerAsync(options);

            if (folders.Count > 0)
            {
                lastSelectedFolder = folders[0].Path.LocalPath;
                LoadDirectoryIntoTreeView(lastSelectedFolder);
            }
            else
            {
                ShowNoFolderSelectedMessage();
            }
        }

        private void ShowNoFolderSelectedMessage()
        {
            FileExplorerTree.IsVisible = false;
            NoFolderSelectedTextBlock.IsVisible = true;
        }

        private void LoadDirectoryIntoTreeView(string path)
        {
            FileExplorerTree.Items.Clear();
            FileExplorerTree.Items.Add(CreateDirectoryNode(path));
            FileExplorerTree.IsVisible = true;
            NoFolderSelectedTextBlock.IsVisible = false;
        }

        private TreeViewItem CreateDirectoryNode(string path)
        {
            var directoryNode = new TreeViewItem
            {
                Header = Path.GetFileName(path),
                Tag = path
            };
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    directoryNode.Items.Add(CreateDirectoryNode(dir));
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    directoryNode.Items.Add(new TreeViewItem
                    {
                        Header = Path.GetFileName(file),
                        Tag = file
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorer les dossiers non accessibles
            }
            return directoryNode;
        }
    }
}
