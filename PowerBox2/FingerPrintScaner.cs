using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PowerBox2
{
    class FingerPrintScaner
    {
        private const int TIME_RESPON = 1500;

        private SerialPort_Helper serialPort;

        private GpioPin pinBlink;
        private GpioPinValue blinkValue;

        private GpioPin pinReset;
        private GpioPinValue resetValue;

        private bool respon;
        private bool error;

        private byte[] bytesRead;

        private int timeRespon; // get only using function - getTimeRespon()

        private MySemaphore _pool = new MySemaphore(1, 2);
        private MySemaphore _pool2 = new MySemaphore(0, 1);
        private MySemaphore _pool3 = new MySemaphore(0, 1);

        public class User
        {
            private int ID;
            private Privilege privilege;

            public User(int ID, Privilege privilege)
            {
                this.ID = ID;
                this.privilege = privilege;
            }

            public int getID()
            {
                return ID;
            }

            public Privilege getPrivilege()
            {
                return privilege;
            }

            public override string ToString()
            {
                return ID.ToString() + ": " + privilege.ToString();
            }
        }

        private class UserID
        {
            private int ID;

            private byte lowID;
            private byte highID;

            private bool HexOrDec;

            public UserID(int ID, bool type)
            {
                this.ID = ID;

                HexOrDec = type;
                if (HexOrDec)
                {
                    string hexValue = "00" + ID.ToString("X");
                    lowID = byte.Parse(hexValue.Substring(hexValue.Length - 2), System.Globalization.NumberStyles.HexNumber);
                    highID = byte.Parse(hexValue.Substring(0, hexValue.Length - 2), System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    lowID = (byte)(ID % 256);
                    highID = (byte)(ID / 256);
                }
            }

            public UserID(byte highID, byte lowID, bool type)
            {
                this.lowID = lowID;
                this.highID = highID;

                HexOrDec = type;
                if (HexOrDec)
                {
                    string hexValue = highID.ToString() + lowID.ToString();
                    ID = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    ID = highID * 256 + lowID;
                }
            }

            public byte getLowID()
            {
                return lowID;
            }

            public byte getHighID()
            {
                return highID;
            }

            public bool getHexOrDec()
            {
                return HexOrDec;
            }

            public int getID()
            {
                return ID;
            }
        }

        public enum Value : byte
        {
            ON = 0,
            OFF = 1
        }

        public enum Privilege : byte
        {
            USER = 1,
            ADMIN = 2,
            VIP = 3
        }

        public enum Times : byte
        {
            First = 1,
            Second = 2,
            Third = 3
        }

        public enum ValuesOperating : byte
        {
            ACK_SUCCESS = 0x00, //Operation successfully 
            ACK_FAIL = 0x01, //Operation failed 
            ACK_FULL = 0x04, //Fingerprint database is full 
            ACK_NOUSER = 0x05, //No such user
            ACK_USER_EXIST = 0x06, //User already exists
            ACK_FIN_EXIST = 0x07, // Fingerprint already exists 
            ACK_TIMEOUT = 0x08 //Acquisition timeout 
        }

        public FingerPrintScaner(int BLINK, int RESET)
        {
            respon = false;
            error = false;

            serialPort = new SerialPort_Helper(delegteReadAsync, exception);

            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                throw new Exception("There is no GPIO controller on this device.");
            }

            pinBlink = gpio.OpenPin(BLINK);
            resetValue = GpioPinValue.High;
            pinBlink.Write(resetValue);
            pinBlink.SetDriveMode(GpioPinDriveMode.Output);

            pinReset = gpio.OpenPin(RESET);
            blinkValue = GpioPinValue.High;
            pinReset.Write(blinkValue);
            pinReset.SetDriveMode(GpioPinDriveMode.Output);     
        }

        public void CloseFingerPrintScaner()
        {
            if (serialPort != null)
            {
                serialPort.CloseDevice();
                serialPort = null;
            }

            if (pinBlink != null)
            {
                pinBlink.Dispose();
            }
                pinBlink = null;

            if (pinReset != null)
            {
                pinReset.Dispose();
            }
                pinReset = null;
        }

        private void send(byte[] comand)
        {
            byte checksum = 0;

            for (int i = 1; i <= 5; i++)
            {
                checksum ^= comand[i];
            }
            comand[6] = checksum;
            Debag.Write("FPS210");
            serialPort.Write(comand);
        }

        private void delegteReadAsync(byte[] byteArray)
        {
            _pool2.Wait();
            _pool.Wait();
            bytesRead = byteArray;
            respon = true;
            _pool.TryRelease();
        }

        private void exception(Exception exception) //обработать вызваное исключение
        {
            _pool.Wait();
            serialPort.connection();
            _pool.TryRelease();
        }

        private void waitResponse(int time)
        {
            Task thread = new Task(() => {
                while (!respon)
                {
                }
            });
            thread.Start();
            _pool2.TryRelease();
            if (time == 0)
            {
                thread.Wait();
            }
            else
            {
                thread.Wait(time);
            }

            _pool.Wait();
            _pool.TryRelease();

            if (!respon)
            {
                respon = false;
                throw new Exception("Scanner does not respond");
            }
            else
            {
                if (error)
                {
                    _pool3.TryRelease();
                    error = false;
                }
                respon = false;
            }
        }

        private string response()
        {
            switch ((ValuesOperating)bytesRead[4])
            {
                case ValuesOperating.ACK_SUCCESS: return "Operation successfully";
                case ValuesOperating.ACK_FAIL: return "Operation failed";
                case ValuesOperating.ACK_FULL: return "Fingerprint database is full";
                case ValuesOperating.ACK_NOUSER: return "No such user";
                case ValuesOperating.ACK_USER_EXIST: return "User already exists";
                case ValuesOperating.ACK_FIN_EXIST: return "Fingerprint already exists";
                case ValuesOperating.ACK_TIMEOUT: return "Acquisition timeout";

                default: return "";
            }
        }

        private int getTimeRespon()
        {
            if (timeRespon != 0)
            {
                return timeRespon;
            }
            else
            {
                return -TIME_RESPON;
            }
        }

        public void setTimeRespon()
        {
            timeRespon = getTimeoutValue() * 1000;
        }

        public void setReset(Value value)
        {
            setPin(pinReset, ref resetValue, value);
        }

        public void setBlink(Value value)
        {
            setPin(pinBlink, ref blinkValue, value);
        }

        public Value getReset()
        {
            return getPin(resetValue);
        }

        public Value getBlink()
        {
            return getPin(blinkValue);
        }

        private Value getPin(GpioPinValue pinValue)
        {
            if (pinValue == GpioPinValue.Low)
            {
                return Value.OFF;
            }
            else
            {
                return Value.ON;
            }
        }

        private void setPin(GpioPin pin, ref GpioPinValue pinValue, Value value)
        {
            if (value == Value.ON)
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
            }
            else
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
            }
        }

        public string sleep()
        {
            send(new byte[] { 0xF5, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);
            return response();
        }

        public string addFingerPrint(int ID, Privilege privilege, Times times)
        {
            UserID userId = new UserID(ID, true);
            Debag.Write("FPS359");
            send(new byte[] { 0xF5, (byte)times, userId.getHighID(), userId.getLowID(), (byte)privilege, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON + getTimeRespon());
            Debag.Write("FPS363");
            return response();
        }

        public string setRepeatMode(Value value)
        {
            send(new byte[] { 0xF5, 0x2D, 0x00, (byte)value, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return response();
        }

        public Value getRepeatMode()
        {
            send(new byte[] { 0xF5, 0x2D, 0x00, 0x00, 0x01, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return (Value)bytesRead[3];
        }

        public string deleteUser(int ID)
        {
            UserID userID = new UserID(ID, true);

            send(new byte[] { 0xF5, 0x04, userID.getHighID(), userID.getLowID(), 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return response();
        }

        public string deleteAllUser()  
        {
            send(new byte[] { 0xF5, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return response();
        }

        public int getNumberOfUsers() 
        {
            send(new byte[] { 0xF5, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            UserID userID = new UserID(bytesRead[2], bytesRead[3], false);
            return userID.getID();
        }

        public string compareOneToOne(int ID)
        {
            UserID userID = new UserID(ID, true);
            send(new byte[] { 0xF5, 0x0B, userID.getHighID(), userID.getLowID(), 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON + getTimeRespon());
            return response();
        }

        public User compareOneToMore()
        {
            send(new byte[] { 0xF5, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON + getTimeRespon());
            if (bytesRead[4] > 0x03)
            {
                throw new Exception(response());
            }
            else
            {
                UserID userId = new UserID(bytesRead[2], bytesRead[3], false);
                return new User(userId.getID(), (Privilege)bytesRead[4]);
            }
        }

        public Privilege getUserPrivilege(int ID)
        {
            UserID userID = new UserID(ID, true);
            send(new byte[] { 0xF5, 0x0A, userID.getHighID(), userID.getLowID(), 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);
            if (bytesRead[4] > 0x03)
            {
                throw new Exception(response());
            }
            else
            {
                return (Privilege)bytesRead[4];
            }
        }

        public string setComparisonLevel(int level)
        {
            send(new byte[] { 0xF5, 0x28, 0x00, (byte)level, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return response();
        }

        public void genExcept()
        {
            error = true;
            Debag.Write("FPS466");
            send(new byte[] { 0xF5, 0x28, 0x00, 0x10, 0x00, 0x00, 0x00, 0xF5 });
            Debag.Write("FPS468");
            _pool3.Wait();
        }

        public int getComparisonLevel()
        {
            send(new byte[] { 0xF5, 0x28, 0x00, 0x00, 0x01, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return bytesRead[3];
        }

        public User[] getUserNumbersAndPrivilege()
        {
            Debag.Write("FPS485");
            send(new byte[] { 0xF5, 0x2B, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF5 });
            Debag.Write("FPS487");
            waitResponse(TIME_RESPON);
            Debag.Write("FPS489");
            if (bytesRead[4] == 1)
            {
                throw new Exception(response());
            }
            else
            {
                User[] users = new User[bytesRead[10]];

                int disp = 11;
                for (int i = 0; i < users.Length; i++)
                {
                    UserID userId = new UserID(bytesRead[disp], bytesRead[disp + 1], false);
                    users[i] = new User(userId.getID(), (Privilege)bytesRead[disp + 2]);
                    disp += 3;
                }
                return users;
            }
        }

        public string setTimeoutValue(int value)
        {
            send(new byte[] { 0xF5, 0x2E, 0x00, (byte)value, 0x00, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            if (bytesRead[4] == 0x00)
            {
                timeRespon = value * 1000;
            }

            return response();
        }

        public int getTimeoutValue()
        {
            send(new byte[] { 0xF5, 0x2E, 0x00, 0x00, 0x01, 0x00, 0x00, 0xF5 });

            waitResponse(TIME_RESPON);

            return bytesRead[3];
        }
    }
}
