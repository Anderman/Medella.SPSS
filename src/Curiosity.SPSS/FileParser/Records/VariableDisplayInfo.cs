﻿using Curiosity.SPSS.SpssDataset;

namespace Curiosity.SPSS.FileParser.Records
{
    public class VariableDisplayInfo
    {
        internal VariableDisplayInfo()
        {
        }

        public MeasurementType MeasurementType { get; internal set; }
        public int Width { get; internal set; }
        public Alignment Alignment { get; internal set; }
    }
}