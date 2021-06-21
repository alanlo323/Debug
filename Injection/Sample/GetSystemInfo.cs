using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Injection;

////
namespace Injection.Sample
{
    public class GetSystemInfo : IExecutable
    {
        public async Task<object> Run(string[] args = null)
        {
            var result = DateTime.Now;
            return result;
        }
    }
}