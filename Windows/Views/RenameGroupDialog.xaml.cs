using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class RenameGroupDialog : Window
    {
        public string GroupTitle { get; private set; } = "Group";
        public string SelectedColorHex { get; private set; } = "#3B82F6";

        public RenameGroupDialog(string currentTitle, string currentColorHex)
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);

            GroupTitle = string.IsNullOrWhiteSpace(currentTitle) ? "Group" : currentTitle;
            SelectedColorHex = string.IsNullOrWhiteSpace(currentColorHex) ? "#3B82F6" : currentColorHex;

            TbGroupName.Text = GroupTitle;
            TbGroupName.SelectAll();
            TbGroupName.Focus();

            PopulateColors();
        }

        private void PopulateColors()
        {
            PanelColors.Children.Clear();
            foreach (var (name, hex) in TabGroup.ChromeColors)
            {
                bool isSelected = string.Equals(hex, SelectedColorHex, StringComparison.OrdinalIgnoreCase);

                var border = new Border
                {
                    Width = 24,
                    Height = 24,
                    CornerRadius = new CornerRadius(12),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)),
                    BorderBrush = isSelected ? Brushes.White : Brushes.Transparent,
                    BorderThickness = new Thickness(isSelected ? 2.5 : 0),
                    Margin = new Thickness(0, 0, 8, 0),
                    Cursor = Cursors.Hand,
                    ToolTip = name
                };

                string capturedHex = hex;
                border.MouseDown += (s, e) =>
                {
                    SelectedColorHex = capturedHex;
                    PopulateColors();
                };

                PanelColors.Children.Add(border);
            }
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnSaveClicked(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            GroupTitle = string.IsNullOrWhiteSpace(TbGroupName.Text) ? "Group" : TbGroupName.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
