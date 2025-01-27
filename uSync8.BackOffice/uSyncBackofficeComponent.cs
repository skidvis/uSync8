﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.JavaScript;

using uSync8.BackOffice.Configuration;
using uSync8.BackOffice.Controllers;
using uSync8.BackOffice.Services;
using uSync8.BackOffice.SyncHandlers;

namespace uSync8.BackOffice
{
    public class uSyncBackofficeComponent : IComponent
    {
        private readonly IProfilingLogger logger;
        private readonly SyncHandlerCollection syncHandlers;

        private readonly SyncFileService syncFileService;
        private readonly uSyncSettings globalSettings;
        private readonly uSyncService uSyncService;
        private readonly IRuntimeState runtimeState;

        public uSyncBackofficeComponent(
            SyncHandlerCollection syncHandlers,
            IProfilingLogger logger,
            SyncFileService fileService,            
            uSyncService uSyncService,
            IRuntimeState runtimeState)
        {
            globalSettings = Current.Configs.uSync();

            this.runtimeState = runtimeState;
            this.syncHandlers = syncHandlers;
            this.logger = logger;

            this.syncFileService = fileService;
            this.uSyncService = uSyncService;

        }

        public void Initialize()
        {
            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;

            if (runtimeState.Level < RuntimeLevel.Run)
            {
                logger.Info<uSyncBackofficeComponent>("Umbraco is not in Run Mode {0} so uSync is not going to run", runtimeState.Level);
                return;
            }

            if (runtimeState.ServerRole == Umbraco.Core.Sync.ServerRole.Replica)
            {
                logger.Info<uSyncBackofficeComponent>("This is a replica server, uSync will not run any of the startup events");
                return;
            }

            using (logger.DebugDuration<uSyncBackofficeComponent>("uSync Starting"))
            {
                InitBackOffice();
            }

        }

        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            if (HttpContext.Current == null)
                throw new InvalidOperationException("This method requires that an HttpContext be active");

            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            e.Add("uSync", new Dictionary<string, object>
            {
                { "uSyncService", urlHelper.GetUmbracoApiServiceBaseUrl<uSyncDashboardApiController>(controller => controller.GetApi()) }
            });
        }

            private void InitBackOffice()
        {
            if (globalSettings.ExportAtStartup || (globalSettings.ExportOnSave && !syncFileService.RootExists(globalSettings.RootFolder)))
            {
                uSyncService.Export(globalSettings.RootFolder);
            }

            if (globalSettings.ImportAtStartup)
            {
                uSyncService.Import(globalSettings.RootFolder,false);
            }

            if (globalSettings.ExportOnSave)
            {
                var handlers = syncHandlers.GetValidHandlers("Save", globalSettings);
                foreach (var syncHandler in handlers)
                {
                    logger.Debug<uSyncBackofficeComponent>($"Starting up Handler {syncHandler.Handler.Name}");
                    syncHandler.Handler.Initialize(syncHandler.Settings);
                }
            }

        }

        public void Terminate()
        {
            logger.Debug<uSyncBackofficeComponent>("Terminiating Component");
        }
    }
}
