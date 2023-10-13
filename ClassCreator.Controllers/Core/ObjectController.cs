using ClassCreator.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClassCreator.Controllers.Core
{
    public class ObjectController: ControllerBase
    {
        private IObjectCreator _objectCreator;

        public ObjectController(IObjectCreator objectCreator)
        {
            _objectCreator = objectCreator;
        }
    }
}