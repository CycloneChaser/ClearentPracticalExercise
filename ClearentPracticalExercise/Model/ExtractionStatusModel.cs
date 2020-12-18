using System;
using System.Collections.Generic;
using System.Text;

namespace ClearentPracticalExercise.Model
{
    public class ExtractionStatusModel
    {
        public string ExtractionId { get; set; }
        public int VersionKey { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortBy { get; set; }
        public bool Last { get; set; }
    }
}
