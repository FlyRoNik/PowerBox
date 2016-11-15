using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace PowerBox2
{
    class Debag
    {
        private const string FOLDER_MAIN = "PowerBox";
        private const string FOLDER_CHECKS = "CHECKS";
        private const string FOLDER_WATCHING = "WATCHING";
        private const string FOLDER_DEBAG = "DEBAG";

        private const string FILE_DEBAG = "Debag_";
        private const string FILE_CHECK = "CHECK_";

        private StorageFolder FolderMain;
        private StorageFolder FolderChecks;
        private StorageFolder FolderWatching;
        private StorageFolder FolderDebags;
        private StorageFile FileDebag;
        private StorageFile FileCheck;
        private int fileCountCheck;
        private int fileCountDebag;

        private MySemaphore _pool = new MySemaphore(1, 1);

        public enum ValueFolder : byte
        {
            FolderMain = 0,
            FolderChecks = 1,
            FolderWatching = 2,
            FolderDebags = 3
        }

        //private string filename;
        //private int fileCount;
        //private StorageFolder localFolder;
        //private StorageFile helloFile;

        public Debag()
        {
            var task = Task.Run(async () => { await initSD(); });
            task.Wait();
        }

        public async Task initSD()
        {
            // Get the logical root folder for all external storage devices.
            StorageFolder rootFolder = (await KnownFolders.RemovableDevices.GetFoldersAsync()).FirstOrDefault();
            if (rootFolder != null)
            {
                FolderMain = await createFolderInFolder(rootFolder, FOLDER_MAIN);
                FolderChecks = await createFolderInFolder(FolderMain, FOLDER_CHECKS);
                FolderWatching = await createFolderInFolder(FolderMain, FOLDER_WATCHING);
                FolderDebags = await createFolderInFolder(FolderMain, FOLDER_DEBAG);

                fileCountCheck = (await FolderChecks.GetFilesAsync()).Count + 1;
                fileCountDebag = (await FolderDebags.GetFilesAsync()).Count + 1;
            }
            else
            {
                // No SD card is present.
            }
            
        }

        public async void dellFolderSD(ValueFolder value)
        {
            switch (value)
            {
                case ValueFolder.FolderMain:
                    await FolderMain.DeleteAsync();
                    break;
                case ValueFolder.FolderChecks:
                    await FolderChecks.DeleteAsync();
                    break;
                case ValueFolder.FolderWatching:
                    await FolderWatching.DeleteAsync();
                    break;
                case ValueFolder.FolderDebags:
                    await FolderDebags.DeleteAsync();
                    break;
                default:
                    break;
            }          
        }

        private async Task<StorageFolder> createFolderInFolder(StorageFolder rootFolder, string NameNewFolder)
        {
            if (!isFolderName(await rootFolder.GetFoldersAsync(), NameNewFolder))
            {
                return await rootFolder.CreateFolderAsync(NameNewFolder);
            }
            else
            {
                return await rootFolder.GetFolderAsync(NameNewFolder);
            }
        }

        private bool isFolderName(IReadOnlyList<StorageFolder> listFolders, string nameFolder)
        {
            foreach (StorageFolder folders in listFolders)
            {
                if (folders.Name == nameFolder)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isFileName(IReadOnlyList<StorageFile> listFile, string nameFile)
        {
            foreach (StorageFile files in listFile)
            {
                if (files.Name == nameFile)
                {
                    return true;
                }
            }
            return false;
        }

        public void WriteSD_CheckIn(string someTextData)
        {
            WriteSD("cell_in" + someTextData);
        }

        public void WriteSD_CheckOut(string someTextData)
        {
            WriteSD("cell_out" + someTextData);
        }

        private async void WriteSD(string someTextData)
        {
            // An SD card is present and the sdCard variable now contains a reference to it.
            IReadOnlyList<StorageFile> listFile = await FolderChecks.GetFilesAsync();

            string fileName = FILE_CHECK + fileCountCheck + someTextData + ".txt";

            if (!isFileName(listFile, fileName))
            {
                FileCheck = await FolderChecks.CreateFileAsync(fileName);
            }
            else
            {
                FileCheck = await FolderChecks.GetFileAsync(fileName);
            }

            await FileIO.AppendTextAsync(FileCheck, someTextData + Environment.NewLine);
        }

        public async void WriteSD_Debag(string someTextData)
        {
            // An SD card is present and the sdCard variable now contains a reference to it.
            IReadOnlyList<StorageFile> listFile = await FolderDebags.GetFilesAsync();

            string fileName = FILE_DEBAG + fileCountDebag + someTextData + ".txt";

            if (!isFileName(listFile, fileName))
            {
                FileDebag = await FolderDebags.CreateFileAsync(fileName);
            }
            else
            {
                FileDebag = await FolderDebags.GetFileAsync(fileName);
            }

            await FileIO.AppendTextAsync(FileDebag, someTextData + Environment.NewLine);
        }

        //public void createdirectory()
        //{
        //    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
        //    myIsolatedStorage.CreateDirectory("TextFilesFolder");
        //    filename = "TextFilesFolder\\Samplefile";
        //    fileCount = 0;
        //}

        //public void WriteSD_Debag(string someTextData)
        //{
        //    _pool.Wait();
        //    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
        //    using (StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream(filename + fileCount+ "_" + someTextData + ".txt", FileMode.OpenOrCreate, FileAccess.Write, myIsolatedStorage)))
        //    {
        //        writeFile.Write(someTextData);
        //        writeFile.Dispose();
        //        fileCount++;
        //    }
        //    _pool.TryRelease();
        //}

        //public async void createdirectoryA()
        //{
        //    localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TextFilesFolder");
        //    filename = "Samplefile.txt";
        //    // создаем файл 
        //    helloFile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        //}

        //public async void WriteA(string someTextData)
        //{
        //    _pool.Wait();
           
        //    // если файл уже ранее был создан, то мы можем получить его через GetFileAsync()
        //    //helloFile = await localFolder.GetFileAsync(filename);

        //    // запись в файл
        //    await FileIO.AppendTextAsync(helloFile, someTextData + Environment.NewLine);
        //    _pool.TryRelease();
        //}
    }
}
