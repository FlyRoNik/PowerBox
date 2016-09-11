using System;
using System.Threading.Tasks;

namespace PowerBox2
{
    class MCU
    {
        private byte[] bytesRead;

        private int I2C_Slave_Address;

        private I2C_Helper i2c = new I2C_Helper();

        private bool respon;

        private Task exception;

        public MCU(int I2C_Slave_Address)
        {
            respon = false;

            this.I2C_Slave_Address = I2C_Slave_Address;
        }

        private async Task WriteRead(I2C_Helper.Mode mode, byte Pin = 0, byte PinValue = 0)
        {
            try
            {
                bytesRead = await i2c.WriteRead(I2C_Slave_Address, mode, Pin, PinValue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                respon = true;
            }
        }

        private void waitResponse(int time)
        {
            Task thread = new Task(() => {
                while (!respon) { }
            });
            thread.Start();
            thread.Wait(time);
            if (!respon)
            {
                throw new Exception("Box does not respond");
            }
            else
            {
                respon = false;
                if (!respon && exception.IsFaulted)
                {
                    exception = null;
                    throw new Exception("MCU with that address does not exist or the connection is lost with him");
                }
            }
        }

        public byte[] getDataMicrocontroller()
        {
            Task thread = new Task(() => {
               exception = WriteRead(I2C_Helper.Mode.Mode0);
            });
            thread.Start();
            waitResponse(10000);
            
            byte[] mas = new byte[bytesRead.Length];
            mas = bytesRead;
            return mas;
        }

        public byte[] getStatePin()
        {
            Task thread = new Task(() => {
                exception = WriteRead(I2C_Helper.Mode.Mode1);
            });
            thread.Start();
            waitResponse(10000);

            byte[] mas = new byte[bytesRead.Length];
            mas = bytesRead;
            return mas;
        }

        public void setPinDigital(byte pin, byte val)
        {
            Task thread = new Task(() => {
                exception = WriteRead(I2C_Helper.Mode.Mode2, pin, val);
            });
            thread.Start();
            waitResponse(10000);
        }

        public void setPinAnalog(byte pin, byte val)
        {
            Task thread = new Task(() => {
                exception = WriteRead(I2C_Helper.Mode.Mode3, pin, val);
            });
            thread.Start();
            waitResponse(10000);
        }
    }
}
