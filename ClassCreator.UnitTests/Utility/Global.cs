using ClassCreator.Data.Utility.DTO;

namespace ClassCreator.UnitTests.Utility
{
    public class Global
    {
        public static ObjectDataDto ObjectDataDto1 { get; }
        public static ObjectDataDto ObjectDataDto2 { get; }
        public static ObjectDataDto ObjectDataDto3 { get; }
        public static ObjectDataDto UpdateDataDto { get; set; }

        static Global()
        {
            var propertyDataDto1 = new PropertyDataDto
            {
                Name = "IntegerValue",
                AccessModifier = "public",
                GetterAccessModifier = "public",
                SetterAccessModifier = "public",
                PropertyType = "int",
            };
            var propertyDataDto2 = new PropertyDataDto
            {
                Name = "StringValue",
                AccessModifier = "public",
                GetterAccessModifier = "public",
                SetterAccessModifier = string.Empty,
                PropertyType = "string",
            };
            var propertyDataDto3 = new PropertyDataDto
            {
                Name = "BooleanValue",
                AccessModifier = "public",
                GetterAccessModifier = "public",
                SetterAccessModifier = string.Empty,
                PropertyType = "bool",
            };
            ObjectDataDto1 = new ObjectDataDto
            {
                AccessModifier = "public",
                Name = "Data1",
                DataType = "class",
                PropertyData = new List<PropertyDataDto> { propertyDataDto1 }
            };
            ObjectDataDto2 = new ObjectDataDto
            {
                AccessModifier = "public",
                Name = "Data2",
                DataType = "class",
                PropertyData = new List<PropertyDataDto> { propertyDataDto2 }
            };
            ObjectDataDto3 = new ObjectDataDto
            {
                AccessModifier = "public",
                Name = "Data3",
                DataType = "class",
                PropertyData = new List<PropertyDataDto> { propertyDataDto3 }
            };
        }
    }
}
