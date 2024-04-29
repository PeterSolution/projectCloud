using System;
using System.Collections.Generic;
using System.Configuration;
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
        List<string> upload = new List<string>();
        string idpliku = null;
        string folderId = "1F9ke9U0PW8G-XOb29Rm3_Dri_HnK7X6h";
        public MainWindow()
        {
            InitializeComponent();
        }
        private void getfileid(string nameoffile)
        {
            GoogleCredential gc;
            using (var stream = new FileStream(connectionPath, FileMode.Open, FileAccess.Read))
            {
                gc = GoogleCredential.FromStream(stream).CreateScoped(new[] { DriveService.ScopeConstants.Drive });

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = gc,
                    ApplicationName = "Google Drive API"
                });

                var request = service.Files.List();
                request.Q = $"'{folderId}' in parents and name = '{nameoffile}'";

                var files = request.Execute().Files;

                if (files != null && files.Count > 0)
                {
                    idpliku = files[0].Id;
                }
                else
                {
                    MessageBox.Show($"Nie znaleziono pliku o nazwie '{nameoffile}' w folderze na Google Dysku.");

                }
            }
        }
        private void pobierz(string fileName)
        {
            try
            {

                if (idpliku != null)
                {
                    GoogleCredential googleCredential;
                    using (var stream = new FileStream(connectionPath, FileMode.Open, FileAccess.Read))
                    {
                        googleCredential = GoogleCredential.FromStream(stream)
                            .CreateScoped(new[] { DriveService.ScopeConstants.Drive });

                        var service = new DriveService(new BaseClientService.Initializer
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = "Google Drive API"
                        });

                        var request = service.Files.Get(idpliku);
                        using (var streamDownload = new MemoryStream())
                        {
                            request.Download(streamDownload);
                            byte[] content = streamDownload.ToArray();

                            // Zapisanie pobranego pliku do lokalnego katalogu
                            string localFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                            File.WriteAllBytes(localFilePath, content);

                            MessageBox.Show($"Plik '{fileName}' został pomyślnie pobrany z Google Dysku.");
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show($"Błąd podczas pobierania pliku");
            }
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
                        Parents = new List<string> { folderId }
                    };

                    using (var streamm = new FileStream(file, FileMode.Open))
                    {
                        var request = service.Files.Create(fileMetadata, streamm, "");
                        request.Fields = "id";
                        var uploadedFile = request.Upload();
                    }
                }
                try
                {
                    GoogleCredential gc;
                    using (var st = new FileStream(connectionPath, FileMode.Open, FileAccess.Read))
                    {
                        gc = GoogleCredential.FromStream(st).CreateScoped(new[] { DriveService.ScopeConstants.Drive });

                        var ser = new DriveService(new BaseClientService.Initializer
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = "Dysk pobierz"
                        });


                        var request = ser.Files.List();
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

                MessageBox.Show("Przesyłanie plików zakończone.");
                upload.Clear();
                listbox.Items.Clear();
            }
        }



        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Wybierz plik do przesłania";
            openFileDialog.Filter = "Wszystkie pliki|*.*";

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

        private void pobwybplik_Click(object sender, RoutedEventArgs e)
        {

            int idelementu = listbox2.SelectedIndex;
            try
            {
                getfileid(listbox2.Items[idelementu].ToString());
                pobierz(listbox2.Items[idelementu].ToString());
                string nazu = Environment.UserName;
                if (Directory.Exists("C:\\Users\\" + nazu + "\\Downloads"))
                {
                    string file = AppDomain.CurrentDomain.BaseDirectory;
                    string nazwapliku = listbox2.Items[idelementu].ToString();
                    if (File.Exists(file + "\\" + nazwapliku))
                    {
                        string sourceFilePath = System.IO.Path.Combine(file, nazwapliku);
                        string destinationFolderPath = "C:\\Users\\" + nazu + "\\Downloads";
                        string destinationFilePath = System.IO.Path.Combine(destinationFolderPath, nazwapliku);
                        File.Copy(sourceFilePath, destinationFilePath, true);
                        File.Delete(sourceFilePath);
                    }
                }
                else
                {
                    Directory.CreateDirectory("C:\\Users\\" + nazu + "\\Downloads");
                    string file = AppDomain.CurrentDomain.BaseDirectory;
                    string nazwapliku = listbox2.Items[idelementu].ToString();
                    if (File.Exists(file + '\\' + nazwapliku))
                    {
                        string sourceFilePath = System.IO.Path.Combine(file, nazwapliku);
                        string destinationFolderPath = "C:\\Users\\" + nazu + "\\Downloads";
                        string destinationFilePath = System.IO.Path.Combine(destinationFolderPath, nazwapliku);
                        File.Copy(sourceFilePath, destinationFilePath, true);
                        File.Delete(sourceFilePath);
                    }

                }
            }
            catch
            {
                MessageBox.Show("Blad z wybraniem indexu");
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in listbox2.Items)
            {
                try
                {
                    getfileid(item.ToString());
                    pobierz(item.ToString());
                    string nazu = Environment.UserName;
                    if (Directory.Exists("C:\\Users\\" + nazu + "\\Downloads"))
                    {
                        string file = AppDomain.CurrentDomain.BaseDirectory;
                        string nazwapliku = item.ToString();
                        if (File.Exists(file + "\\" + nazwapliku))
                        {
                            string sourceFilePath = System.IO.Path.Combine(file, nazwapliku);
                            string destinationFolderPath = "C:\\Users\\" + nazu + "\\Downloads";
                            string destinationFilePath = System.IO.Path.Combine(destinationFolderPath, nazwapliku);
                            File.Copy(sourceFilePath, destinationFilePath, true);
                            File.Delete(sourceFilePath);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory("C:\\Users\\" + nazu + "\\Downloads");
                        string file = AppDomain.CurrentDomain.BaseDirectory;
                        string nazwapliku = item.ToString();
                        if (File.Exists(file + '\\' + nazwapliku))
                        {
                            string sourceFilePath = System.IO.Path.Combine(file, nazwapliku);
                            string destinationFolderPath = "C:\\Users\\" + nazu + "\\Downloads";
                            string destinationFilePath = System.IO.Path.Combine(destinationFolderPath, nazwapliku);
                            File.Copy(sourceFilePath, destinationFilePath, true);
                            File.Delete(sourceFilePath);
                        }

                    }
                }
                catch
                {
                    MessageBox.Show("Blad z wybraniem indexu");
                }
            }
        }
    }
}