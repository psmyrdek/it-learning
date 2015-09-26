﻿using ITLearning.Frontend.Web.Core.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLearning.Frontend.Web.Core.Identity.Services
{
    public interface IIdentityService
    {
        Task SignUpAsync(SignUpModel model);
    }
}