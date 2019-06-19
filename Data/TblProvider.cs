using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblProvider //id, goal, completion_date, complete, date_completed
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public string Website { get; set; }
        public string Type { get; set; }
    }
}