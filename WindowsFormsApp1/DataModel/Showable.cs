using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Interfaces;

namespace WindowsFormsApp1.DataModel
{
    public  class Showable : IShowable
    {
        internal Showable(string mainItem, List<string> subItems, int imageIndex)
        {
            MainItem = mainItem;
            SubItems = subItems;
            ImageIndex = imageIndex;
        }

        public string MainItem { get; }
        public List<string> SubItems { get; }
        public int ImageIndex { get; }
    }
}
