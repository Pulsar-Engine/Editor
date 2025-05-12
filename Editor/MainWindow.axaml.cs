using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Editeur
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
            string filePath = ConsoleInput.Text.Trim();
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
    }
}
