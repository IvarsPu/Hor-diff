using System;
using System.Runtime.Serialization;

namespace Metadataload.Models
{
    [Serializable]
    [DataContract(Name = "Process")]
    public class Process
    {
        public Process(DateTime startTime, DateTime endTime, bool done, int progress)
        {
            StartTime = startTime;
            EndTime = endTime;
            Done = done;
            Progress = progress;
        }

        [DataMember(Name = "StartTime")]
        public DateTime StartTime { get; set; } = new DateTime();

        [DataMember(Name = "EndTime")]
        public DateTime EndTime { get; set; } = new DateTime();

        [DataMember(Name = "Done")]
        public bool Done { get; set; } = false;

        [DataMember(Name = "Progress")]
        public int Progress { get; set; } = 0;
    }
}