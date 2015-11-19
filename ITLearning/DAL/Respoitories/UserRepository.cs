﻿using AutoMapper;
using ITLearning.Frontend.Web.Common.Configs;
using ITLearning.Frontend.Web.Contract.DAL.Repositories;
using ITLearning.Frontend.Web.Contract.Data.Results;
using ITLearning.Frontend.Web.Contract.Data.Model.User;
using Microsoft.Framework.OptionsModel;
using System.Linq;
using ITLearning.Frontend.Web.Contract.Providers;
using System.IO;

namespace ITLearning.Frontend.Web.DAL.Respoitories
{
    public class UserRepository : IUserRepository
    {
        private readonly IOptions<DatabaseConfiguration> _dbConfiguration;
        private readonly IAppConfigurationProvider _configurationProvider;

        public UserRepository(IOptions<DatabaseConfiguration> dbConfiguration, IAppConfigurationProvider configurationProvider)
        {
            _dbConfiguration = dbConfiguration;
            _configurationProvider = configurationProvider;
        }

        public CommonResult<UserProfileData> GetUserProfile(string userName)
        {
            using (var context = ContextFactory.GetDbContext(_dbConfiguration))
            {
                var user = context.Users.First(x => x.UserName == userName);

                var mapped = Mapper.Map<UserProfileData>(user);

                if (!string.IsNullOrEmpty(user.ImageName))
                {
                    var croppedProfileImage = _configurationProvider.GetProfileCroppedImagesFolderPath() + user.ImageName;
                    if (File.Exists(croppedProfileImage))
                    {
                        mapped.ProfileImagePath = _configurationProvider.GetProfileCroppedImagesFolderInternalPath() + user.ImageName;
                    }
                    else
                    {
                        mapped.ProfileImagePath = _configurationProvider.GetProfileOriginalImagesFolderInternalPath() + user.ImageName;
                    }
                }
                else
                {
                    mapped.ProfileImagePath = _configurationProvider.GetProfileDefaultImagePath();
                }

                return CommonResult<UserProfileData>.Success(mapped);
            }
        }

        public CommonResult<UserProfileData> UpdateUserProfile(string userName, UserProfileData requestData)
        {
            using (var context = ContextFactory.GetDbContext(_dbConfiguration))
            {
                var user = context.Users.First(x => x.UserName == userName);
                user.FirstName = requestData.FirstName;
                user.LastName = requestData.LastName;
                user.Email = requestData.Email;

                context.SaveChanges();

                return CommonResult<UserProfileData>.Success(Mapper.Map<UserProfileData>(user));
            }
        }

        public CommonResult<UserProfileData> UpdateUserProfileImage(string userName, string fileName)
        {
            using (var context = ContextFactory.GetDbContext(_dbConfiguration))
            {
                var user = context.Users.First(x => x.UserName == userName);
                user.ImageName = fileName;

                context.SaveChanges();

                return CommonResult<UserProfileData>.Success(Mapper.Map<UserProfileData>(user));
            }
        }
    }
}