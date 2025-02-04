using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public class InvalidChartSyntaxException : ArgumentException
    {
        public string RawContent { get; init; } = string.Empty;

        public InvalidChartSyntaxException(SimaiTimingPoint timingPoint) : this(timingPoint.RawContent, $"(at L{timingPoint.RawTextPositionY}:C{timingPoint.RawTextPositionX}) \"{timingPoint.RawContent}\" is not a valid simai syntax", null)
        {

        }
        public InvalidChartSyntaxException(SimaiTimingPoint timingPoint, Exception innerE) : this(timingPoint.RawContent, $"(at L{timingPoint.RawTextPositionY}:C{timingPoint.RawTextPositionX}) \"{timingPoint.RawContent}\" is not a valid simai syntax", innerE)
        {

        }
        public InvalidChartSyntaxException(string rawContent,int x,int y) : this(rawContent, $"(at L{y}:C{x}) \"{rawContent}\" is not a valid simai syntax", null)
        {

        }
        public InvalidChartSyntaxException(string rawContent, int x, int y, Exception innerE) : this(rawContent, $"(at L{y}:C{x}) \"{rawContent}\" is not a valid simai syntax", innerE)
        {

        }
        public InvalidChartSyntaxException(string rawContent) :this(rawContent,$"\"{rawContent}\" is not a valid simai syntax",null)
        { 
        }
        public InvalidChartSyntaxException(string rawContent,Exception innerE) : this(rawContent, $"\"{rawContent}\" is not a valid simai syntax", innerE)
        {
        }
        public InvalidChartSyntaxException(string rawContent,string msg,Exception? innerE):base(msg,innerE)  
        {
            RawContent = rawContent;
        }
    }
}
