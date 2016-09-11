using System;
using System.Collections.Generic;
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
            try
            {
                Task thread = new Task(() =>
                {
                    scaner = new FingerPrintScaner(PIN_BLINK, PIN_RESET);
                });
                thread.Start();
                Task.Delay(-1).Wait(2000);
                scaner.setTimeRespon();

                for (int i = 0; i < NUMBER_BOX; i++)
                {
                    mcu[i] = new MCU(i);
                }

                cam = new Camera();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
    }
}
