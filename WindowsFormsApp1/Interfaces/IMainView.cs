﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public interface IMainView
    {
        //Properties
        string CurrentPathTextBoxText { get; set; }

        Font CurrentPathTextBoxFont { get; set; }

        int FormWidth { get; set; }

        bool NoSelectedClientLabelVisible { get; set; }

        bool ListViewVisible { get; set; }

        int ProgressBarValue { get; set; }

        string ProgressLabelText { get; set; }

        bool ProgressLabelVisible { get; set; }

        bool ProgressBarVisible { get; set; }

        bool ShouldChangeCurrentPathText { get; set; }

        bool EnableViewModification { get; set; }

        //Methods

        void SetController(MainFormControler controller);

        void DisplayMessage(MessageType type, string header, string message);

        void ShowData(List<FileFolder> data);
    }
}
