// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// This is a static class to contain serialization helpers. It is recommended you use whatever
    /// serialization library you're comfortable with. The APIs this works with accept C#'s byte[].
    /// </summary>
    internal static class PrimativeWriter
    {
        public static unsafe int Write<T>(byte* basePtr, int walker, T data) where T : unmanaged
        {
            *(T*)(basePtr + walker) = data;
            return walker + sizeof(T);
        }

        public static unsafe int Write(byte* basePtr, int walker, byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                *(basePtr + i + walker) = data[i];
            }

            return walker + data.Length;
        }

        public static unsafe int Write(byte* basePtr, int walker, Vector3 data)
        {
            walker = Write<float>(basePtr, walker, data.x);
            walker = Write<float>(basePtr, walker, data.y);
            walker = Write<float>(basePtr, walker, data.z);

            return walker;
        }

        public static unsafe int Write(byte* basePtr, int walker, Quaternion data)
        {
            walker = Write<float>(basePtr, walker, data.x);
            walker = Write<float>(basePtr, walker, data.y);
            walker = Write<float>(basePtr, walker, data.z);
            walker = Write<float>(basePtr, walker, data.w);

            return walker;
        }
    }

    public static class PrimativeReader
    {
        public static unsafe int Read<T>(byte* basePtr, int walker, out T data) where T : unmanaged
        {
            data = *(T*)(basePtr + walker);
            return walker + sizeof(T);
        }

        public static unsafe int Read(byte* basePtr, int walker, out byte[] data, int length)
        {
            data = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                data[i] = *(basePtr + i + walker);
            }

            return walker + length;
        }

        public static unsafe int Read(byte* basePtr, int walker, out Vector3 data)
        {
            walker = Read<float>(basePtr, walker, out data.x);
            walker = Read<float>(basePtr, walker, out data.y);
            walker = Read<float>(basePtr, walker, out data.z);

            return walker;
        }

        public static unsafe int Read(byte* basePtr, int walker, Quaternion data)
        {
            walker = Read<float>(basePtr, walker, out data.x);
            walker = Read<float>(basePtr, walker, out data.y);
            walker = Read<float>(basePtr, walker, out data.z);
            walker = Read<float>(basePtr, walker, out data.w);

            return walker;
        }
    }
}
