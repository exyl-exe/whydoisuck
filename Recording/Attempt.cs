﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whydoisuck.Recording
{
    class Attempt
    {
        public int Number { get; set; }
        public float StartPercent { get; set; }
        public float EndPercent { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}