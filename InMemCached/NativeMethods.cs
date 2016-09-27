using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace InMemCached
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapCreate(uint flOptions, UIntPtr dwInitialSize, UIntPtr dwMaximumSize);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern bool HeapSetInformation(HeapHandle HeapHandle, uint HeapInformationClass, ref uint HeapInformation, UIntPtr HeapInformationLength);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern IntPtr HeapAlloc(HeapHandle hHeap, uint dwFlags, IntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(HeapHandle hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapDestroy(IntPtr hHeap);

        public const uint HeapCompatibilityInformation = 2;
        public const uint HEAP_LFH = 2;
    }
}
