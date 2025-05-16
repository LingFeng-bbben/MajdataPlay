using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public class ChartNotFoundException: Exception
    {
        public SongDetail SongDetail { get; init; }
        public ChartNotFoundException(SongDetail songDetail):base("MaiChart not found") 
        { 
            SongDetail = songDetail;
        }
    }
}
