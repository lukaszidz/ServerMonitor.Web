﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml.Linq;
using LiteDB;
using Microsoft.Web.Administration;
using ServerMonitor.Helpers;
using ServerMonitor.Models;

namespace ServerMonitor.Controllers
{
    public class IisController : BaseApi
    {
        private static string DbPath => HostingEnvironment.MapPath("~/App_Data/ServerMonitor.db");

        [HttpGet]
        public Response Get()
        {
            var response = new Response();
            try
            {
                Log.Debug("GetFilteredApps called.");
                var filteredApps = GetFilteredApps();
                Log.Debug("GetFilteredApps call success.");

                response.Data = filteredApps;
                return response;
            }
            catch (Exception ex)
            {
                response.AddErrorNotification(ex.Message,ex.StackTrace);
                return response;
            }
        }

        private List<IISApplication> GetFilteredApps()
        {
            var mgr = new ServerManager();
            var iis = mgr.Sites[0].Applications;
            var appPools = mgr.ApplicationPools.Select(x => new IISAppPool
            {
                Name = x.Name,
                Running = x.State == ObjectState.Started,
                Apps = iis.Where(a => a.ApplicationPoolName == x.Name)
                    .Select(s => s.Path.Replace("/", string.Empty)).ToList()
            }).ToList();


            var applications = GroupAppPools(appPools);

            var ignoreList = ConfigurationManager.AppSettings["IISIgnoreList"].Split('|');
            var filteredApps = applications.Where(a => !ignoreList.Contains(a.Name)).ToList();


            var buildsNode = GetWhitelistItems();
            if (buildsNode != null)
            {
                filteredApps.ForEach(ap => ap.Whitelisted = IsWhiteListed(ap.ApplicationPools, buildsNode));
            }

            filteredApps.ForEach(ap => ap.Note = GetBuildNote(ap.Name));
            return filteredApps;
        }

        private static List<IISApplication> GroupAppPools(List<IISAppPool> appPools)
        {
            var applications = new List<IISApplication>();
            var regexString = ConfigurationManager.AppSettings["IISAppPoolRegex"];
            var regex = new Regex(regexString);

            var appRoot = ConfigurationManager.AppSettings["AppRootUrl"].EnsureSlash();

            foreach (var appPool in appPools)
            {
                var matches = regex.Match(appPool.Name);
                if (matches.Success)
                {
                    var name = string.Empty;

                    for (var i = 1; i < matches.Groups.Count; i++)
                    {
                        name = matches.Groups[i].Value;
                        if (!string.IsNullOrEmpty(name)) break;
                    }

                    if (string.IsNullOrEmpty(name)) continue;

                    var application = applications.FirstOrDefault(a => a.Name == name);
                    if (application == null)
                    {
                        application = new IISApplication
                        {
                            Name = name,
                            Url = $"{appRoot}{name}/",
                        };
                        applications.Add(application);
                    }

                    application.ApplicationPools.Add(appPool);
                }
            }

            return applications;
        }

        private static XElement GetWhitelistItems()
        {
            var whitelistFile = ConfigurationManager.AppSettings["WhitelistXmlPath"];
            if (!System.IO.File.Exists(whitelistFile)) return null;
            var whitelist = XDocument.Load(whitelistFile);

            var buildsNode = whitelist.Descendants("builds").First();
            return buildsNode;
        }

        private bool IsWhiteListed(IList<IISAppPool> applicationPools, XElement buildsNode)
        {
            return applicationPools.All(appPool => buildsNode.Descendants("build")
                .Any(n => n.Descendants("app").Any(a => a.Attribute("value")?.Value == appPool.Name)));
        }

        [HttpPost]
        [Route("Iis/Toggle")]
        public Response Toggle(IssToggleConfig issToggleConfig)
        {
            var response= new Response();
            try
            {
                var mgr = new ServerManager();
                var pools = mgr.ApplicationPools.Where(app => issToggleConfig.AppPools.Contains(app.Name));

                foreach (var pool in pools)
                {
                    var newState = issToggleConfig.Condition ? pool.Stop() : pool.Start();
                }

                var state = issToggleConfig.Condition ? "stopped" : "started";
                response.AddSuccessNotification($"Application pools {state} successfuly.");
                return response;
            }
            catch (Exception ex)
            {
                response.AddErrorNotification(ex.Message,ex.StackTrace);
                return response;
            }

        }

