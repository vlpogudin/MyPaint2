using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_1
{
    public partial class ProgressForm: Form
    {
        private readonly CancellationTokenSource cts;

        public ProgressForm(CancellationTokenSource cts)
        {
            InitializeComponent();
            this.cts = cts;
            progressBar.Value = 0;
            progressLabel.Text = "0%";
        }

        public void UpdateProgress(int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgress), percentage);
                return;
            }
            progressBar.Value = Math.Min(percentage, 100);
            progressLabel.Text = $"{percentage}%";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            btnCancel.Enabled = false;
            progressLabel.Text = "Отмена...";
        }
    }
}
