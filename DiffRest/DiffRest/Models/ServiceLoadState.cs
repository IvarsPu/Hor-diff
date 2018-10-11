using System;
using System.Collections.Generic;
using static Models.RestService;

namespace Models
{
    [Serializable]
    public class ServiceLoadState
    {
        public List<RestService> Services { get; set; }

        public int NotLoaded { get; set; } = 0;

        public int Loaded { get; set; } = 0;

        public int LoadedWithErrors { get; set; } = 0;

        public int Failed { get; set; } = 0;

        public int PendingLoadServices
        {

            get
            {
                return this.NotLoaded + this.LoadedWithErrors + this.Failed;
            }
        }

        public void CalcStatistics()
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