using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Shapes;

namespace KlientPrir
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            // Set initial visibility
            UpdatePlaceholderVisibility();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            // Update Username placeholder visibility
            txtUsernamePlaceholder.Visibility = string.IsNullOrEmpty(txtUsername.Text) ? Visibility.Visible : Visibility.Collapsed;

            // Update Password placeholder visibility
            txtPasswordPlaceholder.Visibility = string.IsNullOrEmpty(txtPassword.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            await UploadFile();
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            await DownloadFile();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            await DeleteFile();
        }

        private async void BtnReverse_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItem is FileRecord selectedFile)
            {
                var response = await client.PostAsync($"https://localhost:7036/api/files/reverse/{selectedFile.Id}", null);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("File reversed successfully!");
                    await LoadFiles();
                }
                else
                {
                    MessageBox.Show("File reversal failed!");
                }
            }
            else
            {
                MessageBox.Show("Please select a file to reverse.");
            }
        }


        private async Task Login()
        {
            var loginModel = new { Username = txtUsername.Text, Password = txtPassword.Password };
            var response = await client.PostAsJsonAsync("https://localhost:7036/api/auth/login", loginModel);
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                MessageBox.Show("Login successful!");
                await LoadFiles();
            }
            else
            {
                MessageBox.Show("Login failed!");
            }
        }

        private async Task LoadFiles()
        {
            var response = await client.GetAsync("https://localhost:7036/api/files/list");
            if (response.IsSuccessStatusCode)
            {
                var files = await response.Content.ReadFromJsonAsync<List<FileRecord>>();
                lstFiles.ItemsSource = files;
            }
        }

        private async Task UploadFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(File.OpenRead(filePath)), "file", System.IO.Path.GetFileName(filePath));
                var response = await client.PostAsync("https://localhost:7036/api/files/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("File uploaded successfully!");
                    await LoadFiles();
                }
                else
                {
                    MessageBox.Show("File upload failed!");
                }
            }
        }

        private async Task DownloadFile()
        {
            if (lstFiles.SelectedItem is FileRecord selectedFile)
            {
                var response = await client.GetAsync($"https://localhost:7036/api/files/download/{selectedFile.Id}");
                if (response.IsSuccessStatusCode)
                {
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog { FileName = selectedFile.FileName };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                        MessageBox.Show("File downloaded successfully!");
                    }
                }
                else
                {
                    MessageBox.Show("File download failed!");
                }
            }
        }

        private async Task DeleteFile()
        {
            if (lstFiles.SelectedItem is FileRecord selectedFile)
            {
                var response = await client.DeleteAsync($"https://localhost:7036/api/files/delete/{selectedFile.Id}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("File deleted successfully!");
                    await LoadFiles();
                }
                else
                {
                    MessageBox.Show("File deletion failed!");
                }
            }
        }
    }

    public class FileRecord
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int UserId { get; set; }

        public override string ToString()
        {
            return FileName;
        }
    }
}