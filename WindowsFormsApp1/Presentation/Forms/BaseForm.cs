using System;
using System.Windows.Forms;
using WindowsFormsApp1.Core.Common.Logging;

namespace WindowsFormsApp1.Presentation.Forms
{
    public partial class BaseForm : Form
    {
        protected readonly ILogger Logger;
        
        public BaseForm()
        {
            Logger = new ConsoleLogger(GetType().Name);
        }
        
        protected void SafeInvoke(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
        
        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        protected void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        protected void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        protected bool ConfirmAction(string message)
        {
            return MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
        
        protected virtual void OnFormLoad(object sender, EventArgs e)
        {
            Logger.LogInfo("Form loaded");
        }
        
        protected virtual void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.LogInfo("Form closing");
        }
    }
}