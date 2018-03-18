using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Nzxt.Kraken.Core
{
    public class HidDriver : IDisposable
    {
        public HidDriver(ushort vid, ushort pid, string serial)
        {
            this.OpenDevice(vid, pid, serial);
        }

        private IntPtr ReadHandle = IntPtr.Zero;

        private IntPtr WriteHandle = IntPtr.Zero;

        private void OpenDevice(ushort vendor, ushort product, string serial)
        {
            this.OpenDevice(this.GetDevicePath(vendor, product, serial));
        }

        private void OpenDevice(string devicePath)
        {
            this.ReadHandle = HidPlatform.CreateFile(
                devicePath,
                HidPlatform.FILE_ACCESS_READ,
                HidPlatform.FILE_SHARE_READ | HidPlatform.FILE_SHARE_WRITE,
                0,
                HidPlatform.FILE_CREATION_DISPOSITON_OPEN_EXISTING,
                0,
                0
            );
            this.WriteHandle = HidPlatform.CreateFile(
                devicePath,
                HidPlatform.FILE_ACCESS_WRITE,
                HidPlatform.FILE_SHARE_READ | HidPlatform.FILE_SHARE_WRITE,
                0,
                HidPlatform.FILE_CREATION_DISPOSITON_OPEN_EXISTING,
                0,
                0
            );
        }

        public string GetDevicePath(ushort vendor, ushort product, string serial)
        {
            foreach (var device in HidDevice.Devices)
            {
                if (device.Vender == vendor && device.Product == product && device.Serial == serial)
                {
                    return device.DevicePath;
                }
            }
            throw new InvalidOperationException(string.Format("Could not determine device path: \"{0}\":\"{1}\":\"{2}\".", vendor, product, serial));
        }

        public bool Write(byte[] data, bool input = false)
        {
            if (input)
            {
                return HidPlatform.HidD_SetOutputReport(this.ReadHandle, data, (uint)data.Length);
            }
            else
            {
                var count = default(uint);
                return HidPlatform.WriteFile(this.WriteHandle, data, (uint)data.Length, ref count, IntPtr.Zero);
            }
        }

        public bool Read(ref byte[] data)
        {
            var count = default(uint);
            data[0] = 0;
            return HidPlatform.ReadFile(this.ReadHandle, data, (uint)data.Length, ref count, IntPtr.Zero);
        }

        public void Close()
        {
            if (this.ReadHandle != IntPtr.Zero && HidPlatform.CloseHandle(this.ReadHandle) == 1)
            {
                this.ReadHandle = IntPtr.Zero;
            }
            if (this.WriteHandle != IntPtr.Zero && HidPlatform.CloseHandle(this.WriteHandle) == 1)
            {
                this.WriteHandle = IntPtr.Zero;
            }
        }

        public bool Read(ref byte[] data, bool input = false)
        {
            data[0] = 0;
            if (input)
            {
                return HidPlatform.HidD_GetInputReport(this.ReadHandle, data, (uint)data.Length);
            }
            else
            {
                var count = default(uint);
                return HidPlatform.ReadFile(this.ReadHandle, data, (uint)data.Length, ref count, IntPtr.Zero);
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            try
            {
                this.OnDisposing();
            }
            catch
            {
                //Will not raise exceptions on the GC thread.
            }
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Close();
        }

        ~HidDriver()
        {
            this.Dispose(true);
        }
    }
}
