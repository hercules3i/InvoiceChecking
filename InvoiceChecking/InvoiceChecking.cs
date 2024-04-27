using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvoiceChecking
{
    public partial class InvoiceChecking : Form
    {
        private Process exeProcess;
        public InvoiceChecking()
        {
            InitializeComponent();
            string json = File.ReadAllText(@"main\\check_invoice.json");
            dataGridView1.DataSource = JsonConvert.DeserializeObject(json);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            runExe();
        }
        private async Task runExe()
        {
            if (exeProcess == null || exeProcess.HasExited)
            {

                string executablePath = @"main\\CheckInvoice.exe";
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                exeProcess = new Process { StartInfo = startInfo };
                await Task.Run(() =>
                {
                    exeProcess.Start();
                    exeProcess.WaitForExit();
                });

                exeProcess = null;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string json = File.ReadAllText(@"main\\check_invoice.json");
            dataGridView1.DataSource = JsonConvert.DeserializeObject(json);

        }
        public class InvoiceData
        {
            public string TaxNumber { get; set; }
            public string SerialNumber { get; set; }
            public string BillNumber { get; set; }
            public string Status { get; set; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog object
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            // Set initial directory (optional)
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Set filter for supported file types (optional)
            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            // Show the dialog and get user selection
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;

                // Process the selected file (e.g., read content, display message)
                MessageBox.Show("Đã load được file: " + filePath);
            }

            
            
        }
    }
}
