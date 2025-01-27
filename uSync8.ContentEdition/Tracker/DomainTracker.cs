﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using uSync8.Core.Serialization;
using uSync8.Core.Tracking;

namespace uSync8.ContentEdition.Tracker
{
    public class DomainTracker : SyncBaseTracker<IDomain>, ISyncTracker<IDomain>
    {
        public DomainTracker(ISyncSerializer<IDomain> serializer) : base(serializer)
        {
        }

        protected override TrackedItem TrackChanges()
        {
            return new TrackedItem(serializer.ItemType, true)
            {
                Children = new List<TrackedItem>()
                {
                    new TrackedItem("Language", "/Language", true),
                    new TrackedItem("Content Root", "/Root", true)
                }
            };
        }
    }
}
