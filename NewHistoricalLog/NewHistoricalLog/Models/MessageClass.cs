using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog.Models
{

    public class MessageClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private int id;
        private string text;
        private DateTime date;
        private string kvited;
        private MessagePriorityEnum priority;
        private string messageValue;
        private string source;
        private string user;

        /// <summary>
        /// Идентификатор сообщений
        /// </summary>
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        /// <summary>
        /// Текст сообщений
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }
        /// <summary>
        /// Дата сообщения
        /// </summary>
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                OnPropertyChanged("Date");
            }
        }
        /// <summary>
        /// Отметка квитирования
        /// </summary>
        public string Kvited
        {
            get { return kvited; }
            set
            {
                kvited = value;
                OnPropertyChanged("Kvited");
            }
        }
        /// <summary>
        /// Приоритет сообщения
        /// </summary>
        public MessagePriorityEnum Priority
        {
            get { return priority; }
            set
            {
                priority = value;
                OnPropertyChanged("Priority");
            }
        }
        /// <summary>
        /// Значение в сообщении
        /// </summary>
        public string MessageValue
        {
            get { return messageValue; }
            set
            {
                messageValue = value;
                OnPropertyChanged("MessageValue");
            }
        }
        /// <summary>
        /// Источник сообщения
        /// </summary>
        public string Source
        {
            get { return source; }
            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }
        /// <summary>
        /// Пользователь, создавший сообщение
        /// </summary>
        public string User
        {
            get { return user; }
            set
            {
                user = value;
                OnPropertyChanged("User");
            }
        }
    }
}
