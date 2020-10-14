﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whydoisuck.DataModel;

namespace Whydoisuck.UIModel.GraphView
{
    public class RangeOfAttempts
    {
        public Range Range { get; set; }
        public List<SessionAttempt> Attempts { get; set; }

        public RangeOfAttempts(Range range)
        {
            Range = range;
            Attempts = new List<SessionAttempt>();
        }
    }
}
