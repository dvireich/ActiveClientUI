using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class BaseForm : Form
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void CreateHandles()
        {
            if (!IsHandleCreated)
            {
                CreateHandle();
            }
        }

        public void ListView_SizeChanged(object sender, EventArgs e)
        {
            if (!(sender is ListView listView)) return;

            if (listView.View == View.Details)
            {
                for (var i = 0; i < listView.Columns.Count; i++)
                {
                    listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }
        }

        public BaseForm()
        {
            CreateHandles();
        }
    }
}
