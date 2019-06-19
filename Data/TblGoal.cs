using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblGoal //id, goal, completion_date, complete, date_completed
    {
        public Int64 Id { get; set; }
        public string Goal { get; set; }
        public string CompletionDate { get; set; }
        public Int32 Complete { get; set; }
        public string DateCompleted { get; set; }
    }
}