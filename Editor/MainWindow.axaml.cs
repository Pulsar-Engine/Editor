using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.VisualTree;

namespace YourApp
{
    public partial class MainWindow : Window
    {
        private bool _fileExplorerVisible = true;
        private bool _gameViewVisible = true;
        private bool _consoleVisible = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenFolder(object? sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads)
            });

            if (folder.Count > 0)
            {
                var path = folder[0].Path.LocalPath;
                LoadDirectory(path, FileExplorerTree);
                NoFolderMessage.IsVisible = false;
                FileExplorerTree.IsVisible = true;
            }
        }

        private void LoadDirectory(string path, TreeView tree)
        {
            var root = new TreeViewItem { Header = Path.GetFileName(path), Tag = path };
            AddDirectoryItems(path, root);
            tree.ItemsSource = new List<TreeViewItem> { root };
        }

        private void AddDirectoryItems(string path, TreeViewItem parent)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    var dirItem = new TreeViewItem { Header = Path.GetFileName(dir), Tag = dir };
                    AddDirectoryItems(dir, dirItem);
                    parent.Items.Add(dirItem);
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    var fileItem = new TreeViewItem { Header = Path.GetFileName(file), Tag = file };
                    parent.Items.Add(fileItem);
                }
            }
            catch
            {
                // On ignore les exceptions pour ne pas planter
            }
        }

        private void AppendConsoleText(string text)
        {
            // Ajoute du texte à la TextBox console en gardant un saut de ligne
            ConsoleOutput.Text += text + "\n";

            // Place le caret à la fin du texte pour suivre le scroll
            ConsoleOutput.CaretIndex = ConsoleOutput.Text.Length;

            // Scroll automatique vers le bas
            var scrollViewer = ConsoleOutput.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();

            scrollViewer?.ScrollToEnd();
        }

        private void OnExecuteButtonClick(object? sender, RoutedEventArgs e)
        {
            ExecuteCommandAsync();
        }

        private void OnConsoleInputKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteCommandAsync();
                e.Handled = true;
            }
        }

        private async void ExecuteCommandAsync()
        {
            var command = ConsoleInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(command))
                return;

            ConsoleInput.Text = string.Empty;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "neutron",
                    Arguments = $"\"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        AppendConsoleText("Erreur : Impossible de démarrer le processus.");
                        return;
                    }

                    // Lecture asynchrone des sorties pour ne pas bloquer l’UI
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    await Task.WhenAll(outputTask, errorTask);

                    if (!string.IsNullOrWhiteSpace(outputTask.Result))
                        AppendConsoleText(outputTask.Result.Trim());

                    if (!string.IsNullOrWhiteSpace(errorTask.Result))
                        AppendConsoleText("Erreur : " + errorTask.Result.Trim());

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                AppendConsoleText("Exception : " + ex.Message);
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
                ExecuteCommandAsync();
            }
        }

        private void ToggleFileExplorer(object sender, RoutedEventArgs e)
        {
            var col0 = MainGrid.ColumnDefinitions[0];
            _fileExplorerVisible = !_fileExplorerVisible;

            if (_fileExplorerVisible)
            {
                col0.Width = new GridLength(250);
                FileExplorerPanel.IsVisible = true;
            }
            else
            {
                col0.Width = new GridLength(0);
                FileExplorerPanel.IsVisible = false;
            }

            if (sender is MenuItem menuItem)
                UpdateMenuItemHeader(menuItem, "File Explorer", _fileExplorerVisible);
        }

        private void ToggleGameView(object sender, RoutedEventArgs e)
        {
            var col2 = MainGrid.ColumnDefinitions[2];
            _gameViewVisible = !_gameViewVisible;

            if (_gameViewVisible)
            {
                GameViewPanel.IsVisible = true;
                col2.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                GameViewPanel.IsVisible = false;
                col2.Width = new GridLength(0);
            }

            if (sender is MenuItem menuItem)
                UpdateMenuItemHeader(menuItem, "Game View", _gameViewVisible);
        }

        private void ToggleConsole(object sender, RoutedEventArgs e)
        {
            var row2 = MainGrid.RowDefinitions[2];
            _consoleVisible = !_consoleVisible;

            if (_consoleVisible)
            {
                ConsolePanel.IsVisible = true;
                row2.Height = new GridLength(250);
            }
            else
            {
                ConsolePanel.IsVisible = false;
                row2.Height = new GridLength(0);
            }

            if (sender is MenuItem menuItem)
                UpdateMenuItemHeader(menuItem, "Console", _consoleVisible);
        }

        private void UpdateMenuItemHeader(MenuItem menuItem, string baseText, bool visible)
        {
            menuItem.Header = (visible ? "✓ " : "✗ ") + baseText;
        }

        private void Exit(object? sender, RoutedEventArgs e) => Close();
    }
}
