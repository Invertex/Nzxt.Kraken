using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nzxt.Kraken.Core
{
    public static class HidPlatform
    {
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(string lpFileName, FILE_ACCESS_MASK dwDesiredAccess, FILE_SHARE_MODE dwShareMode, uint lpSecurityAttributes, FILE_CREATION_DISPOSITON dwCreationDisposition, FILE_FLAG_ATTRIBUTES dwFlagsAndAttributes, uint hTemplateFile);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, ref uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CancelIo(IntPtr hFile);

        [DllImport("hid.dll")]
        public static extern void HidD_GetHidGuid(out Guid HidGuid);

        [DllImport("hid.dll")]
        public static extern bool HidD_GetAttributes(IntPtr HidDeviceObject, out HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", CharSet = CharSet.Auto)]
        public static extern bool HidD_GetSerialNumberString(IntPtr HidDeviceObject, StringBuilder Buffer, int BufferLength);

        [DllImport("hid.dll")]
        public static extern bool HidD_FlushQueue(IntPtr HidDeviceObject);

        [DllImport("hid.dll")]
        public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, out IntPtr PreparsedData);

        [DllImport("hid.dll")]
        public static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

        [DllImport("hid.dll")]
        public static extern bool HidD_GetInputReport(IntPtr HidDeviceObject, out IntPtr ReportBuffer, ulong ReportBufferLength);

        [DllImport("hid.dll")]
        public static extern bool HidD_SetOutputReport(IntPtr HidDeviceObject, IntPtr ReportBuffer, ulong ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetCaps(IntPtr pPHIDP_PREPARSED_DATA, ref HIDP_CAPS myPHIDP_CAPS);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetInputReport(IntPtr hFileHandle, byte[] reportBuffer, uint reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_SetOutputReport(IntPtr hFileHandle, byte[] reportBuffer, uint reportBufferLength);

        [DllImport("User32.dll")]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        [DllImport("User32.dll")]
        public static extern bool UnregisterDeviceNotification(IntPtr Handle);

        public enum DIGCF : uint
        {
            DIGCF_DEFAULT = 1,
            DIGCF_PRESENT = 2,
            DIGCF_ALLCLASSES = 4,
            DIGCF_PROFILE = 8,
            DIGCF_DEVICEINTERFACE = 16, // 0x00000010
        }

        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            public int reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            public short devicePath;
        }

        public enum FILE_ACCESS_MASK : uint
        {
            GENERIC_EXECUTE = 536870912, // 0x20000000
            GENERIC_WRITE = 1073741824, // 0x40000000
            GENERIC_READ = 2147483648, // 0x80000000
        }

        public enum FILE_SHARE_MODE : uint
        {
            FILE_SHARE_NONE = 0,
            FILE_SHARE_READ = 1,
            FILE_SHARE_WRITE = 2,
            FILE_SHARE_DELETE = 4,
        }

        public enum FILE_CREATION_DISPOSITON : uint
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5,
        }

        public enum FILE_FLAG_ATTRIBUTES : uint
        {
            FILE_ATTRIBUTE_HIDDEN = 2,
            FILE_ATTRIBUTE_ARCHIVE = 32, // 0x00000020
            FILE_ATTRIBUTE_NORMAL = 128, // 0x00000080
            FILE_ATTRIBUTE_ENCRYPTED = 16384, // 0x00004000
            FILE_FLAG_OVERLAPPED = 1073741824, // 0x40000000
        }

        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        public struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        public enum DBCH_DEVICETYPE : uint
        {
            DBT_DEVTYP_OEM = 0,
            DBT_DEVTYP_VOLUME = 2,
            DBT_DEVTYP_PORT = 3,
            DBT_DEVTYP_DEVICEINTERFACE = 5,
            DBT_DEVTYP_HANDLE = 6,
        }
    }
}
