using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using System.Reflection;

namespace ClassCreator.Data.Utility
{
    /// <summary>
    /// Creates dynamic assembly and builds type of incoming <see cref="ObjectData"/>
    /// </summary>
    internal class AssemblyHelper
    {
        private readonly ILogger _logger;

        public AssemblyHelper(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates an instance of <see cref="Type"/> of incoming <see cref="ObjectData"/>
        /// </summary>
        /// <param name="objectData"></param>
        /// <returns>A new created instance if creating of dynamic assembly has been successed; otherwise - NULL</returns>
        public Type? CreateTypeWithDynamicAssembly(ObjectData objectData)
        {
            var methodName = nameof(CreateTypeWithDynamicAssembly);
            var objectAssemblyNameString = $"Dynamic.{objectData.Name}";
            var objectAssemblyName = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.Equals(objectAssemblyNameString))
                ? new AssemblyName($"New{objectAssemblyNameString}")
                : new AssemblyName(objectAssemblyNameString);

            //define assembly, module, type and get constructor of object (default)
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(objectAssemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(objectAssemblyName.Name);
            var typeBuilder = moduleBuilder.DefineType(objectData.Name, TypeAttributes.Public);
            var objectConstructorInfo = typeof(object).GetConstructor(Type.EmptyTypes);
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var constructorIlGenerator = constructorBuilder.GetILGenerator();

            //create default constructor
            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            constructorIlGenerator.Emit(OpCodes.Call, objectConstructorInfo!);
            constructorIlGenerator.Emit(OpCodes.Ret);

            //define all properties
            var tasks = objectData.Properties.Select(property =>
            {
                return Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Trace, $"{methodName} - Defining {property.Name} property");
                    var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, Type.EmptyTypes);
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
                            null, new Type[] { property.PropertyType });
                        var setterIlGenerator = setterBuilder.GetILGenerator();
                        var markedLabel = setterIlGenerator.DefineLabel();
                        setterIlGenerator.Emit(OpCodes.Ldarg_0);
                        setterIlGenerator.Emit(OpCodes.Ldarg_1);
                        setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                        setterIlGenerator.Emit(OpCodes.Ret);

                        propertyBuilder.SetSetMethod(setterBuilder);
                    }

                    _logger.Log(LogLevel.Trace, $"{methodName} - Defining of {property.Name} has been finished");
                });
            }).ToArray();
            Task.WaitAll(tasks);
            var type = typeBuilder.CreateType();

            return GetInstance(type) is not null ? type : null;
        }

        /// <summary>
        /// Creates a new instance of chosen <see cref="Type"/>
        /// </summary>
        /// <param name="type">Chosen type for instance creation</param>
        /// <returns>A new created instance as an <see cref="Object"/> or null</returns>
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
