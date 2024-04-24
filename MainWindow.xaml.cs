using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Win32;

namespace cloudproject
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionPath = "connection.json";
        List<string> upload=new List<string>();
        string folderId = "1F9ke9U0PW8G-XOb29Rm3_Dri_HnK7X6h";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadToDrive(List<string> fileList)
        {
            GoogleCredential googleCredential;
            using (var stream = new FileStream(connectionPath, FileMode.Open, FileAccess.Read))
            {
                googleCredential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { DriveService.ScopeConstants.Drive });

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = "Google upload"
                });

                foreach (var file in fileList)
                {
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File
                    {
                        Name = System.IO.Path.GetFileName(file),
                        Parents = new List<string> { folderId } // Ustawienie folderu docelowego
                    };

                    // Przesyłanie pliku
                    using (var streamm = new FileStream(file, FileMode.Open))
                    {
                        var request = service.Files.Create(fileMetadata, streamm, "");
                        request.Fields = "id";
                        var uploadedFile = request.Upload();
                    }
                }

                MessageBox.Show("Przesyłanie plików zakończone.");
                upload.Clear();
                listbox.Items.Clear();
            }
        }
            


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Wybierz plik do przesłania";
            openFileDialog.Filter = "Pliki tekstowe|*.txt|Wszystkie pliki|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                upload.Add(selectedFilePath);
                listbox.Items.Add(System.IO.Path.GetFileName(selectedFilePath));
            }
        }

        private void SendData_Click(object sender, RoutedEventArgs e)
        {
            if (upload.Count > 0)
            {
                UploadToDrive(upload);
            }
            else
            {
                MessageBox.Show("Nie wybrano żadnych plików do przesłania.");
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GoogleCredential googleCredential;
                using (var stream = new FileStream(connectionPath, FileMode.Open, FileAccess.Read))
                {
                    googleCredential = GoogleCredential.FromStream(stream).CreateScoped(new[] { DriveService.ScopeConstants.Drive });

                    var service = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = googleCredential,
                        ApplicationName = "Dysk pobierz"
                    });


                    var request = service.Files.List();
                    request.Q = $"'{folderId}' in parents";

                    var files = request.Execute().Files;

                    if (files != null && files.Count > 0)
                    {
                        listbox2.Items.Clear();

                        foreach (var file in files)
                        {
                            listbox2.Items.Add(file.Name);
                        }

                    }
                }
            }
            catch 
            {
                
            }
        }
    }
}