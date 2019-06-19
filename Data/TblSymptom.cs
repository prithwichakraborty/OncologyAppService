using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblSymptom
    {
        public Int64 Id { get; set; }
        public string Symptom { get; set; }
        public string Type { get; set; }
        public string Detail { get; set; }
    }
}