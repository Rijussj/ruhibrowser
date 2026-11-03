using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class PdfToolsWindow : Window
    {
        public string? PdfPath { get; private set; }
        public string? SelectedAction { get; private set; }

        public PdfToolsWindow(string? currentPdf = null)
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);

            if (!string.IsNullOrWhiteSpace(currentPdf))
            {
                SetPdfPath(currentPdf);
            }
        }

        private void SetPdfPath(string path)
        {
            if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(path);
                    path = uri.LocalPath;
                }
                catch { }
            }

            PdfPath = path;
            TbPdfPath.Text = path;
        }

        private void OnBrowseClicked(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select PDF File - RUHI Browser",
                Filter = "PDF Documents (*.pdf)|*.pdf|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) == true && !string.IsNullOrWhiteSpace(dlg.FileName))
            {
                SetPdfPath(dlg.FileName);
            }
        }

        private void OnOpenInTabClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PdfPath) || !File.Exists(PdfPath))
            {
                MessageBox.Show(this, "Please select a valid PDF file first.", "PDF Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedAction = "OpenTab";
            DialogResult = true;
            Close();
        }

        private void OnRotateClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PdfPath) || !File.Exists(PdfPath))
            {
                MessageBox.Show(this, "Please select a valid PDF file first.", "PDF Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedAction = "Rotate";
            DialogResult = true;
            Close();
        }

        private void OnPrintClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PdfPath) || !File.Exists(PdfPath))
            {
                MessageBox.Show(this, "Please select a valid PDF file first.", "PDF Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedAction = "Print";
            DialogResult = true;
            Close();
        }

        private void OnOpenExternalClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PdfPath) || !File.Exists(PdfPath))
            {
                MessageBox.Show(this, "Please select a valid PDF file first.", "PDF Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = PdfPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open external app: " + ex.Message, "External App Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
