using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblBloodResult
    {
        public Int64 Id { get; set; }
        //public Double Wcc { get; set; }
        //public Double Neutrophils { get; set; }
        public Int64 Weight { get; set; }
        public Int64 Height { get; set; }
        public Int64 Hb { get; set; }
        public Int64 Platelets { get; set; }
        public String Date { get; set; }
        public String Day { get; set; }
    }
}