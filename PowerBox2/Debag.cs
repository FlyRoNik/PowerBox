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
    static class Debag
    {
        private static string filename;
        private static int fileCount;
        private static StorageFolder localFolder;
        private static StorageFile helloFile;
        private static MySemaphore _pool = new MySemaphore(1, 1);
        private static StorageFolder sdCard;
        private static StorageFolder testFolder;

        public static void createdirectory()
        {
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            myIsolatedStorage.CreateDirectory("TextFilesFolder");
            filename = "TextFilesFolder\\Samplefile";
            fileCount = 0;
        }

        public async static void createDirectorySD()
        {
            // Get the logical root folder for all external storage devices.
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;

            // Get the first child folder, which represents the SD card.
            sdCard = await (await externalDevices.GetFoldersAsync()).FirstOrDefault().CreateFolderAsync("TextFilesFolder");
        }

        public async static void WriteCD(string someTextData)
        {
            if (sdCard != null)
            {
                // An SD card is present and the sdCard variable now contains a reference to it.
                var testFile = await sdCard.CreateFileAsync("Test.txt");

                await FileIO.AppendTextAsync(testFile, someTextData + Environment.NewLine);
            }
            else
            {
                // No SD card is present.
            }
        }

        public static void Write(string someTextData)
        {
            _pool.Wait();
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            using (StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream(filename + fileCount+ "_" + someTextData + ".txt", FileMode.OpenOrCreate, FileAccess.Write, myIsolatedStorage)))
            {
                writeFile.Write(someTextData);
                writeFile.Dispose();
                fileCount++;
            }
            _pool.TryRelease();
        }

        public static async void createdirectoryA()
        {
            localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TextFilesFolder");
            filename = "Samplefile.txt";
            // создаем файл 
            helloFile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        }

        public static async void WriteA(string someTextData)
        {
            _pool.Wait();
           
            // если файл уже ранее был создан, то мы можем получить его через GetFileAsync()
            //helloFile = await localFolder.GetFileAsync(filename);

            // запись в файл
            await FileIO.AppendTextAsync(helloFile, someTextData + Environment.NewLine);
            _pool.TryRelease();
        }

    }
}
