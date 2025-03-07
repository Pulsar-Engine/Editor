using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;

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
            string command = ConsoleInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(command))
                return;

            ConsoleInput.Text = "";
            ConsoleOutput.Text += "> " + command + Environment.NewLine;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe", // Utilise "bash" sur Linux/macOS si nécessaire
                    Arguments = "/C " + command,
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
    }
}
