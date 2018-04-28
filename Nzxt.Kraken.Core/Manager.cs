using System;
using System.Threading;

namespace Nzxt.Kraken.Core
{
    public class Manager
    {
        public static byte DEVICE_ACK = 15;

        public static byte DEVICE_NUMBER = 10;

        public static readonly byte[] LightingChannels = new byte[]
        {
            1,
            2
        };

        private Manager()
        {
            this.Data = new byte[64];
        }

        public byte[] Data { get; private set; }

        public Manager(Device device) : this()
        {
            this.Device = device;
        }

        public Device Device { get; private set; }

        public byte Id { get; private set; }

        public void Start()
        {
            var data = new byte[2]
            {
                1, //Unknown
                89 //Unknown
            };
            if (!this.Device.Write(data, false))
            {
                throw ManagerException.Initialize;
            }
            Thread.Sleep(1000); //Unknown
            if (!this.Read(true))
            {
                //This happens after power cycle. 
                //I have no idea why.
                //Seems you can ignore it?
            }
            this.GetOrAddDeviceId();
        }

        protected virtual void GetOrAddDeviceId()
        {
            this.Id = this.Data[DEVICE_NUMBER]; //Device number.
            if (this.Data[0] == 4 && this.Data[DEVICE_ACK] != 31) //Unknown
            {
                if (this.Id == 0 || this.Id == byte.MaxValue)
                {
                    //Perhaps this happens for new devices?
                    //I've never been here.
                    this.AddDeviceId();
                    //We might need to restart the driver here.
                }
            }
            else
            {
                //I've never been here either.
                this.ConfirmDeviceId();
            }
        }

        protected virtual void AddDeviceId()
        {
            var data = new byte[65];
            data[0] = 2; //Unknown
            data[1] = 70; //Unknown
            data[2] = this.CreateDeviceId();
            if (!this.Device.Write(data, false))
            {
                throw ManagerException.Identify;
            }
        }

        protected virtual byte CreateDeviceId()
        {
            //I don't know what the fuck this is all about.
            //while (!success) { randomize(); }
            return (byte)new Random(
                unchecked((int)DateTime.Now.Ticks)
            ).Next(1, byte.MaxValue);
        }

        protected virtual void ConfirmDeviceId()
        {
            var data = new byte[65];
            data[0] = 16; //Unknown
            data[1] = 1; //Unknown
            if (!this.Device.Write(data, false))
            {
                //Is this an error? It appears to be ignored.
                return;
            }
            if (!this.Device.Read(this.Data, false))
            {
                throw ManagerException.Identify;
            }
            if (this.Data[4] == 2)  //Unknown
            {
                //Some reference to "IsFWBoots = True".
                //I don't know what it means but the value does not seem to be used.
                //It may be something to do with firmware updates.
            }
            if (!this.Device.Read(this.Data, false))
            {
                throw ManagerException.Identify;
            }
            if (this.Id != this.Data[DEVICE_NUMBER])
            {
                throw ManagerException.Identify;
            }
        }

        public bool Read(bool required)
        {
            if (required)
            {
                for (var a = 0; a <= 10; a++)
                {
                    if (this.Read(false) && this.Data[DEVICE_ACK] != 0)
                    {
                        return true;
                    }
                    Thread.Sleep(5);
                }
                return false;
            }
            else
            {
                return this.Device.Read(this.Data, false);
            }
        }

        public int Temperature
        {
            get
            {
                return this.Data[1] + this.Data[2];
            }
        }

        public int FanSpeed
        {
            get
            {
                return (this.Data[3] * 256) + this.Data[4];
            }
        }

        public int PumpSpeed
        {
            get
            {
                return (this.Data[5] * 256) + this.Data[6];
            }
        }

        public void SetLightingColor(byte red, byte green, byte blue)
        {
            var data = new byte[65];
            data[0] = 2; //Unknown
            data[1] = 76; //Unknown
            data[4] = 2; //Speed
            data[6] = red; //Red 1
            data[5] = green; //Green 1
            data[7] = blue;  //Blue 1
            for (var a = 0; a < 8; a++)
            {
                data[8 + (a * 3)] = red; //Red 2 + a
                data[9 + (a * 3)] = green; //Green 2 + a
                data[10 + (a * 3)] = blue; //Blue 2 + a
            }
            foreach (var channel in LightingChannels)
            {
                data[2] = channel; //Channel
                this.Device.Write(data, false);
            }
        }

        public void SetFanSpeed(byte percent)
        {
            var data = new byte[65];
            data[0] = 2; //Unknown
            data[1] = 77; //Unknown
            data[2] = 0; //0 = Fan or 64 = pump + "index".
            data[4] = percent; //Fan/Pump percent. 
            this.Device.Write(data, false);
            Thread.Sleep(1000); //Unknown
        }

        public void SetPumpSpeed(byte percent)
        {
            var data = new byte[65];
            data[0] = 2; //Unknown
            data[1] = 77; //Unknown
            data[2] = 64; //0 = Fan or 64 = pump + "index".
            data[4] = percent; //Fan/Pump percent. 
            this.Device.Write(data, false);
            Thread.Sleep(1000); //Unknown
        }
    }

    public class ManagerException : Exception
    {
        public ManagerException(string message) : base(message)
        {

        }

        public static Exception Initialize
        {
            get
            {
                return new ManagerException("Failed to initialize device.");
            }
        }

        public static Exception Identify
        {
            get
            {
                return new ManagerException("Failed to determine or create device identifier.");
            }
        }
    }
}