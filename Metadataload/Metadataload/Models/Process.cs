using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
            
            Token = TokenSource.Token;
        }

        [DataMember(Name = "StartTime")]
        public DateTime StartTime { get; set; } = new DateTime();

        [DataMember(Name = "EndTime")]
        public DateTime EndTime { get; set; } = new DateTime();

        [DataMember(Name = "Done")]
        public bool Done { get; set; } = false;

        [DataMember(Name = "Progress")]
        public int Progress { get; set; } = 0;

        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        public CancellationToken Token { get; set; } = new CancellationToken();
    }
}