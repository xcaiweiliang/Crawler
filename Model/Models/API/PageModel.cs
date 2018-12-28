﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.API
{
    public class PageModel<T> where T:class
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalPage { get; set; }

        public List<T> ResList { get; set; } 


    }
}
