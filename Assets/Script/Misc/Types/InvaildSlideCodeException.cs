using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public class InvaildSlideCodeException : Exception
    {
        public InvaildSlideCodeException(string err) : base(err) 
        { }
    }
}
