using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mikodev.Binary.Dynamic")]

namespace Mikodev.Binary.Internal
{
    internal abstract partial class ModuleHelper
    {
        internal static readonly AssemblyBuilder AssemblyBuilder;

        internal static readonly ModuleBuilder ModuleBuilder;

        internal static readonly ModuleHelper Instance;

        static ModuleHelper()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Mikodev.Binary.Dynamic"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Mikodev.Binary.Dynamic");
            Instance = CreateInstance();
        }

        private static ModuleHelper CreateInstance()
        {
            var typeBuilder = ModuleBuilder.DefineType(nameof(ModuleHelper), TypeAttributes.Public | TypeAttributes.Sealed, typeof(ModuleHelper));
            var allocatorByRefType = typeof(Allocator).MakeByRefType();
            const MethodAttributes MethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;

            var handleMethodBuilder = typeBuilder.DefineMethod(nameof(AsHandle), MethodAttributes, typeof(IntPtr), new[] { allocatorByRefType });
            var handleGenerator = handleMethodBuilder.GetILGenerator();
            handleGenerator.Emit(OpCodes.Ldarg_1);
            handleGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(handleMethodBuilder, typeof(ModuleHelper).GetMethod(nameof(AsHandle)));

            var allocatorMethodBuilder = typeBuilder.DefineMethod(nameof(AsAllocator), MethodAttributes, allocatorByRefType, new[] { typeof(IntPtr) });
            var allocatorGenerator = allocatorMethodBuilder.GetILGenerator();
            allocatorGenerator.Emit(OpCodes.Ldarg_1);
            allocatorGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(allocatorMethodBuilder, typeof(ModuleHelper).GetMethod(nameof(AsAllocator)));

            var helperType = typeBuilder.CreateTypeInfo();
            var result = Activator.CreateInstance(helperType);
            return (ModuleHelper)result;
        }
    }
}
