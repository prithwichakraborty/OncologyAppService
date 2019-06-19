using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OncologyAppService.Data
{
    public class TblContact //id, name, phone, email, location, note, provider_id, type
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public string Note { get; set; }
        public Int64 ProviderId { get; set; }
        public string Type { get; set; }
    }
}