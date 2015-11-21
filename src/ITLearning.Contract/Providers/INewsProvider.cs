﻿using ITLearning.Contract.Data.Model.News;
using System.Collections.Generic;

namespace ITLearning.Contract.Providers
{
    public interface INewsProvider
    {
        NewsData GetById(string id);
        IEnumerable<NewsData> GetAll();
        IEnumerable<NewsData> GetAllWithoutContent();
    }
}