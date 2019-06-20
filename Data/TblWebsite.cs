using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblWebsite //id, goal, completion_date, complete, date_completed
    {
        public Int64 Id { get; set; }
        public string Website { get; set; }
        public string Url { get; set; }
    }
}