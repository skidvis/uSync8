﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using uSync8.BackOffice.Configuration;
using uSync8.BackOffice.Services;
using uSync8.Core.Serialization;
using uSync8.Core.Tracking;

namespace uSync8.BackOffice.SyncHandlers.Handlers
{
    [SyncHandler("macroHandler", "Macros", "Macros", uSyncBackOfficeConstants.Priorites.Macros, Icon = "icon-settings-alt")]
    public class MacroHandler : SyncHandlerBase<IMacro, IMacroService>, ISyncHandler
    {
        private readonly IMacroService macroService;

        public MacroHandler(IEntityService entityService,
            IProfilingLogger logger, 
            IMacroService macroService,
            ISyncSerializer<IMacro> serializer,
            ISyncTracker<IMacro> tracker,
            SyncFileService syncFileService) 
            : base(entityService, logger, serializer, tracker, syncFileService)
        {
            this.macroService = macroService;
        }

        /// <summary>
        ///  overrider the default export, because macros, don't exist as an object type???
        /// </summary>
        public override IEnumerable<uSyncAction> ExportAll(int parent, string folder, HandlerSettings config, SyncUpdateCallback callback)
        {
            // we clean the folder out on an export all. 
            syncFileService.CleanFolder(folder);

            var actions = new List<uSyncAction>();

            var items = macroService.GetAll().ToList();
            int count = 0;
            foreach(var item in items)
            {
                count++;
                callback?.Invoke(item.Name, count, items.Count);
                actions.Add(Export(item, folder, config));
            }

            return actions;
        }

        protected override IMacro GetFromService(int id)
            => macroService.GetById(id);

        // not sure we can trust macro guids in the path just yet.
        protected override string GetItemPath(IMacro item, bool useGuid, bool isFlat)
        {
            if (useGuid) return item.Key.ToString();
            return item.Alias.ToSafeAlias();
        }

        protected override void InitializeEvents(HandlerSettings settings)
        {
            MacroService.Saved += EventSavedItem;
            MacroService.Deleted += EventDeletedItem;
        }

        protected override IMacro GetFromService(Guid key) 
            => null;

        protected override IMacro GetFromService(string alias) 
            => macroService.GetByAlias(alias);

        protected override void DeleteViaService(IMacro item)
            => macroService.Delete(item);

        protected override string GetItemName(IMacro item)
            => item.Name;
    }

}
