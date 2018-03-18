using System;

namespace Nzxt.Kraken.Core
{
    public class Device : IDisposable
    {
        public const ushort VENDOR = 7793;

        public const ushort PRODUCT = 5902;

        public Device(ushort vendor = VENDOR, ushort product = PRODUCT)
        {
            this.Vendor = vendor;
            this.Product = product;
            this.Serial = GetSerial(vendor, product);
            this.Open();
        }

        public ushort Vendor { get; private set; }

        public ushort Product { get; private set; }

        public string Serial { get; private set; }

        public HidDriver Driver { get; private set; }

        protected virtual void Open()
        {
            this.Driver = new HidDriver(this.Vendor, this.Product, this.Serial);
        }

        public bool Read(byte[] data, bool input)
        {
            return this.Driver.Read(ref data, input);
        }

        public bool Write(byte[] data, bool input)
        {
            return this.Driver.Write(data, input);
        }

        private static string GetSerial(ushort vendor, ushort product)
        {
            foreach (var device in HidDevice.Devices)
            {
                if (device.Vender != vendor)
                {
                    continue;
                }
                if (device.Product != product)
                {
                    continue;
                }
                return device.Serial;
            }
            throw new InvalidOperationException(string.Format("Device not found: \"{0}\":\"{1}\".", vendor, product));
        }

        public void Dispose()
        {
            this.Driver.Close();
        }
    }
}
