using System;
using System.Runtime.Serialization;
using System.Threading;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "Process")]
    public class Process
    {
        public Process(int processId, int serverId, DateTime startTime)
        {
            Id = processId;
            ServerId = serverId;
            StartTime = startTime;
            
            Token = TokenSource.Token;
        }

        [DataMember(Name = "Id")]
        public int Id { get; set; } = 0;

        [DataMember(Name = "UserId")]
        public int ServerId { get; set; } = 0;

        [DataMember(Name = "Version")]
        public string Version { get; set; } = "";

        [DataMember(Name = "Release")]
        public string Release { get; set; } = "";

        [DataMember(Name = "StartTime")]
        public DateTime StartTime { get; set; } = new DateTime();

        [DataMember(Name = "EndTime")]
        public DateTime EndTime { get; set; } = new DateTime();

        [DataMember(Name = "Done")]
        public bool Done { get; set; } = false;

        [DataMember(Name = "Status")]
        public Status Status { get; set; } = new Status();

        [DataMember(Name = "Progress")]
        public int Progress { get; set; } = 0;

        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        public CancellationToken Token { get; set; } = new CancellationToken();
    }

    [DataContract(Name = "Status")]
    public class Status
    {
        [DataMember(Name = "Text")]
        public string Text { get; set; } = "";

        [DataMember(Name = "Total")]
        public int Total { get; set; } = 0;

        [DataMember(Name = "Loaded")]
        public int Loaded { get; set; } = 0;

        [DataMember(Name = "Failed")]
        public int Failed { get; set; } = 0;
    }
}