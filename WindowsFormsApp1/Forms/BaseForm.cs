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

        private readonly List<System.Threading.Timer> _timerThreads = new List<System.Threading.Timer>();

        public void RunInAnotherThread(Action task, Action callback = null)
        {
            Task.Factory.StartNew(() =>
            {
                task();
            }).ContinueWith(t => callback?.Invoke());
        }

        public void PefromTaskEveryXTime(Action task, int seconds)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(seconds);

            var timer = new System.Threading.Timer((e) =>
            {
                try
                {
                    task();
                }
                catch (Exception ex)
                {
                    log.Debug($"Error in executing task: {task.GetType().FullName} with the following exception: {ex.Message}");
                }
            }, null, startTimeSpan, periodTimeSpan);

            _timerThreads.Add(timer);
        }

        public void CloseAllThreads()
        {
            _timerThreads.ForEach(timer => timer.Dispose());
        }

        public BaseForm()
        {
            CreateHandles();
        }
    }
}
