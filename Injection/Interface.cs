using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Injection
{
    public interface IExecutable
    {
        Task<object> Run(string[] args = null);
    }
}