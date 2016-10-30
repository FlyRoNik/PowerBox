using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBox2
{
    class Box
    {
        private const int NUMBER_BOX = 8;
        private const int PIN_BLINK = 23;
        private const int PIN_RESET = 24;

        public FingerPrintScaner scaner;
        public MCU[] mcu = new MCU[NUMBER_BOX];
        public Camera cam;

        public int numberCell;

        public Box()
        {
            Debag.createdirectory();
            Debag.createDirectorySD();

            try
            {
                Task thread = new Task(() =>
                {
                    scaner = new FingerPrintScaner(PIN_BLINK, PIN_RESET);
                });
                thread.Start();
                thread.Wait();
                scaner.setTimeRespon();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Scanner does not respond")
                {
                    try
                    {
                        scaner.setReset(FingerPrintScaner.Value.OFF);
                        scaner.setReset(FingerPrintScaner.Value.ON);
                        scaner.setTimeRespon();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scaner: " + e.Message);
                    }
                }
                else
                {
                    scaner.CloseFingerPrintScaner();
                    throw new Exception("Scaner: " + ex.Message);
                }
            }

            try
            {
                for (int i = 0; i < NUMBER_BOX; i++)
                {
                    mcu[i] = new MCU(i);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("MCU: " + ex.Message);
            }

            try
            {
                cam = new Camera();
            }
            catch (Exception ex)
            {
                throw new Exception("Camera: " + ex.Message);
            }
        }

    }
}
