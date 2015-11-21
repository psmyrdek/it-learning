﻿using System.Collections.Generic;
using ITLearning.Contract.Data.Results;
using ITLearning.Contract.Data.Model.News;
using ITLearning.Contract.Data.Requests;

namespace ITLearning.Contract.Services
{
    public interface INewsService
    {
        CommonResult<IEnumerable<NewsData>> GetAll(bool withContent);
        CommonResult<IEnumerable<NewsData>> GetFiltered(NewsFilterRequest filterRequest);
        CommonResult<NewsData> GetById(string id);
        CommonResult<NewsListRequest> GetInitialRequest();

    }
}