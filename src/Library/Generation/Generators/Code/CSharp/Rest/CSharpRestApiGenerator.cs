using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atom.Generation.Generators.Code.CSharp.Rest
{
    public class CSharpRestApiGenerator
    {
        public string Generate()
        {
            return $@"
using test;
namespace Thing 
{{

    public class TestController : ApiController
    {{
        
        public CreateTestResponse Create(CreateTestRequest )
        {{
            
        }}
    
    }}

}}";
        }
    }
}
