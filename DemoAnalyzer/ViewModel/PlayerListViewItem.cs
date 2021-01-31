using DemoInfo;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DemoAnalyzer.ViewModel
{
    public class PlayerListViewItem : INotifyPropertyChanged
    {
        private string _name = "";
        private Team _team = Team.Spectate;
        private bool _dead = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlayerListViewItem(int entityID)
        {
            EntityID = entityID;
        }

        public PlayerListViewItem(int entityID, string name, Team team, bool dead) : this(entityID)
        {
            _name = name;
            _team = team;
            _dead = dead;
        }

        public int EntityID { get; }

        public string Name
        { 
            get => _name; 
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Team Team 
        { 
            get => _team;
            set
            {
                if (_team != value)
                {
                    _team = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Dead
        { 
            get => _dead;
            set
            {
                if (_dead != value)
                {
                    _dead = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Used { get; set; }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
