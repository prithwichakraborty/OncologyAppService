using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblSSpedi
    {
        public Int64 Id { get; set; }
        public string Date { get; set; }
        public string Day { get; set; }
        public Int64 Disappointed { get; set; }
        public Int64 Scared { get; set; }
        public Int64 Cranky { get; set; }
    }
}