        [HttpPost]
        public Response WhitelistToggle(IssToggleConfig issToggleConfig)
        {
            var response = new Response();
            try
            {
                var whitelistFile = ConfigurationManager.AppSettings["WhitelistXmlPath"];
                var whitelist = XDocument.Load(whitelistFile);

                var buildsNode = whitelist.Descendants("builds").First();

                if (!issToggleConfig.Condition)
                {
                    var build = new XElement("build");

                    foreach (var appPool in issToggleConfig.AppPools)
                    {
                        var poolElement = new XElement("app");
                        poolElement.SetAttributeValue("value", appPool);
                        build.Add(poolElement);
                    }

                    buildsNode.Add(build);
                }
                else
                {
                    foreach (var appPool in issToggleConfig.AppPools)
                    {
                        var builds = buildsNode.Descendants("build").Where(n => n.Descendants("app").Any(a => a.Attribute("value")?.Value == appPool)).ToList();
                        foreach (var build in builds)
                        {
                            build.Descendants("app").Where(a => a.Attribute("value")?.Value == appPool).Remove();
                            if (!build.HasElements) build.Remove();
                        }
                    }
                }

                using (var file = new StreamWriter(whitelistFile))
                {
                    whitelist.Save(file);
                }

                //if (application.Name.EndsWith("-O"))
                //{
                //    var oracleInstances = GetAllOracleInstances();
                //    var instance = oracleInstances.FirstOrDefault(i => i.CurrentBuildName == application.Name.Replace("-O", ""));
                //    if (instance != null)
                //    {
                //        SetReserved(new OracleInstanceReservationRequest
                //        {
                //            Id = instance.Id,
                //            Reserve = true
                //        });
                //    }
                //}
                var function = issToggleConfig.Condition ? "un" : "";
                response.AddSuccessNotification($"Application {function}whitelisted successfuly.");
                return response;
            }
            catch (Exception ex)
            {
                response.AddErrorNotification(ex.Message, ex.StackTrace);
                return response;
            }

        }


        protected string GetBuildNote(string name)
        {
            using (var db = new LiteDatabase(DbPath))
            {
                var col = db.GetCollection<BuildNote>("BuildNotes");

                var note = col.Find(c => c.BuildName == name).FirstOrDefault();
                return note?.Note;
            }
        }

        protected void SetBuildNote(string buildName, string note)
        {
            using (var db = new LiteDatabase(DbPath))
            {
                var col = db.GetCollection<BuildNote>("BuildNotes");

                var buildNote = col.Find(c => c.BuildName == buildName).FirstOrDefault();

                if (buildNote == null)
                {
                    col.Insert(new BuildNote
                    {
                        Id = new Guid(),
                        BuildName = buildName,
                        Note = note
                    });
                }
                else
                {
                    buildNote.Note = note;
                    col.Update(buildNote);
                }

                col.EnsureIndex(x => x.BuildName);
            }
        }

        [HttpPost]
        [Route("Iis/SaveBuildNote")]
        public Response SaveBuildNote([FromBody]Data<string> data)
        {
            var response = new Response();
            try
            {
                SetBuildNote(data.Name, data.Value);
                response.AddSuccessNotification("Application note saved succesfully.");
                return response;
            }
            catch (Exception ex)
            {
                response.AddErrorNotification(ex.Message, ex.StackTrace);
                return response;
            }
        }

        [HttpGet]
        public Response GetBuildNotes(string name)
        {
            var response = new Response();
            try
            {
                var buildNote = GetBuildNote(name);
                response.Data = buildNote;
                return response;
            }
            catch (Exception ex)
            {
                response.AddErrorNotification(ex.Message, ex.StackTrace);
                return response;
            }
        }

    }
}