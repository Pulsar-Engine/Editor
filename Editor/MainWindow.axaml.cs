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
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Avalonia.Media;

namespace PulsarApp
{
    public partial class MainWindow : Window
    {
        // États de visibilité
        private bool _fileExplorerVisible = true;
        private bool _gameViewVisible = true;
        private bool _consoleVisible = true;
        private bool _editorVisible = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeControls();
            SetupEventHandlers();
            FileExplorerTree.AddHandler(InputElement.DoubleTappedEvent, OnFileDoubleClick, RoutingStrategies.Bubble);
            this.SizeChanged += OnWindowSizeChanged;
        }

        private void InitializeControls()
        {
            // Initialisation des références aux contrôles
            SelectToolButton = this.FindControl<Button>("SelectToolButton");
            MoveToolButton = this.FindControl<Button>("MoveToolButton");
            MoreToolsButton = this.FindControl<Button>("MoreToolsButton");
            GridToggle = this.FindControl<ToggleButton>("GridToggle");
            SnapToggle = this.FindControl<ToggleButton>("SnapToggle");
        }

        private void SetupEventHandlers()
        {
            FileExplorerTree.AddHandler(InputElement.DoubleTappedEvent, OnFileDoubleClick, RoutingStrategies.Bubble);
            this.SizeChanged += OnWindowSizeChanged;
            
            // Initialisation des états
            if (SelectToolButton != null) 
                SelectToolButton.IsEnabled = false; // Mode sélection activé par défaut
        }

