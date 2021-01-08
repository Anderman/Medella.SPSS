﻿using System;

namespace Curiosity.SPSS.FileParser
{
    [Serializable]
    public class SpssFileFormatException : Exception
    {
        public SpssFileFormatException()
        {
        }

        public SpssFileFormatException(string message)
            : base(message)
        {
        }

        public SpssFileFormatException(string message, int dictionaryIndex)
            : base(message + ". Dictionary index " + dictionaryIndex)
        {
            DictionaryIndex = dictionaryIndex;
        }

        public int DictionaryIndex { get; set; }
    }
}