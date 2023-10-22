using ClassCreator.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClassCreator.Controllers.Core
{
    public class ObjectController: ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IObjectHandler _objectCreator;

        public ObjectController(ILogger logger, IObjectHandler objectCreator)
        {
            _logger = logger;
            _objectCreator = objectCreator;
        }
    }
}