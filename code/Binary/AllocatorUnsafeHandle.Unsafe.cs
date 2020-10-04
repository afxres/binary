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
        private static readonly HandleHelper HandleHelperInstance;

        static AllocatorUnsafeHandle()
        {
            static void MakeHandleHelperMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
            {
                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, methodInfo.ReturnType, methodInfo.GetParameters().Select(x => x.ParameterType).ToArray());
                var generator = methodBuilder.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            static HandleHelper MakeHandleHelper(ModuleBuilder moduleBuilder)
            {
                var typeBuilder = moduleBuilder.DefineType(nameof(HandleHelper), TypeAttributes.Public | TypeAttributes.Sealed, typeof(HandleHelper));
                MakeHandleHelperMethod(typeBuilder, typeof(HandleHelper).GetMethod(nameof(HandleHelper.AsHandle)));
                MakeHandleHelperMethod(typeBuilder, typeof(HandleHelper).GetMethod(nameof(HandleHelper.AsAllocator)));
                var typeInfo = typeBuilder.CreateTypeInfo();
                Debug.Assert(typeInfo is not null);
                return (HandleHelper)Activator.CreateInstance(typeInfo);
            }

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Mikodev.Binary.Unsafe.Handle"), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("InMemoryModule");
            HandleHelperInstance = MakeHandleHelper(moduleBuilder);
        }
    }
}
