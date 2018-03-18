using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Nzxt.Kraken.Core
{
    public class HidDevice
    {
        public static uint MAX_DEVICES = 64;

        static HidDevice()
        {
            HidPlatform.HidD_GetHidGuid(out HID);
        }

        private static Guid HID;

        public HidDevice(ushort vender, ushort product, ushort version, string serial, string devicePath)
        {
            this.Vender = vender;
            this.Product = product;
            this.Version = version;
            this.Serial = serial;
            this.DevicePath = devicePath;
        }

        public ushort Vender { get; set; }

        public ushort Product { get; set; }

        public ushort Version { get; set; }

        public string Serial { get; set; }

        public string DevicePath { get; set; }

        private static IEnumerable<string> DevicePaths
        {
            get
            {
                var devices = SetupDiGetClassDevs();
                var deviceInterface = new HidPlatform.SP_DEVICE_INTERFACE_DATA();
                deviceInterface.Size = Marshal.SizeOf(deviceInterface);
                for (var a = 0u; a < MAX_DEVICES; a++)
                {
                    if (!HidPlatform.SetupDiEnumDeviceInterfaces(devices, IntPtr.Zero, ref HID, a, ref deviceInterface))
                    {
                        continue;
                    }
                    uint size = SetupDiGetDeviceInterfaceDetail(devices, ref deviceInterface);
                    var devicePath = default(string);
                    if (GetDevicePath(devices, deviceInterface, size, out devicePath))
                    {
                        yield return devicePath;
                    }
                }
            }
        }

        private static uint SetupDiGetDeviceInterfaceDetail(IntPtr devices, ref HidPlatform.SP_DEVICE_INTERFACE_DATA deviceInterface)
        {
            var size = default(uint);
            HidPlatform.SetupDiGetDeviceInterfaceDetail(
                devices,
                ref deviceInterface,
                IntPtr.Zero,
                0,
                out size,
                IntPtr.Zero
            );
            return size;
        }

        private static bool SetupDiGetDeviceInterfaceDetail(IntPtr devices, ref HidPlatform.SP_DEVICE_INTERFACE_DATA deviceInterface, uint size, IntPtr data)
        {
            return HidPlatform.SetupDiGetDeviceInterfaceDetail(
                devices,
                ref deviceInterface,
                data,
                size,
                out size,
                IntPtr.Zero
            );
        }

        private static IntPtr SetupDiGetClassDevs()
        {
            return HidPlatform.SetupDiGetClassDevs(
                ref HID,
                0,
                IntPtr.Zero,
                HidPlatform.DIGCF_PRESENT | HidPlatform.DIGCF_DEVICEINTERFACE
            );
        }

        private static bool GetDevicePath(IntPtr devices, HidPlatform.SP_DEVICE_INTERFACE_DATA deviceInterface, uint size, out string devicePath)
        {
            var data = Marshal.AllocHGlobal((int)size);
            try
            {
                var deviceDetail = new HidPlatform.SP_DEVICE_INTERFACE_DETAIL_DATA();
                deviceDetail.Size = Marshal.SizeOf(deviceDetail);
                Marshal.StructureToPtr(deviceDetail, data, false);
                if (SetupDiGetDeviceInterfaceDetail(devices, ref deviceInterface, size, data))
                {
                    devicePath = Marshal.PtrToStringAuto(data + 4);
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(data);
            }
            devicePath = default(string);
            return false;
        }

        public static IEnumerable<HidDevice> Devices
        {
            get
            {
                var devices = new List<HidDevice>();
                foreach (var devicePath in DevicePaths)
                {
                    var file = HidPlatform.CreateFile(
                        devicePath,
                        HidPlatform.FILE_ACCESS_READ | HidPlatform.FILE_ACCESS_WRITE,
                        HidPlatform.FILE_SHARE_READ | HidPlatform.FILE_SHARE_WRITE,
                        0,
                        HidPlatform.FILE_CREATION_DISPOSITON_OPEN_EXISTING,
                        0,
                        0
                    );
                    var attributes = new HidPlatform.HIDD_ATTRIBUTES();
                    attributes.Size = Marshal.SizeOf(attributes);
                    if (HidPlatform.HidD_GetAttributes(file, out attributes))
                    {
                        var buffer = new StringBuilder(128);
                        if (HidPlatform.HidD_GetSerialNumberString(file, buffer, buffer.Capacity))
                        {
                            var device = new HidDevice(
                                attributes.VendorID,
                                attributes.ProductID,
                                attributes.VersionNumber,
                                buffer.ToString(),
                                devicePath
                            );
                            devices.Add(device);
                        }
                    }
                    HidPlatform.CloseHandle(file);
                }
                return devices;
            }
        }
    }
}