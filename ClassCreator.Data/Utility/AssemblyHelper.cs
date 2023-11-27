using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using System.Reflection;

namespace ClassCreator.Data.Utility
{
    internal class AssemblyHelper
    {
        private readonly ILogger _logger;

        public AssemblyHelper(ILogger logger)
        {
            _logger = logger;
        }

        public Type? CreateTypeWithDynamicAssembly(ObjectData objectData)
        {
            var methodName = nameof(CreateTypeWithDynamicAssembly);
            var objectAssemblyName = new AssemblyName($"Dynamic.{objectData.Name}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(objectAssemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(objectAssemblyName.Name);
            var typeBuilder = moduleBuilder.DefineType(objectData.Name, TypeAttributes.Public);

            try
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                var constructorIlGenerator = constructorBuilder.GetILGenerator();
                constructorIlGenerator.Emit(OpCodes.Ldarg_0);
                constructorIlGenerator.Emit(OpCodes.Ret);
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - ERROR: {exception.Message}");

                return null;
            }

            //define all properties
            var tasks = objectData.Properties.Select(property =>
            {
                return Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Trace, $"{methodName} - Defining {property.Name} property");

                    try
                    {
                        var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);
                        var fieldAttributes = property.SetterAccessModifier is null ? FieldAttributes.InitOnly : FieldAttributes.Private | FieldAttributes.SpecialName;
                        var fieldBuilder = typeBuilder.DefineField(property.Name, property.PropertyType, fieldAttributes);

                        //getter
                        if (property.GetterAccessModifier is not null)
                        {
                            var getterBuilder = typeBuilder.DefineMethod($"get_{property.Name}", property.GetterAccessModifier.Value | 
                                MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                property.PropertyType, Type.EmptyTypes);
                            var getterIlGenerator = getterBuilder.GetILGenerator();
                            getterIlGenerator.Emit(OpCodes.Ldarg_0);
                            getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                            getterIlGenerator.Emit(OpCodes.Ret);

                            propertyBuilder.SetGetMethod(getterBuilder);
                        }

                        //setter
                        if (fieldAttributes is not FieldAttributes.InitOnly)
                        {
                            var setterBuilder = typeBuilder.DefineMethod($"set_{property.Name}", property.SetterAccessModifier.Value | 
                                MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                null, new[] { property.PropertyType });
                            var setterIlGenerator = setterBuilder.GetILGenerator();
                            var markedLabel = setterIlGenerator.DefineLabel();
                            setterIlGenerator.Emit(OpCodes.Ldarg_0);
                            setterIlGenerator.Emit(OpCodes.Ldarg_1);
                            setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                            setterIlGenerator.Emit(OpCodes.Ret);

                            propertyBuilder.SetSetMethod(setterBuilder);
                        }
#if DEBUG
                        var type = typeBuilder.CreateTypeInfo();
#endif
                        _logger.Log(LogLevel.Trace, $"{methodName} - Defining of {property.Name} has been finished");

                        return true;
                    }
                    catch (Exception exception)
                    {
                        _logger.Log(LogLevel.Error, $"{methodName} - Cannot perform defining of {property.Name}. ERROR: {exception.Message}");

                        return false;
                    }
                });
            }).ToArray();
            Task.WaitAll(tasks);

            if (tasks.Any(x => !x.Result))
            {
                return null;
            }

            var type = typeBuilder.CreateType();
            var t = typeof(int);

            return GetInstance(type) is not null ? type : null;
        }

        public object? GetInstance(Type type)
        {
            var methodName = nameof(GetInstance);

            try
            {
                var instance = Activator.CreateInstance(type);

                return instance;
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Debug, $"{methodName} - Cannot create instance of {type.Name}. ERROR: {exception.Message}");

                return null;
            }
        }
    }
}
