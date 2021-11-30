using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace QhitChat_Client.Presistent.Database.Models
{
    [DataContract]
    public class Messages : INotifyPropertyChanged
    {
        [DataMember(Order = 0)]
        [Required]
        public string From { get; set; }

        [DataMember(Order = 1)]
        [Required]
        public string To { get; set; }

        private string _content;
        [DataMember(Order = 2)]
        public string Content
        {
            get => _content;
            set => SetField(ref _content, value);
        }

        [DataMember(Order = 3)]
        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int IsSent { get; set; } // 0 - message not delivered yet; -1 - message delivered.

        [Key]
        public ulong Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Content;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
