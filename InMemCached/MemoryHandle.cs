using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace InMemCached
{
    internal class MemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private readonly HeapHandle _heap;
        private readonly int _length;

        public MemoryHandle(HeapHandle heap, byte[] value)
            : base(true)
        {
            if (heap == null)
                throw new ArgumentNullException(nameof(heap));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _heap = heap;
            _length = value.Length;

            SetHandle(NativeMethods.HeapAlloc(heap, 0, (IntPtr)value.Length));

            Marshal.Copy(value, 0, handle, value.Length);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.HeapFree(_heap, 0, handle);
        }

        public byte[] ToByteArray()
        {
            if (IsClosed)
                return null;

            var result = new byte[_length];

            Marshal.Copy(handle, result, 0, _length);

            return result;
        }
    }
}
