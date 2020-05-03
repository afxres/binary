using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mikodev.Binary
{
    public readonly partial struct AllocatorUnsafeHandle
    {
        private static readonly ModuleHelper ModuleHelperInstance;

        static AllocatorUnsafeHandle()
        {
            static void MakeModuleHelperMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
            {
                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, methodInfo.ReturnType, methodInfo.GetParameters().Select(x => x.ParameterType).ToArray());
                var generator = methodBuilder.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            static ModuleHelper MakeModuleHelper(ModuleBuilder moduleBuilder)
            {
                var typeBuilder = moduleBuilder.DefineType(nameof(ModuleHelper), TypeAttributes.Public | TypeAttributes.Sealed, typeof(ModuleHelper));
                MakeModuleHelperMethod(typeBuilder, typeof(ModuleHelper).GetMethod(nameof(ModuleHelper.AsHandle)));
                MakeModuleHelperMethod(typeBuilder, typeof(ModuleHelper).GetMethod(nameof(ModuleHelper.AsAllocator)));
                var typeInfo = typeBuilder.CreateTypeInfo();
                Debug.Assert(typeInfo != null);
                return (ModuleHelper)Activator.CreateInstance(typeInfo);
            }

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Mikodev.Binary.Dynamic"), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("InMemoryModule");
            ModuleHelperInstance = MakeModuleHelper(moduleBuilder);
        }
    }
}
