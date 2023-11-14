using ClassCreator.Data.Utility.Entity;
using ClassCreator.Infrastructure.Utility;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    internal class AssemblyHelper
    {
        private readonly ILogger _logger;
        private readonly AssemblyName _objectsAssemblyName;

        public AssemblyHelper(ILogger logger)
        {
            _logger = logger;
            _objectsAssemblyName = new AssemblyName(Global.OBJECTS_ASSEMBLY_NAME);
        }

        public Assembly? GetDynamicAssembly(ObjectData objectData)
        {
            var methodName = nameof(GetDynamicAssembly);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_objectsAssemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(_objectsAssemblyName.Name ?? Global.OBJECTS_ASSEMBLY_NAME);
            var typeBuilder = moduleBuilder.DefineType(objectData.Name, TypeAttributes.Public);

            try
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                var constructorIlGenerator = constructorBuilder.GetILGenerator();
                constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - ERROR: {exception.Message}");

                return null;
            }

            var tasks = new List<Task<bool>>();

            //define all properties
            objectData.Properties.ForEach(property =>
            {
                var task = Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Trace, $"{methodName} - Defining {property.Name} property");

                    try
                    {
                        var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, Type.EmptyTypes);
                        var fieldBuilder = typeBuilder.DefineField(property.Name, property.PropertyType, FieldAttributes.Private);

                        //getter
                        var getterBuilder = typeBuilder.DefineMethod($"get_{property.Name}", property.GetterAccessModifier, property.PropertyType, Type.EmptyTypes);
                        var getterIlGenerator = getterBuilder.GetILGenerator();
                        getterIlGenerator.Emit(OpCodes.Ldarg_0);
                        getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                        getterIlGenerator.Emit(OpCodes.Ret);

                        //setter
                        var setterBuilder = typeBuilder.DefineMethod($"set_{property.Name}", property.SetterAccessModifier, null, new[] { property.PropertyType });
                        var setterIlGenerator = setterBuilder.GetILGenerator();
                        setterIlGenerator.Emit(OpCodes.Ldarg_0);
                        setterIlGenerator.Emit(OpCodes.Ldarg_1);
                        setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                        setterIlGenerator.Emit(OpCodes.Ret);

                        propertyBuilder.SetSetMethod(setterBuilder);
                        propertyBuilder.SetGetMethod(getterBuilder);

                        _logger.Log(LogLevel.Trace, $"{methodName} - Defining of {property.Name} has been finished");

                        return true;
                    }
                    catch (Exception exception)
                    {
                        _logger.Log(LogLevel.Error, $"{methodName} - Cannot perform defining of {property.Name}. ERROR: {exception.Message}");

                        return false;
                    }
                });

                tasks.Add(task);
            });

            Task.WaitAll(tasks.ToArray());

            return tasks.Any(x => !x.Result) ? null : assemblyBuilder;
        }
    }
}
