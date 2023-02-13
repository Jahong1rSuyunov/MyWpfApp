using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MyWpfApp.Extinsion;
using MyWpfApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MyWpfApp.Storage
{
    public partial class Storages
    {
        private static string[] scopes = { DriveService.Scope.Drive };
        private static string appName = "DriveApi";
        private const string CRED_PATH = "token";

        private UserCredential _credential;

        public bool ConnectDrive()
        {
            var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

            try
            {
                _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    Environment.UserName,
                    CancellationToken.None,
                    new FileDataStore(CRED_PATH, true)).Result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public void DeleteToken()
        {
            // Get all files in the folder
            string[] files = Directory.GetFiles(CRED_PATH);

            // Delete each file
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
        public UserCredential GetCredential()
        {
            var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

            _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    Environment.UserName,
                    CancellationToken.None,
                    new FileDataStore(CRED_PATH, true)).Result;

            return _credential;
        }

        public List<DriveFile> GetDriveFiles(string Id = "")
        {
            var result = new List<DriveFile>();

            // Create Drive API service.
            var service = GetDriveService();

            FilesResource.ListRequest listRequest = service.Files.List();

            string query;
            if (!string.IsNullOrEmpty(Id))
                query = $"(mimeType='text/csv' "
                 + "or mimeType='application/vnd.google-apps.spreadsheet' "
                 + "or mimeType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' "
                + "or mimeType='application/vnd.ms-excel') "
                + $"and ('{Id}' in parents and trashed = false)";
            else
                query = $"mimeType='text/csv' "
                + "or mimeType='application/vnd.ms-excel' "
                + "or mimeType='application/vnd.google-apps.spreadsheet' "
                + "or mimeType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ";

            listRequest.Q = query;

            var fileList = listRequest.Execute();

            foreach (var item in fileList.Files)
            {
                var drive = new DriveFile();
                drive.Id = item.Id;
                drive.Name = item.Name.CutStringUpToDot();
                drive.MimeType = item.MimeType;
                drive.Kind = item.Kind;
                drive.FileType = item.MimeType.ConvertType();
                result.Add(drive);
            }

            return result;
        }

        public List<DriveFile> GetDriveFolderFiles(string Id = "")
        {
            var result = new List<DriveFile>();

            // Create Drive API service.
            var service = GetDriveService();

            FilesResource.ListRequest listRequest = service.Files.List();
            string query;
            if (!string.IsNullOrEmpty(Id))
                query = $"mimeType='application/vnd.google-apps.folder' and '{Id}' in parents and trashed = false";
            else
                query = "mimeType='application/vnd.google-apps.folder'";

            listRequest.Q = query;

            var fileList = listRequest.Execute();

            foreach (var item in fileList.Files)
            {
                var drive = new DriveFile();
                drive.Id = item.Id;
                drive.Name = item.Name.CutStringUpToDot();
                drive.MimeType = item.MimeType;
                drive.Kind = item.Kind;
                drive.FileType = item.MimeType.ConvertType();
                result.Add(drive);
            }

            return result;
        }

        public DriveService GetDriveService()
        {
            GetCredential();

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = appName
            });
        }



        #region Download
        public bool CsvDownload(DriveFile driveFile)
        {
            var service = GetDriveService();

            var request = service.Files.Get(driveFile.Id);

            var stream = new MemoryStream();

            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine($"Download failed. {progress.Exception.Message}");
                            break;
                        }
                }
            };
            request.Download(stream);


            return ImportExcelToSQLite(stream, driveFile);
        }

        public bool DownloadFile(DriveFile file)
        {
            var result = false;
            switch (file.MimeType)
            {
                case "application/vnd.google-apps.spreadsheet":
                    result = SheetDownload(file);
                    break;
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    result = ExcelDownload(file);
                    break;
                case "text/csv":
                    result = CsvDownload(file);
                    break;
                case "application/vnd.ms-excel":
                    result = ExcelDownload(file);
                    break;
            }
            return result;
        }

        public bool ExcelDownload(DriveFile driveFile)
        {
            var service = GetDriveService();

            var request = service.Files.Get(driveFile.Id);

            var stream = new MemoryStream();

            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine($"Download failed. {progress.Exception.Message}");
                            break;
                        }
                }
            };
            request.Download(stream);

            return ImportExcelToSQLite(stream, driveFile);
        }

        public bool SheetDownload(DriveFile driveFile)
        {
            var service = GetDriveService();

            var request = service.Files.Export(driveFile.Id, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            var stream = new MemoryStream();


            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine($"Download failed. {progress.Exception.Message}");
                            break;
                        }
                }
            };

            request.Download(stream);

            return ImportExcelToSQLite(stream, driveFile);
        }

        #endregion
    }
}
