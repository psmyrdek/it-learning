﻿using CommonMark;
using ITLearning.Contract.Data.Model.News;
using ITLearning.Contract.Providers;
using ITLearning.Shared.Configs;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using ITLearning.Contract.Data.Requests.News;
using ITLearning.Contract.Data.Results;
using ITLearning.Contract.Data.Results.News;
using Microsoft.AspNet.Http;
using ITLearning.Contract.DataAccess.Repositories;

namespace ITLearning.Backend.DataAccess.Repositories
{
    public class StaticFilesNewsRepository : INewsRepository
    {
        private IHostingEnvironment _hostingEnvironment;
        private IOptions<PathsConfiguration> _pathsConfiguration;

        private string _newsPath;
        private string _newsImagesPath;

        public StaticFilesNewsRepository(IHostingEnvironment hostingEnvironment, IOptions<PathsConfiguration> pathsConfiguration)
        {
            _hostingEnvironment = hostingEnvironment;
            _pathsConfiguration = pathsConfiguration;

            _newsPath = _pathsConfiguration.Value.News;
            _newsImagesPath = _pathsConfiguration.Value.NewsImages;
        }

        public IEnumerable<NewsData> GetAll()
        {
            var newsList = new List<NewsData>();
            newsList.AddRange(GetAllWithoutContent());

            var fileProvider = _hostingEnvironment.WebRootFileProvider;
            var directoryContents = fileProvider.GetDirectoryContents(_newsPath).Where(x => x.Name.EndsWith("_content.md"));

            foreach (var news in newsList)
            {
                var contentFileForNews = directoryContents.Single(x => x.Name == $"{news.Id}_content.md");
                string htmlContent = GetHtmlContentFromMarkdownFile(contentFileForNews);

                news.Content = htmlContent;
            }

            return newsList;
        }

        public IEnumerable<NewsData> GetAllWithoutContent()
        {
            var newsList = new List<NewsData>();

            var fileProvider = _hostingEnvironment.WebRootFileProvider;
            var directoryContents = fileProvider.GetDirectoryContents(_newsPath).Where(x => x.Name.EndsWith(".json"));

            foreach (var directoryContent in directoryContents)
            {
                newsList.Add(GetNewsFromJsonFile(directoryContent));
            }

            return newsList;
        }

        public NewsData GetById(string id, bool contentAsHtml = true)
        {
            var fileProvider = _hostingEnvironment.WebRootFileProvider;

            var directoryContents = fileProvider.GetDirectoryContents(_newsPath);

            var newsJsonInfoFile = directoryContents.SingleOrDefault(x => x.Name == $"{id}.json");
            var newsContentFile = directoryContents.SingleOrDefault(x => x.Name == $"{id}_content.md");

            if (newsJsonInfoFile == null || newsContentFile == null)
            {
                return null;
            }

            NewsData news = GetNewsFromJsonFile(newsJsonInfoFile);
            news.Content = GetHtmlContentFromMarkdownFile(newsContentFile, contentAsHtml);

            return news;
        }

        private string GetHtmlContentFromMarkdownFile(IFileInfo contentFile, bool contentAsHtml = true)
        {
            var markdownContent = File.ReadAllText(contentFile.PhysicalPath);
            return contentAsHtml ? CommonMarkConverter.Convert(markdownContent) : markdownContent;
        }

        private NewsData GetNewsFromJsonFile(IFileInfo contentFile)
        {
            var jsonNews = File.ReadAllText(contentFile.PhysicalPath);
            var news = JsonConvert.DeserializeObject<NewsData>(jsonNews);

            return news;
        }

        public async Task SaveDataAsync(NewsData data)
        {
            var fileName = $"{data.Id}.json";
            var path = Path.Combine(_newsPath, fileName);
            data.ImagePath = _newsImagesPath + data.ImageName;

            await SaveFileAsync(path, JsonConvert.SerializeObject(data));
        }

        public async Task SaveContentAsync(NewsContentData data)
        {
            var fileName = $"{data.Id}_content.md";
            var path = Path.Combine(_newsPath, fileName);

            await SaveFileAsync(path, data.Content);
        }

        public async Task SaveFileAsync(string subPath, string content)
        {
            var rootPath = _hostingEnvironment.WebRootPath;

            var root = rootPath.Replace('\\', '/');
            var path = root + subPath;

            using (var writer = new StreamWriter(path))
            {
                await writer.WriteAsync(content);
            }
        }

        public string GetNewNewsId()
        {
            var fileProvider = _hostingEnvironment.WebRootFileProvider;
            var directoryContents = fileProvider.GetDirectoryContents(_newsPath).Where(x => x.Name.EndsWith(".json"));

            var date = DateTime.Now;

            var newsId = string.Empty;

            if (directoryContents.Any())
            {
                var lastNewsId = directoryContents.Count();

                newsId = $"{lastNewsId + 1}_{date.Year}{date.Month}{date.Day}";
            }
            else
            {
                newsId = $"1_{date.Year}{date.Month}{date.Day}";
            }

            return newsId;
        }

        public CommonResult DeleteNews(DeleteNewsRequest request)
        {
            var contentFile = $"{request.NewsId}_content.md";
            var jsonFile = $"{request.NewsId}.json";

            var rootPath = _hostingEnvironment.WebRootPath.Replace('\\', '/');

            var contentPath = rootPath + Path.Combine(_newsPath, contentFile);
            var jsonPath = rootPath + Path.Combine(_newsPath, jsonFile);

            if (File.Exists(contentPath) && File.Exists(jsonPath))
            {
                File.Delete(contentPath);
                File.Delete(jsonPath);

                return CommonResult.Success();
            }
            else
            {
                return CommonResult.Failure("News o podanym Id nie istnieje.");
            }
        }

        public async Task<SaveNewsImageResult> SaveImageAsync(IFormFile image)
        {
            if (image == null)
            {
                return new SaveNewsImageResult
                {
                    ImagePath = "default/default-news-image.jpg"
                };
            }

            var rootPath = _hostingEnvironment.WebRootPath.Replace('\\', '/') + "/static/news-images/";
            var fileName = "uploaded/" + Guid.NewGuid().ToString() + GetExtension(image);
            var path = rootPath + fileName;

            await image.SaveAsAsync(path);

            return new SaveNewsImageResult
            {
                ImagePath = fileName
            };
        }

        private string GetExtension(IFormFile image)
        {
            var contentType = image.ContentType;

            switch (contentType)
            {
                case "image/png":
                    return ".png";
                case "image/jpeg":
                    return ".jpg";
                case "image/gif":
                    return ".gif";
                default:
                    return ".jpg";
            }
        }
    }
}
