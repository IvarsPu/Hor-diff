using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "Process")]
    public class Process
    {
        public Process(int processId, int metadataServiceId, DateTime startTime)
        {
            Id = processId;
            MetadataServiceId = metadataServiceId;
            StartTime = startTime;
            
            Token = TokenSource.Token;
        }


        [DataMember(Name = "Id")]
        [DisplayName("Procesa id")]
        public int Id { get; set; } = 0;

        [DataMember(Name = "ProfileId")]
        [DisplayName("Profila id")]
        public int MetadataServiceId { get; set; } = 0;

        [DataMember(Name = "Version")]
        [DisplayName("Versija")]
        public string Version { get; set; } = "";

        [DataMember(Name = "Release")]
        [DisplayName("Laidiens")]
        public string Release { get; set; } = "";

        [DataMember(Name = "StartTime")]
        [DisplayName("Sākuma laiks")]
        public DateTime StartTime { get; set; } = new DateTime();

        [DataMember(Name = "EndTime")]
        [DisplayName("Beigu laiks")]
        public DateTime EndTime { get; set; } = new DateTime();

        [DataMember(Name = "Done")]
        [DisplayName("Pabeigts")]
        public bool Done { get; set; } = false;

        [DataMember(Name = "Status")]
        [DisplayName("Status")]
        public Status Status { get; set; } = new Status();

        [DataMember(Name = "Progress")]
        [DisplayName("Progress")]
        public int Progress { get; set; } = 0;

        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        public CancellationToken Token { get; set; } = new CancellationToken();
    }

    [DataContract(Name = "Status")]
    public class Status
    {
        [DataMember(Name = "Text")]
        [DisplayName("Teksts")]
        public string Text { get; set; } = "";

        [DataMember(Name = "Total")]
        [DisplayName("Kopā")]
        public int Total { get; set; } = 0;

        [DataMember(Name = "Loaded")]
        [DisplayName("Ielādēti")]
        public int Loaded { get; set; } = 0;

        [DataMember(Name = "Failed")]
        [DisplayName("Neizdevušies")]
        public int Failed { get; set; } = 0;
    }
}