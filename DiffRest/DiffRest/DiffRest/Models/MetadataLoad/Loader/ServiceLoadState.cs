using System;
using System.Collections.Generic;
using static DiffRest.Models.RestService;

namespace DiffRest.Models
{
    [Serializable]
    internal class ServiceLoadState
    {
        internal List<RestService> Services { get; set; }

        internal int NotLoaded { get; set; } = 0;

        internal int Loaded { get; set; } = 0;

        internal int LoadedWithErrors { get; set; } = 0;

        internal int Failed { get; set; } = 0;

        internal int PendingLoadServices
        {

            get
            {
                return this.NotLoaded + this.LoadedWithErrors + this.Failed;
            }
        }

        internal void CalcStatistics()
        {
            this.NotLoaded = 0;
            this.Loaded = 0;
            this.LoadedWithErrors = 0;
            this.Failed = 0;

            if (this.Services == null)
            {
                return;
            }

            foreach (RestService service in Services)
            {
                switch (service.LoadStatus)
                {
                    case ServiceLoadStatus.NotLoaded: this.NotLoaded++; break;
                    case ServiceLoadStatus.Loaded: this.Loaded++; break;
                    case ServiceLoadStatus.LoadedWithErrors: this.LoadedWithErrors++; break;
                    case ServiceLoadStatus.Failed: this.Failed++; break;
                }
            }
        }
    }
}