using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace InMemCached
{
    internal class HeapHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public HeapHandle()
            : base(true)
        {
            SetHandle(NativeMethods.HeapCreate(0, UIntPtr.Zero, UIntPtr.Zero));

            uint HeapInformation = NativeMethods.HEAP_LFH;
            NativeMethods.HeapSetInformation(this, NativeMethods.HeapCompatibilityInformation, ref HeapInformation, (UIntPtr)4);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.HeapDestroy(handle);
        }
    }
}
