using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Laba2.Runner.Utils;

namespace Laba2.Runner.Infrastructure.TreeViewItems
{
    public class FirstItem : BindableBase, ITreeView
    {
        public FirstItem()
        {
            TreeViews = new ObservableCollection<ITreeView>();
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<ITreeView> TreeViews { get; set; }
    }
}
