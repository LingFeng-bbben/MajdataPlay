﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public class LangTable
    {
        public MajText Type { get; set; } = MajText.OTHER_MAJTEXT;
        public string Origin { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
