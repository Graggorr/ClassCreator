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

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ObjectHandler>>();
            _handler = new ObjectHandler(_loggerMock.Object);
        }

        [Test]
        public async Task AddTest()
        {
            Assert.IsTrue(await _handler.Add(Global.ObjectDataDto1));
        }

        [Test]
        public async Task UpdateTest()
        {
            await _handler.Add(Global.ObjectDataDto1);
            Global.UpdateDataDto = new ObjectDataDto()
            {
                Name = Global.ObjectDataDto1.Name,
                AccessModifier = Global.ObjectDataDto1.AccessModifier,
                DataType = Global.ObjectDataDto1.DataType,
                PropertyData = Global.ObjectDataDto2.PropertyData,
            };

            Assert.IsTrue(await _handler.Update(Global.UpdateDataDto));
        }

        [Test]
        public async Task GetTest()
        {
            await _handler.Add(Global.ObjectDataDto1);
            var result = _handler.Get(Global.ObjectDataDto1.Name);

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetAllTest()
        {
            await _handler.Add(Global.ObjectDataDto1);
            await _handler.Add(Global.ObjectDataDto2);
            await _handler.Add(Global.ObjectDataDto3);
            var result = _handler.GetAll();

            Assert.IsTrue(result.Count().Equals(3));
        }

        [Test]
        public async Task RemoveTest()
        {
            await _handler.Add(Global.ObjectDataDto1);
            await _handler.Add(Global.ObjectDataDto2);
            var isRemoved = _handler.Remove(Global.ObjectDataDto1.Name);
            var result = _handler.GetAll();

            Assert.IsTrue(isRemoved && result.Count().Equals(1));
        }
    }
}