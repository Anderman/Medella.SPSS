using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spss.FileStructure;
using Spss.Models;
using Spss.SpssMetadata;

namespace Spss.MetadataReaders.RecordReaders
{
    public class RecordTypeReader
    {
        private readonly MetadataInfo _metadataInfo;

        private readonly Reader _reader;
        private int _currentIndex = 1;

        public RecordTypeReader(Reader reader, MetadataInfo metadataInfo)
        {
            _reader = reader;
            _metadataInfo = metadataInfo;
        }

        public void ReadEndRecord()
        {
            _reader.BaseStream.Seek(4, SeekOrigin.Current);
        }


        public void ReadHeaderRecord()
        {
            _reader.BaseStream.Seek(60 + 4, SeekOrigin.Current);
            _reader.ReadInt32();
            _metadataInfo.Compressed = _reader.ReadInt32();
            _reader.ReadInt32();
            _metadataInfo.Metadata.Cases = _reader.ReadInt32();
            _metadataInfo.Metadata.Bias = (int) _reader.ReadDouble();
            _reader.BaseStream.Seek(17 + 64 + 3, SeekOrigin.Current);
        }

        public void ReadValueLabelRecord()
        {
            var count = _reader.ReadInt32();
            List<(byte[] value, byte[] label)> labels = new List<(byte[] value, byte[] label)>();
            for (var i = 0; i < count; i++)
            {
                var value = _reader.ReadBytes(8);
                var length = _reader.ReadByte();
                var label = _reader.ReadBytes(length);
                _reader.ReadBytes((length + 1 + 7) / 8 * 8 - (length + 1));
                labels.Add((value, label));
            }

            _reader.ReadInt32();
            count = _reader.ReadInt32();
            var indexes = Enumerable.Range(0, count).Select(_ => _reader.ReadInt32()).ToList();
            _metadataInfo.ShortValueLabels.Add(new ShortValueLabel(labels, indexes));
        }

        public void ReadVariableRecord()
        {
            var valueLength = _reader.ReadInt32();
            var hasVariableLabel = _reader.ReadInt32() == 1;
            var missingValueType = _reader.ReadInt32();
            _reader.ReadInt32(); //print format
            var decimalPlaces = _reader.ReadByte();
            var spssWidth = _reader.ReadByte();
            var formatType = _reader.ReadByte();
            _reader.ReadByte();
            var shortName = _reader.ReadBytes(8);

            var properties = new VariableProperties { FormatType = (FormatType) formatType, Index = _currentIndex, MissingValueType = missingValueType, ShortName = shortName, DecimalPlaces = decimalPlaces };
            if (hasVariableLabel)
                properties.Label = ReadLabel();
            if (Math.Abs(missingValueType) != 0)
                ReadMissing(properties);

            var blockWidth = ReadBlankRecords(valueLength);

            properties.ValueLength = blockWidth;
            properties.SpssWidth = formatType == (int) FormatType.A ? blockWidth : spssWidth;
            _metadataInfo.Variables.Add(properties);
            _currentIndex += SpssMath.GetNumberOf32ByteBlocks(blockWidth);
        }

        private void ReadMissing(VariableProperties properties)
        {
            properties.Missing = Enumerable.Range(0, Math.Abs(properties.MissingValueType)).Select(_ => _reader.ReadBytes(8)).ToArray();
        }

        private byte[] ReadLabel()
        {
            var length = _reader.ReadInt32();
            var label = _reader.ReadBytes(length);
            _reader.ReadBytes((length + 3) / 4 * 4 - length);
            return label;
        }

        private int ReadBlankRecords(int valueLength)
        {
            var lengthTotal = 0;
            if (valueLength <= 8) return 8;
            var skip = SpssMath.GetAllocatedSize(valueLength);
            lengthTotal += skip == 256 ? 252 : valueLength;
            skip -= 8;
            _reader.BaseStream.Seek(skip * 4, SeekOrigin.Current);
            return lengthTotal;
        }
    }
}