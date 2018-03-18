using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Nzxt.Kraken.Core
{
    public class HidDevice
    {
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
                var devices = HidPlatform.SetupDiGetClassDevs(
                    ref HID,
                    0,
                    IntPtr.Zero,
                    HidPlatform.DIGCF.DIGCF_PRESENT | HidPlatform.DIGCF.DIGCF_DEVICEINTERFACE
                );
                var deviceInterface = new HidPlatform.SP_DEVICE_INTERFACE_DATA();
                deviceInterface.cbSize = Marshal.SizeOf(deviceInterface);
                for (uint index = 0; index < 64; index++)
                {
                    if (!HidPlatform.SetupDiEnumDeviceInterfaces(devices, IntPtr.Zero, ref HID, index, ref deviceInterface))
                    {
                        continue;
                    }
                    uint size = 0;
                    HidPlatform.SetupDiGetDeviceInterfaceDetail(
                        devices,
                        ref deviceInterface,
                        IntPtr.Zero,
                        0,
                        out size,
                        IntPtr.Zero
                    );
                    var data = Marshal.AllocHGlobal((int)size);
                    var deviceDetail = new HidPlatform.SP_DEVICE_INTERFACE_DETAIL_DATA();
                    deviceDetail.cbSize = Marshal.SizeOf(deviceDetail);
                    Marshal.StructureToPtr(deviceDetail, data, false);
                    if (HidPlatform.SetupDiGetDeviceInterfaceDetail(devices, ref deviceInterface, data, size, out size, IntPtr.Zero))
                    {
                        var devicePath = Marshal.PtrToStringAuto(data + 4);
                        yield return devicePath;
                    }
                    Marshal.FreeHGlobal(data);
                }
            }
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
                        HidPlatform.FILE_ACCESS_MASK.GENERIC_READ | HidPlatform.FILE_ACCESS_MASK.GENERIC_WRITE,
                        HidPlatform.FILE_SHARE_MODE.FILE_SHARE_READ | HidPlatform.FILE_SHARE_MODE.FILE_SHARE_WRITE,
                        0,
                        HidPlatform.FILE_CREATION_DISPOSITON.OPEN_EXISTING,
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