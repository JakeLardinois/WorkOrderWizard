﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkOrderWizard.Models
{
    public class WorkOrderSearch
    {
        public string WONUM { get; set; }
        public DateTime CLOSEDATEGT { get; set; }
        public DateTime CLOSEDATELT { get; set; }
        public string TASKDESC { get; set; }
        public string[] WOTYPES { get; set; }
        public string ORIGINATOR { get; set; }
        public string PRIORITY { get; set; }
        public DateTime REQUESTDATEGT { get; set; }
        public DateTime REQUESTDATELT { get; set; }
        public string[] STATUSES { get; set; }


        //Searchable WOEQLIST properties...
        public string[] EQNUMS { get; set; }
    }
}