using System;
using System.Runtime.Serialization;
using System.Threading;

namespace Metadataload.Models
{
    [Serializable]
    [DataContract(Name = "Process")]
    public class Process
    {
        public Process(int processId, DateTime startTime, DateTime endTime, bool done, int progress)
        {
            Id = processId;
            StartTime = startTime;
            EndTime = endTime;
            Done = done;
            Progress = progress;
            
            Token = TokenSource.Token;
        }

        [DataMember(Name = "Id")]
        public int Id { get; set; } = 0;

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