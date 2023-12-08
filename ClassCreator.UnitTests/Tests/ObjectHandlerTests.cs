using ClassCreator.Data.Core;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.UnitTests.Utility;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClassCreator.UnitTests.Tests
{
    public class ObjectHandlerTests
    {
        private ObjectHandler _handler;
        private Mock<ILogger<ObjectHandler>> _loggerMock;
        private readonly static string _path = Path.Combine(Directory.GetCurrentDirectory(), "Classes");

        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(_path))
            {
                var fileNames = Directory.GetFiles(_path);

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            _loggerMock = new Mock<ILogger<ObjectHandler>>();
            _handler = new ObjectHandler(_loggerMock.Object);
        }

        [Test]
        public void AddTest()
        {
            Assert.IsTrue(_handler.Add(Global.ObjectDataDto1));
        }

        [Test]
        public void UpdateTest()
        {
            _handler.Add(Global.ObjectDataDto1);
            Global.UpdateDataDto = new ObjectDataDto()
            {
                Name = Global.ObjectDataDto1.Name,
                AccessModifier = Global.ObjectDataDto1.AccessModifier,
                DataType = Global.ObjectDataDto1.DataType,
                PropertyData = Global.ObjectDataDto2.PropertyData,
            };

            Assert.IsTrue(_handler.Update(Global.UpdateDataDto));
        }

        [Test]
        public void GetTest()
        {
            _handler.Add(Global.ObjectDataDto1);
            var result = _handler.Get(Global.ObjectDataDto1.Name);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetAllTest()
        {
            _handler.Add(Global.ObjectDataDto1);
            _handler.Add(Global.ObjectDataDto2);
            _handler.Add(Global.ObjectDataDto3);
            var result = _handler.GetAll();

            Assert.IsTrue(result.Count().Equals(3));
        }

        [Test]
        public void RemoveTest()
        {
            _handler.Add(Global.ObjectDataDto1);
            _handler.Add(Global.ObjectDataDto2);
            var isRemoved = _handler.Remove(Global.ObjectDataDto1.Name);
            var result = _handler.GetAll();

            Assert.IsTrue(isRemoved && result.Count().Equals(1));
        }

        [Test]
        public void GetInstanceTest()
        {
            _handler.Add(Global.ObjectDataDto1);
            var result = _handler.TryGetInstance(Global.ObjectDataDto1.Name);

            Assert.IsNotNull(result);
        }
    }
}