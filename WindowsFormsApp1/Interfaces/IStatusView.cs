
using System.Collections.Generic;
using WindowsFormsApp1.Controlers;

namespace WindowsFormsApp1.Interfaces
{
    public interface IStatusView : IView
    {
        //Properties

        string SelectedClient { get; set; }

        bool SelectedClientVisible { get; set; }

        bool NoSelectedClientLabelVisible { get; set; }

        bool ListViewVisible { get; set; }


        //Methods

        void SetController(StatusFormControler controller);
    }
}