        private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateToolsVisibility();
        }

        private void UpdateToolsVisibility()
        {
            if (GameViewPanel != null && MoreToolsButton != null)
            {
                MoreToolsButton.IsVisible = GameViewPanel.Bounds.Width < 800;
            }
        }

        private void ToggleEditor(object? sender, RoutedEventArgs e)
        {
            var col2 = MainGrid.ColumnDefinitions[2];
            var col3Splitter = MainGrid.ColumnDefinitions[3];
            _editorVisible = !_editorVisible;

            if (_editorVisible)
            {
                EditorPanel.IsVisible = true;
                col2.Width = new GridLength(1, GridUnitType.Star);
                col3Splitter.Width = new GridLength(5);
            }
            else
            {
                EditorPanel.IsVisible = false;
                col2.Width = new GridLength(0);
                col3Splitter.Width = new GridLength(0);
            }

            if (sender is MenuItem menuItem)
                UpdateMenuItemHeader(menuItem, "Editor", _editorVisible);
        }

        private void OnFileDoubleClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is Control control && 
                control.DataContext is TreeViewItem item && 
                item.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    OpenFileInEditor(filePath);
                }
            }
            else if (e.Source is TextBlock textBlock && 
                     textBlock.DataContext is TreeViewItem altItem && 
                     altItem.Tag is string altFilePath)
            {
                if (File.Exists(altFilePath))
                {
                    OpenFileInEditor(altFilePath);
                }
            }
        }

        private void OpenFileInEditor(string filePath)
        {
            try
            {
                if (EditorTabs?.Items == null) return;
                
                // Vérifie si le fichier est déjà ouvert
                var existingTab = EditorTabs.Items.OfType<TabItem>()
                    .FirstOrDefault(tab => tab.Tag is string path && path == filePath);
                
                if (existingTab != null)
                {
                    EditorTabs.SelectedItem = existingTab;
                    return;
                }

                var content = File.ReadAllText(filePath);
                var textBox = new TextBox 
                { 
                    Text = content,
                    AcceptsReturn = true,
                    AcceptsTab = true,
                    TextWrapping = TextWrapping.NoWrap,
                    FontFamily = "Consolas"
                };

                var tabItem = new TabItem 
                { 
                    Header = Path.GetFileName(filePath),
                    Content = new ScrollViewer { Content = textBox },
                    Tag = filePath,
                    ContextMenu = CreateTabContextMenu(filePath, textBox)
                };
                
                EditorTabs.Items.Add(tabItem);
                EditorTabs.SelectedItem = tabItem;
            }
            catch (Exception ex)
            {
                AppendConsoleText($"Erreur lors de l'ouverture du fichier: {ex.Message}");
            }
        }

        private ContextMenu? CreateTabContextMenu(string filePath, TextBox editor)
        {
            if (editor == null) return null;
            
            var menu = new ContextMenu();
            
            var saveItem = new MenuItem { Header = "Enregistrer" };
            saveItem.Click += (s, e) => SaveFile(filePath, editor.Text ?? string.Empty);
            
            var closeItem = new MenuItem { Header = "Fermer" };
            closeItem.Click += (s, e) => CloseTab(filePath);
            
            menu.Items.Add(saveItem);
            menu.Items.Add(closeItem);
            
            return menu;
        }

        private void SaveFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                AppendConsoleText($"Fichier enregistré: {filePath}");
            }
            catch (Exception ex)
            {
                AppendConsoleText($"Erreur lors de l'enregistrement: {ex.Message}");
            }
        }

        private void SaveCurrentFile(object? sender, RoutedEventArgs e)
        {
            if (EditorTabs.SelectedItem is TabItem currentTab && currentTab.Tag is string filePath)
            {
                try
                {
                    if (currentTab.Content is ScrollViewer scrollViewer && 
                        scrollViewer.Content is TextBox textBox)
                    {
                        File.WriteAllText(filePath, textBox.Text);
                        AppendConsoleText($"Fichier sauvegardé: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    AppendConsoleText($"Erreur lors de la sauvegarde: {ex.Message}");
                }
            }
            else
            {
                AppendConsoleText("Aucun fichier à sauvegarder");
            }
        }

        private void CloseCurrentTab(object? sender, RoutedEventArgs e)
        {
            if (EditorTabs.SelectedItem is TabItem tab && EditorTabs.Items.Count > 1)
            {
                EditorTabs.Items.Remove(tab);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
            {
                SaveCurrentFile(this, new RoutedEventArgs());
                e.Handled = true;
                return;
            }
            
            if (GameViewPanel.IsPointerOver)
            {
                switch (e.Key)
                {
                    case Key.D when e.KeyModifiers == KeyModifiers.Control:
                        OnDuplicateClicked(this, new RoutedEventArgs());
                        break;
                    case Key.Delete:
                        OnDeleteClicked(this, new RoutedEventArgs());
                        break;
                    case Key.V:
                        if (SelectToolButton != null)
                            OnSelectToolClicked(SelectToolButton, new RoutedEventArgs());
                        break;
                    case Key.M:
                        if (MoveToolButton != null)
                            OnMoveToolClicked(MoveToolButton, new RoutedEventArgs());
                        break;
                }
                e.Handled = true;
            }
        }

        private void OnSelectToolClicked(object? sender, RoutedEventArgs e)
        {
            if (MoveToolButton != null) MoveToolButton.IsEnabled = true;
            if (sender is Button btn) btn.IsEnabled = false;
            AppendConsoleText("Mode sélection activé");
        }

        private void OnMoveToolClicked(object? sender, RoutedEventArgs e)
        {
            if (SelectToolButton != null) SelectToolButton.IsEnabled = true;
            if (sender is Button btn) btn.IsEnabled = false;
            AppendConsoleText("Mode déplacement activé");
        }

        private void OnDuplicateClicked(object? sender, RoutedEventArgs e)
        {
            AppendConsoleText("Duplication demandée");
            // Implémentez votre logique de duplication ici
        }

        private void OnDeleteClicked(object? sender, RoutedEventArgs e)
        {
            AppendConsoleText("Suppression demandée");
            // Implémentez votre logique de suppression ici
        }

        private void OnAlignXClicked(object? sender, RoutedEventArgs e)
        {
            AppendConsoleText("Alignement sur X demandé");
        }

        private void OnAlignYClicked(object? sender, RoutedEventArgs e)
        {
            AppendConsoleText("Alignement sur Y demandé");
        }

        private void OnToggleGrid(object? sender, RoutedEventArgs e)
        {
            if (GridToggle != null)
            {
                GridToggle.IsChecked = !GridToggle.IsChecked;
                AppendConsoleText($"Grille {(GridToggle.IsChecked == true ? "activée" : "désactivée")}");
            }
        }

        private void OnToggleSnap(object? sender, RoutedEventArgs e)
        {
            if (SnapToggle != null)
            {
                SnapToggle.IsChecked = !SnapToggle.IsChecked;
                AppendConsoleText($"Snap {(SnapToggle.IsChecked == true ? "activé" : "désactivé")}");
            }
        }

        private void CloseTab(string filePath)
        {
            if (EditorTabs?.Items == null) return;
            
            for (int i = 0; i < EditorTabs.Items.Count; i++)
            {
                if (EditorTabs.Items[i] is TabItem tab && tab.Tag is string path && path == filePath)
                {
                    EditorTabs.Items.RemoveAt(i);
                    break;
                }
            }
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
                LoadDirectory(path);
                NoFolderMessage.IsVisible = false;
                FileExplorerTree.IsVisible = true;
            }
        }

        private void LoadDirectory(string path)
        {
            var rootItem = new FileSystemItem
            {
                Name = Path.GetFileName(path),
                FullPath = path,
                IsDirectory = true,
                Children = GetDirectoryItems(path)
            };

            FileExplorerTree.ItemsSource = new List<FileSystemItem> { rootItem };
        }

        private List<FileSystemItem> GetDirectoryItems(string path)
        {
            var items = new List<FileSystemItem>();

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    items.Add(new FileSystemItem
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir,
                        IsDirectory = true,
                        Children = GetDirectoryItems(dir)
                    });
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    items.Add(new FileSystemItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    });
                }
            }
            catch
            {
                // Ignorer les erreurs
            }

            return items;
        }

        private void AppendConsoleText(string text)
        {
            if (ConsoleOutput == null) return;
            
            ConsoleOutput.Text += text + Environment.NewLine;
            ConsoleOutput.CaretIndex = ConsoleOutput.Text.Length;

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
            if (ConsoleInput == null || string.IsNullOrWhiteSpace(ConsoleInput.Text))
                return;

            var command = ConsoleInput.Text.Trim();
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
                ConsoleInput!.Text = files[0].Path.LocalPath;
                ExecuteCommandAsync();
            }
        }

        private void ToggleFileExplorer(object? sender, RoutedEventArgs e)
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

        private void ToggleGameView(object? sender, RoutedEventArgs e)
        {
            var col4 = MainGrid.ColumnDefinitions[4];
            var col3Splitter = MainGrid.ColumnDefinitions[3];
            _gameViewVisible = !_gameViewVisible;

            if (_gameViewVisible)
            {
                GameViewPanel.IsVisible = true;
                col4.Width = new GridLength(1, GridUnitType.Star);
                col3Splitter.Width = new GridLength(5);
            }
            else
            {
                GameViewPanel.IsVisible = false;
                col4.Width = new GridLength(0);
                col3Splitter.Width = new GridLength(0);
            }

            if (sender is MenuItem menuItem)
                UpdateMenuItemHeader(menuItem, "Game View", _gameViewVisible);
        }

        private void ToggleConsole(object? sender, RoutedEventArgs e)
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