using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Editeur
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ScrollToBottom()
        {
            if (ConsoleOutput.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToEnd();
            }
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
            string filePath = ConsoleInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            ConsoleInput.Text = "";
            ConsoleOutput.Text += "> Running: " + filePath + Environment.NewLine;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "neutron", // Le binaire Rust
                    Arguments = filePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                        ConsoleOutput.Text += output + Environment.NewLine;

                    if (!string.IsNullOrWhiteSpace(error))
                        ConsoleOutput.Text += "Erreur : " + error + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                ConsoleOutput.Text += "Erreur d'exécution : " + ex.Message + Environment.NewLine;
            }

            ScrollToBottom();
        }

        private async void OnOpenFileClicked(object? sender, RoutedEventArgs e)
        {
            var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
            if (storageProvider == null) return;

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choisir un fichier Neutron",
                AllowMultiple = false
            });

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
    }
}
