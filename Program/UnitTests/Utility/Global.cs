using ClassCreator.Data.Files.DTO;

namespace ClassCreator.UnitTests.Utility
{
    public class Global
    {
        public static FileObjectDataDto ObjectDataDto1 { get; }
        public static FileObjectDataDto ObjectDataDto2 { get; }
        public static FileObjectDataDto ObjectDataDto3 { get; }
        public static FileObjectDataDto UpdateDataDto { get; set; }

        static Global()
        {
            var propertyDataDto1 = new FilePropertyDataDto
            {
                Name = "IntegerValue",
                AccessModifier = "public",
                GetterAccessModifier = "public",
                SetterAccessModifier = "public",
                PropertyType = "int",
            };
            var propertyDataDto2 = new FilePropertyDataDto
            {
                Name = "StringValue",
                AccessModifier = "public",
                GetterAccessModifier = "public",
                SetterAccessModifier = string.Empty,
                PropertyType = "string",
            };
            var propertyDataDto3 = new FilePropertyDataDto
            {
                Name = "BooleanValue",
                AccessModifier = "public",
                GetterAccessModifier = "public",
                SetterAccessModifier = string.Empty,
                PropertyType = "bool",
            };
            ObjectDataDto1 = new FileObjectDataDto
            {
                AccessModifier = "public",
                Name = "Data1",
                DataType = "class",
                PropertyData = new List<FilePropertyDataDto> { propertyDataDto1 }
            };
            ObjectDataDto2 = new FileObjectDataDto
            {
                AccessModifier = "public",
                Name = "Data2",
                DataType = "class",
                PropertyData = new List<FilePropertyDataDto> { propertyDataDto2 }
            };
            ObjectDataDto3 = new FileObjectDataDto
            {
                AccessModifier = "public",
                Name = "Data3",
                DataType = "class",
                PropertyData = new List<FilePropertyDataDto> { propertyDataDto3 }
            };
        }
    }
}
