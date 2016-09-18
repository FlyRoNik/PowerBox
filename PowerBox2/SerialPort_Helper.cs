using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace PowerBox2
{
    class SerialPort_Helper
    {
        private SerialDevice serialPort = null;

        private DataWriter dataWriteObject = null;
        private DataReader dataReaderObject = null;

        private CancellationTokenSource ReadCancellationTokenSource;

        private Action<byte[]> delegteReadAsync;
        private Action<Exception> exception;

        //public async Task RunTheMethod(Func<byte[], Task> myMethodName, byte[] mas)
        //{
        //    await myMethodName(mas);
        //}

        public SerialPort_Helper(Action<byte[]> delegteReadAsync, Action<Exception> exception)
        {
            this.delegteReadAsync = delegteReadAsync;
            this.exception = exception;
            connection();
        }

        public void setDelegate(Action<byte[]> delegteReadAsync)
        {
            this.delegteReadAsync = delegteReadAsync;
        }

        public async void connection()
        {
            string aqs = SerialDevice.GetDeviceSelector("UART0");

            var myDevices = await DeviceInformation.FindAllAsync(aqs, null);

            if (myDevices.Count == 0)
            {
                throw new Exception("Device not found...");
            }

            try
            {
                serialPort = await SerialDevice.FromIdAsync(myDevices[0].Id);

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 19200;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                Task thread = new Task(Listen);
                thread.Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // - Create a DataReader object
        // - Create an async task to read from the SerialDevice InputStream
        public async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    CloseDevice();
                    exception(new Exception("Reading task was cancelled, closing device and cleaning up"));
                }
                else
                {
                    //exception(new Exception(ex.Message));
                    connection();
                }
            }
            finally
            {
                // Cleanup once complete
                //if (dataReaderObject != null)
                //{
                //    dataReaderObject.DetachStream();
                //    dataReaderObject = null;
                //}
            }
        }

        // ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                IBuffer buffer = dataReaderObject.ReadBuffer(bytesRead);
                DataReader reader = DataReader.FromBuffer(buffer);
                byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(fileContent);
                delegteReadAsync(fileContent);
            }
        }

        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }

        public async void Write(byte[] mas)
        {
            try
            {
                if (serialPort != null)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteAsync(mas);
                }
                else
                {
                    throw new Exception("Select a device and connect");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }

        // WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        private async Task WriteAsync(byte[] mas)
        {
            Task<UInt32> storeAsyncTask;

            // Load the text from the sendText input text box to the dataWriter object
            dataWriteObject.WriteBytes(mas);

            // Launch an async task to complete the write operation
            storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

            await storeAsyncTask;
        }
    }
}
