﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLearning.Contract.Data.Requests.Groups
{
    public class GroupAccessTypeRequest
    {
        public int GroupId { get; set; }
        public string UserName { get; set; }
    }
}
