using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UltimateTiming.DomainModel
{
    public abstract class RaceObject : INotifyPropertyChanged
    {
        private bool _isThisDirty;

        public bool IsThisDirty
        {
            get { return _isThisDirty; }
            set
            {
                if (value != _isThisDirty)
                {
                    _isThisDirty = value;
                    NotifyPropertyChanged();
                    if (!_isThisDirty)
                    { //Clear this out if the IsThisDirty Flag is set to false
                        _changedFields.Clear();
                    }
                }
            }
        }

        private bool _isNew;

        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (value != _isNew)
                {
                    _isNew = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _deleted;

        public bool Deleted
        {
            get { return _deleted; }
            private set
            {
                if (value != _deleted)
                {
                    _deleted = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    SetPropertyChanged();
                }
            }

        }

        public abstract bool IsDirty();

        public RaceObject()
        {
            this.IsNew = true;
        }

        public RaceObject(string id)
        {
            this.Id = id;
            this.IsNew = false;
        }


        private Stack<string> _changedFields = new Stack<string>();

        public ReadOnlyCollection<string> ChangedFields
        {
            get { return new ReadOnlyCollection<string>(_changedFields.ToList()); }
        }



        protected void SetPropertyChanged([CallerMemberName] string propertyName = "")
        {
            _isThisDirty = true;
            NotifyPropertyChanged(propertyName);

            _changedFields.Push(propertyName);
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
