using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InjectionBuilder.Builder;
using InjectionBuilder.Mapper;
using InjectionBuilder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace InjectionBuilder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeGenController : ControllerBase
    {
        private static readonly InjectionProcessor processor = new InjectionProcessor();
        private static readonly MappingGenerator mappingGenerator = new MappingGenerator();

        [HttpPost]
        [Route("[action]")]
        public string Constructor([FromBody] string csharp)
        {            
            return processor.CreateClass(csharp, new ConstructorBuilder(), new PassthroughTransform());
        }

        [HttpPost]
        [Route("[action]")]
        public string Mock([FromBody] string csharp)
        {
            return processor.CreateClass(csharp, new InitalizeBuilder(), new MockTransform(), "Test", SyntaxKind.PrivateKeyword);
        }

        [HttpPost]
        [Route("[action]")]
        public string Mapping([FromBody] MappingModel model)
        {
            return mappingGenerator.BuildMap(model.SourceTo, model.SourceFrom);
        }
    }
}
