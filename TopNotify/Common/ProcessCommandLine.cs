//This File Is From https://github.com/sonicmouse/ProcCmdLine/blob/master/ManagedProcessCommandLine/ProcessCommandLine.cs
//Huge Thanks To sonicmouse For Providing This Useful File

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace TopNotify.Common
{
    public static class ProcessCommandLine
    {
        private static class Win32Native
        {
            public const uint PROCESS_BASIC_INFORMATION = 0;

            [Flags]
            public enum OpenProcessDesiredAccess : uint
            {
                PROCESS_VM_READ = 0x0010,
                PROCESS_QUERY_INFORMATION = 0x0400,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ProcessBasicInformation
            {
                public IntPtr Reserved1;
                public IntPtr PebBaseAddress;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public IntPtr[] Reserved2;
                public IntPtr UniqueProcessId;
                public IntPtr Reserved3;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct UnicodeString
            {
                public ushort Length;
                public ushort MaximumLength;
                public IntPtr Buffer;
            }

            // This is not the real struct!
            // I faked it to get ProcessParameters address.
            // Actual struct definition:
            // https://docs.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-peb
            [StructLayout(LayoutKind.Sequential)]
            public struct Peb
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public IntPtr[] Reserved;
                public IntPtr ProcessParameters;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RtlUserProcessParameters
            {
                public uint MaximumLength;
                public uint Length;
                public uint Flags;
                public uint DebugFlags;
                public IntPtr ConsoleHandle;
                public uint ConsoleFlags;
                public IntPtr StandardInput;
                public IntPtr StandardOutput;
                public IntPtr StandardError;
                public UnicodeString CurrentDirectory;
                public IntPtr CurrentDirectoryHandle;
                public UnicodeString DllPath;
                public UnicodeString ImagePathName;
                public UnicodeString CommandLine;
            }

            [DllImport("ntdll.dll")]
            public static extern uint NtQueryInformationProcess(
                IntPtr ProcessHandle,
                uint ProcessInformationClass,
                IntPtr ProcessInformation,
                uint ProcessInformationLength,
                out uint ReturnLength);

            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(
                OpenProcessDesiredAccess dwDesiredAccess,
                [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
                uint dwProcessId);

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ReadProcessMemory(
                IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer,
                uint nSize, out uint lpNumberOfBytesRead);

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);

            [DllImport("shell32.dll", SetLastError = true,
                CharSet = CharSet.Unicode, EntryPoint = "CommandLineToArgvW")]
            public static extern IntPtr CommandLineToArgv(string lpCmdLine, out int pNumArgs);
        }

        private static bool ReadStructFromProcessMemory<TStruct>(
            IntPtr hProcess, IntPtr lpBaseAddress, out TStruct val)
        {
            val = default;
            var structSize = Marshal.SizeOf<TStruct>();
            var mem = Marshal.AllocHGlobal(structSize);
            try
            {
                if (Win32Native.ReadProcessMemory(
                    hProcess, lpBaseAddress, mem, (uint)structSize, out var len) &&
                    (len == structSize))
                {
                    val = Marshal.PtrToStructure<TStruct>(mem);
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }
            return false;
        }

        public static string ErrorToString(int error) =>
            new string[]
            {
            "Success",
            "Failed to open process for reading",
            "Failed to query process information",
            "PEB address was null",
            "Failed to read PEB information",
            "Failed to read process parameters",
            "Failed to read parameter from process"
            }[Math.Abs(error)];

        public enum Parameter
        {
            CommandLine,
            WorkingDirectory,
        }

        public static int Retrieve(Process process, out string parameterValue, Parameter parameter = Parameter.CommandLine)
        {
            parameterValue = null;

            var hProcess = Win32Native.OpenProcess(
                Win32Native.OpenProcessDesiredAccess.PROCESS_QUERY_INFORMATION |
                Win32Native.OpenProcessDesiredAccess.PROCESS_VM_READ, false, (uint)process.Id);

            if (hProcess == IntPtr.Zero)
            {
                return -1; // couldn't open process for VM read
            }

            try
            {
                return RetrieveFromProcess(hProcess, out parameterValue, parameter);
            }
            finally
            {
                Win32Native.CloseHandle(hProcess);
            }
        }

        private static int RetrieveFromProcess(IntPtr hProcess, out string parameterValue, Parameter parameter)
        {
            parameterValue = null;

            var sizePBI = Marshal.SizeOf<Win32Native.ProcessBasicInformation>();
            var memPBI = Marshal.AllocHGlobal(sizePBI);
            try
            {
                var ret = Win32Native.NtQueryInformationProcess(
                    hProcess, Win32Native.PROCESS_BASIC_INFORMATION, memPBI,
                    (uint)sizePBI, out _);

                if (ret != 0)
                {
                    return -2; // NtQueryInformationProcess failed
                }

                var pbiInfo = Marshal.PtrToStructure<Win32Native.ProcessBasicInformation>(memPBI);
                if (pbiInfo.PebBaseAddress == IntPtr.Zero)
                {
                    return -3; // PebBaseAddress is null
                }

                return RetrieveFromPeb(hProcess, pbiInfo.PebBaseAddress, out parameterValue, parameter);
            }
            finally
            {
                Marshal.FreeHGlobal(memPBI);
            }
        }

        private static int RetrieveFromPeb(IntPtr hProcess, IntPtr pebBaseAddress, out string parameterValue, Parameter parameter)
        {
            parameterValue = null;

            if (!ReadStructFromProcessMemory<Win32Native.Peb>(hProcess, pebBaseAddress, out var pebInfo))
            {
                return -4; // couldn't read PEB information
            }

            if (!ReadStructFromProcessMemory<Win32Native.RtlUserProcessParameters>(
                hProcess, pebInfo.ProcessParameters, out var ruppInfo))
            {
                return -5; // couldn't read ProcessParameters
            }

            var unicodeString = parameter switch
            {
                Parameter.CommandLine => ruppInfo.CommandLine,
                Parameter.WorkingDirectory => ruppInfo.CurrentDirectory,
                _ => ruppInfo.CommandLine
            };

            return ReadUnicodeString(hProcess, unicodeString, out parameterValue);
        }

        private static int ReadUnicodeString(IntPtr hProcess, Win32Native.UnicodeString unicodeString, out string value)
        {
            value = null;
            var clLen = unicodeString.MaximumLength;
            var memCL = Marshal.AllocHGlobal(clLen);
            try
            {
                if (!Win32Native.ReadProcessMemory(hProcess, unicodeString.Buffer, memCL, clLen, out _))
                {
                    return -6; // couldn't read parameter line buffer
                }

                value = Marshal.PtrToStringUni(memCL);
                return 0; // Success
            }
            finally
            {
                Marshal.FreeHGlobal(memCL);
            }
        }

        public static IReadOnlyList<string> CommandLineToArgs(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine)) { return Array.Empty<string>(); }

            var argv = Win32Native.CommandLineToArgv(commandLine, out var argc);
            if (argv == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; ++i)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }
                return args.ToList().AsReadOnly();
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}

