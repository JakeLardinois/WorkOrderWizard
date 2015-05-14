using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkOrderWizard.Models
{
    public class WorkOrderSearch
    {
        public WorkOrderSearch()
        {
            WONUM = string.Empty;
            TASKDESC = string.Empty;
            ORIGINATOR = string.Empty;

        }

        public string WONUM { get; set; }
        public DateTime CLOSEDATEGT { get; set; }
        public DateTime CLOSEDATELT { get; set; }
        public string TASKDESC { get; set; }
        public string[] WOTYPES { get; set; }
        public string ORIGINATOR { get; set; }
        public string[] PRIORITIES { get; set; }
        public DateTime REQUESTDATEGT { get; set; }
        public DateTime REQUESTDATELT { get; set; }
        public string[] STATUSES { get; set; }
        public DateTime COMPLETIONDATEGT { get; set; }
        public DateTime COMPLETIONDATELT { get; set; }

        //Searchable WOEQLIST properties...
        public string[] EQNUMS { get; set; }
    }
}