using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClassCreator.Controllers.Core
{
    [ApiController]
    public class ObjectController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IObjectHandler _objectCreator;

        public ObjectController(ILogger<ObjectController> logger, IObjectHandler objectCreator)
        {
            _logger = logger;
            _objectCreator = objectCreator;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> PostObject(ObjectDataDto objectDataDto)
        {
            var methodName = nameof(PostObject);

            var result = await _objectCreator.AddOrUpdate(objectDataDto);

            if (result)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {objectDataDto.Name} has been added");

                return Ok();
            }

            _logger.Log(LogLevel.Warning, $"{methodName} - A new object has not been added. Possible name: {objectDataDto?.Name}");

            return BadRequest();
        }
    }
}