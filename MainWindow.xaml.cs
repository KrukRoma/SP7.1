using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 

namespace PD_311_AsyncAwait
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchWord = wordTextBox.Text; 
            string directoryPath = pathTextBox.Text; 

            resultsList.Items.Clear(); 
            progressBar.Value = 0; 
            var results = await SearchFilesAsync(searchWord, directoryPath);

            foreach (var result in results)
            {
                resultsList.Items.Add($"File: {result.FileName}, Path: {result.FilePath}, Occurrences: {result.Count}");
            }
        }

        private async Task<List<SearchResult>> SearchFilesAsync(string searchWord, string directoryPath)
        {
            var results = new List<SearchResult>();
            var files = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            int fileCount = files.Count();
            int processedFiles = 0;

            foreach (var file in files)
            {
                int count = await CountOccurrencesAsync(file, searchWord);
                if (count > 0)
                {
                    results.Add(new SearchResult
                    {
                        FileName = Path.GetFileName(file),
                        FilePath = file,
                        Count = count
                    });
                }

                processedFiles++;
                progressBar.Value = (double)processedFiles / fileCount * 100; 
            }

            return results;
        }

        private async Task<int> CountOccurrencesAsync(string filePath, string searchWord)
        {
            var content = await File.ReadAllTextAsync(filePath);
            return (content.Split(new[] { searchWord }, StringSplitOptions.None).Length - 1);
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new CommonOpenFileDialog();
            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathTextBox.Text = fileDialog.FileName; 
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathTextBox.Text = folderDialog.FileName; 
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new CommonSaveFileDialog();
            saveDialog.Filters.Add(new CommonFileDialogFilter("Text Files", "*.txt"));
            if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SaveResultsToFile(saveDialog.FileName);
            }
        }

        private void SaveResultsToFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var item in resultsList.Items)
                {
                    writer.WriteLine(item.ToString());
                }
            }
            MessageBox.Show("Results saved to file.");
        }

        private void wordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (wordTextBox.Text == "Enter search word")
            {
                wordTextBox.Text = string.Empty;
                wordTextBox.Foreground = Brushes.Black; 
            }
        }

        private void wordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(wordTextBox.Text))
            {
                wordTextBox.Text = "Enter search word";
                wordTextBox.Foreground = Brushes.Gray; 
            }
        }

        private void pathTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (pathTextBox.Text == "Enter or select directory")
            {
                pathTextBox.Text = string.Empty;
                pathTextBox.Foreground = Brushes.Black; 
            }
        }

        private void pathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pathTextBox.Text))
            {
                pathTextBox.Text = "Enter or select directory";
                pathTextBox.Foreground = Brushes.Gray; 
            }
        }
    }

    public class SearchResult
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int Count { get; set; }
    }
}
