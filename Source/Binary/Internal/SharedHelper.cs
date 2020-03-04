using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class SharedHelper
    {
        private static readonly int[] MaxByteCounts;

        private static readonly AssemblyBuilder AssemblyBuilder;

        private static readonly ModuleBuilder ModuleBuilder;

        internal static readonly ModuleHelper ModuleHelper;

        internal static readonly AllocatorAction<IPAddress> WriteIPAddress;

        static SharedHelper()
        {
            static void MakeModuleHelperMethod(TypeBuilder typeBuilder, string name, Type returnType, Type[] parameterTypes)
            {
                var methodBuilder = typeBuilder.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes);
                var generator = methodBuilder.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder, typeof(ModuleHelper).GetMethod(name));
            }

            static ModuleHelper MakeModuleHelper(ModuleBuilder moduleBuilder)
            {
                var typeBuilder = moduleBuilder.DefineType(nameof(Internal.ModuleHelper), TypeAttributes.Public | TypeAttributes.Sealed, typeof(ModuleHelper));
                var allocatorByRefType = typeof(Allocator).MakeByRefType();
                MakeModuleHelperMethod(typeBuilder, nameof(Internal.ModuleHelper.AsHandle), typeof(IntPtr), new[] { allocatorByRefType });
                MakeModuleHelperMethod(typeBuilder, nameof(Internal.ModuleHelper.AsAllocator), allocatorByRefType, new[] { typeof(IntPtr) });
                return (ModuleHelper)Activator.CreateInstance(typeBuilder.CreateTypeInfo());
            }

            const int MaxByteCountLimits = 256;
            MaxByteCounts = Enumerable.Range(0, MaxByteCountLimits + 1).Select(Converter.Encoding.GetMaxByteCount).ToArray();
            MaxByteCounts[0] = 0;
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Mikodev.Binary.Dynamic"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("InMemoryModule");
            ModuleHelper = MakeModuleHelper(ModuleBuilder);
#if NETOLD
            WriteIPAddress = (span, data) => data.GetAddressBytes().CopyTo(span);
#else
            WriteIPAddress = (span, data) => data.TryWriteBytes(span, out _);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IPAddress GetIPAddress(ReadOnlySpan<byte> source)
        {
#if NETOLD
            return new IPAddress(source.ToArray());
#else
            return new IPAddress(source);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetString(ReadOnlySpan<byte> source, Encoding encoding)
        {
            Debug.Assert(encoding != null);
#if NETOLD
            var length = source.Length;
            if (length == 0)
                return string.Empty;
            unsafe
            {
                fixed (byte* srcptr = &MemoryMarshal.GetReference(source))
                    return encoding.GetString(srcptr, length);
            }
#else
            return encoding.GetString(source);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetBytes(ReadOnlySpan<char> source, ref byte target, int limits, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(source.Length != 0);
            Debug.Assert(limits >= 0);
#if NETOLD
            unsafe
            {
                fixed (byte* dstptr = &target)
                fixed (char* srcptr = &MemoryMarshal.GetReference(source))
                    return encoding.GetBytes(srcptr, source.Length, dstptr, limits);
            }
#else
            return encoding.GetBytes(source, MemoryMarshal.CreateSpan(ref target, limits));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxByteCount(ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var length = span.Length;
            if (length == 0)
                return 0;
            int[] counts;
            if (encoding == Converter.Encoding && (uint)length < (uint)(counts = MaxByteCounts).Length)
                return counts[length];
#if NETOLD
            unsafe
            {
                fixed (char* srcptr = &MemoryMarshal.GetReference(span))
                    return encoding.GetByteCount(srcptr, span.Length);
            }
#else
            return encoding.GetByteCount(span);
#endif
        }
    }
}
