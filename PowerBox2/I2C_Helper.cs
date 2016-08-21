using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace PowerBox2
{
    public class I2C_Helper
    {
        private string AQS;
        private DeviceInformationCollection DIS;

        public enum Mode : byte
        {
            // Retrieves sensor data from specified I2C slave Arduino
            Mode0 = 0,

            // Retrieves devices state from specified I2C slave Arduino
            Mode1 = 1,

            // Sends IO signal to pin of specified I2C slave Arduino
            Mode2 = 2,

            Mode3 = 3
        }

        // Sends control signal to the specific Arduino and retrieves response bytes.
        public async Task<byte[]> WriteRead(int I2C_Slave_Address, Mode ControlMode, byte Pin = 0, byte PinValue = 0)
        {
            // Create response byte array of fourteen
            byte[] Response = new byte[7];

            try
            {
                // Initialize I2C
                var Settings = new I2cConnectionSettings(I2C_Slave_Address);
                Settings.BusSpeed = I2cBusSpeed.StandardMode;

                if (AQS == null || DIS == null)
                {
                    AQS = I2cDevice.GetDeviceSelector("I2C1");
                    DIS = await DeviceInformation.FindAllAsync(AQS);
                }

                using (I2cDevice Device = await I2cDevice.FromIdAsync(DIS[0].Id, Settings))
                {
                    Device.Write(new byte[] { byte.Parse(ControlMode.ToString().Replace("Mode", "")), Pin, PinValue });
                    
                    Device.Read(Response);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Response;
        }

    }
}
