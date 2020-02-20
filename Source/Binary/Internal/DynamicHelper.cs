using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mikodev.Binary.Dynamic")]

namespace Mikodev.Binary.Internal
{
    internal static partial class DynamicHelper
    {
        internal static readonly AssemblyBuilder AssemblyBuilder;

        internal static readonly ModuleBuilder ModuleBuilder;

        internal static readonly UnsafeHelper UnsafeHelperInstance;

        static DynamicHelper()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Mikodev.Binary.Dynamic"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Mikodev.Binary.Dynamic");
            UnsafeHelperInstance = BuildUnsafeHelper();
        }

        private static UnsafeHelper BuildUnsafeHelper()
        {
            var typeBuilder = ModuleBuilder.DefineType(nameof(UnsafeHelper), TypeAttributes.Public | TypeAttributes.Sealed, typeof(UnsafeHelper));
            var allocatorByRefType = typeof(Allocator).MakeByRefType();
            const MethodAttributes MethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;

            var pointerMethodBuilder = typeBuilder.DefineMethod(nameof(UnsafeHelper.AsPointer), MethodAttributes, typeof(IntPtr), new[] { allocatorByRefType });
            var pointerGenerator = pointerMethodBuilder.GetILGenerator();
            pointerGenerator.Emit(OpCodes.Ldarg_1);
            pointerGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(pointerMethodBuilder, typeof(UnsafeHelper).GetMethod(nameof(UnsafeHelper.AsPointer)));

            var allocatorMethodBuilder = typeBuilder.DefineMethod(nameof(UnsafeHelper.AsAllocator), MethodAttributes, allocatorByRefType, new[] { typeof(IntPtr) });
            var allocatorGenerator = allocatorMethodBuilder.GetILGenerator();
            allocatorGenerator.Emit(OpCodes.Ldarg_1);
            allocatorGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(allocatorMethodBuilder, typeof(UnsafeHelper).GetMethod(nameof(UnsafeHelper.AsAllocator)));

            var helperType = typeBuilder.CreateTypeInfo();
            return (UnsafeHelper)Activator.CreateInstance(helperType);
        }
    }
}
