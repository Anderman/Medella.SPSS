using SpssCommon.SpssMetadata;

namespace SpssCommon.FileStructure
{
    public class DisplayParameter
    {
        public MeasurementType Measure { get; set; }
        public int Columns { get; set; }
        public Alignment Alignment { get; set; }
    }
}