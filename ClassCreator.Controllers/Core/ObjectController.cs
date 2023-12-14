using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClassCreator.Controllers.Core
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IObjectHandler _objectCreator;

        public ObjectController(ILogger<ObjectController> logger, IObjectHandler objectCreator)
        {
            _logger = logger;
            _objectCreator = objectCreator;
        }

        [HttpPost("/add/")]
        public IActionResult PostObject(ObjectDataDto objectDataDto)
        {
            var methodName = nameof(PostObject);

            var result = _objectCreator.Add(objectDataDto);

            if (result)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {objectDataDto.Name} has been added");

                return Ok();
            }

            _logger.Log(LogLevel.Warning, $"{methodName} - A new object has not been added. Possible name: {objectDataDto?.Name}");

            return BadRequest();
        }

        [HttpGet("/{name}/")]
        public IActionResult GetObject(string name)
        {
            var methodName = nameof(GetObject);
            var result = _objectCreator.Get(name);

            if (result is not null)
            {
                return Ok(result);
            }

            _logger.Log(LogLevel.Information, $"{methodName} - Object with chosen name is not found");

            return BadRequest($"{name} is not found.");
        }

        [HttpGet()]
        public IActionResult GetAll()
        {
            var result = _objectCreator.GetAll();

            if (!result.Any())
            {
                return new EmptyResult();
            }

            return Ok(result);
        }

        [HttpDelete("/{name}/")]
        public IActionResult RemoveObject(string name)
        {
            var methodName = nameof(RemoveObject);
            var result = _objectCreator.Remove(name);

            if (result)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {name} has been removed successfully");

                return Ok();
            }

            _logger.Log(LogLevel.Warning, $"{methodName} - Cannot remove {name}");

            return BadRequest();
        }

        [HttpPut("/{name}/")]
        public IActionResult UpdateObject(string name, ObjectDataDto dto)
        {
            var methodName = nameof(UpdateObject);
            var result = _objectCreator.Update(dto);

            if (result)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {name} has been updated successfully");

                return Ok();
            }

            _logger.Log(LogLevel.Error, $"{methodName} - Cannot update chosen object");

            return BadRequest();
        }
    }
}