using System;
using System.Diagnostics;
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
        internal static readonly ModuleHelper ModuleHelperInstance;

        internal static readonly AllocatorAction<IPAddress> IPAddressAction;

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
                var typeBuilder = moduleBuilder.DefineType(nameof(ModuleHelper), TypeAttributes.Public | TypeAttributes.Sealed, typeof(ModuleHelper));
                var allocatorByRefType = typeof(Allocator).MakeByRefType();
                MakeModuleHelperMethod(typeBuilder, nameof(ModuleHelper.AsHandle), typeof(IntPtr), new[] { allocatorByRefType });
                MakeModuleHelperMethod(typeBuilder, nameof(ModuleHelper.AsAllocator), allocatorByRefType, new[] { typeof(IntPtr) });
                return (ModuleHelper)Activator.CreateInstance(typeBuilder.CreateTypeInfo());
            }

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Mikodev.Binary.Dynamic"), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("InMemoryModule");
            ModuleHelperInstance = MakeModuleHelper(moduleBuilder);
#if NETOLD
            IPAddressAction = (span, data) => data.GetAddressBytes().CopyTo(span);
#else
            IPAddressAction = (span, data) => data.TryWriteBytes(span, out _);
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
        internal static int GetBytes(ReadOnlySpan<char> source, Span<byte> target, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(source.Length != 0);
            Debug.Assert(target.Length != 0);
#if NETOLD
            unsafe
            {
                fixed (char* srcptr = &MemoryMarshal.GetReference(source))
                fixed (byte* dstptr = &MemoryMarshal.GetReference(target))
                    return encoding.GetBytes(srcptr, source.Length, dstptr, target.Length);
            }
#else
            return encoding.GetBytes(source, target);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxByteCount(ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var length = span.Length;
            if (length == 0)
                return 0;
            const int Limits = 64;
            if ((uint)length <= Limits && encoding == Converter.Encoding)
                return (length + 1) * 3;
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
