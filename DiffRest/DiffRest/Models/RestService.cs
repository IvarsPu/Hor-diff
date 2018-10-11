using System;

namespace Models
{
    [Serializable]
    public class RestService
    {
        [Serializable]
        public enum ServiceLoadStatus
        {
            NotLoaded,
            Loaded,
            LoadedWithErrors,
            Failed
        }

        public string Href { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Filepath { get; set; }

        public ServiceLoadStatus LoadStatus { get; set; } = ServiceLoadStatus.NotLoaded;
    }
}