using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Invoice
{
    public partial class Capcha : Form
    {
  
        private FileSystemWatcher fileWatcher;
        private DateTime lastJsonChangeTime;

        //old
        InvoiceGateAddon invoice;
        //private FileSystemWatcher fileWatcher;
        //private DateTime lastJsonChangeTime;
        String path_img = @"main\\capcha.png";
        String notification_py = @"main\\notification_py.json";
        String notification_c = @"main\\test_json\\test_json_in.json";
        public Capcha(InvoiceGateAddon i)
        {
            InitializeComponent();
            pictureBox1.ImageLocation = @"main\\capcha.png";
            invoice = i;
            label1.Text = "";
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = Path.GetDirectoryName(notification_py);
            fileWatcher.Filter = Path.GetFileName(notification_py);
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Changed += FileWatcher_Changed;
            fileWatcher.EnableRaisingEvents = true;
            this.FormClosing += Form1_FormClosing;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Please type the capcha", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                String codeCapcha = textBox1.Text;
                DateTime currentTime = DateTime.Now;
                string jsonContent = File.ReadAllText(notification_c);
                string json_py = File.ReadAllText(notification_py);

                string user_id = "A";
                int id = 0;

                try
                {
                    List<dynamic> jsonArrayObject = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

                    JArray jsonArray = JArray.Parse(jsonContent);

                    if (jsonArray.Count > 0)
                    {
                        JObject lastObject = (JObject)jsonArray[jsonArray.Count - 1];
                        id = (int)lastObject.GetValue("id") + 1;
                    }
                    var newJson = new
                    {
                        id = id,
                        user = user_id,
                        notify = codeCapcha,
                        datetime = currentTime
                    };
                    jsonArrayObject.Add(newJson);

                    string newJsonString = JsonConvert.SerializeObject(jsonArrayObject);
                    using (var fileStream = new FileStream(notification_c, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(newJsonString);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show("Lỗi định dạng tệp tin JSON.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DateTime startTime = DateTime.Now;
                bool shouldStop = false; // Thêm biến shouldStop để kiểm tra xem có nên dừng chương trình hay không

                CheckJsonNotifyImg(json_py, ref shouldStop); // Thay đổi phương thức gọi và truyền giá trị shouldStop như là một tham chiếu

                if (!shouldStop) // Kiểm tra giá trị của shouldStop
                {
                    System.Threading.Thread.Sleep(1000);
                    TimeSpan elapsed = DateTime.Now - startTime;
                    pictureBox1.ImageLocation = path_img;

                    if (elapsed.TotalSeconds >= 60)
                    {
                        MessageBox.Show("pyThon không phản hồi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

     


        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(@".\main\dist\Status.txt", string.Empty);
            File.WriteAllText(@".\main\dist\Check_Account_1.txt", string.Empty);
            this.Close();
        }


        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string jsonContent;
            bool check = false;
            using (var fileStream = new FileStream(notification_py, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    jsonContent = streamReader.ReadToEnd();
                }
            }
            if (!CheckJsonNotifyImg(jsonContent, ref check))
            {
                string latestImagePath = FindLatestImageInFolder(@"main");

                if (!string.IsNullOrEmpty(latestImagePath))
                {
                    pictureBox1.ImageLocation = latestImagePath;
                }
            }
            lastJsonChangeTime = DateTime.Now;
        }
        private string FindLatestImageInFolder(string folderPath)
        {
            string[] imageExtensions = { ".png" };
            DirectoryInfo directory = new DirectoryInfo(folderPath);
            FileInfo[] files = directory.GetFiles()
                .Where(f => imageExtensions.Contains(f.Extension.ToLower()))
                .OrderByDescending(f => f.LastWriteTime)
                .ToArray();

            if (files.Length > 0)
            {
                return files[0].FullName;
            }

            return null;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Xóa tệp tin ảnh capcha
            if (File.Exists(path_img))
            {
                File.Delete(path_img);
            }
        }
        //fix
        //bool isMessageBoxShown = false;


        private bool isMessageBoxShown = false;
        private bool CheckJsonNotifyImg(string jsonContent, ref bool  shouldStop)
        {
            try
            {
                JArray jsonArray = JArray.Parse(jsonContent);

                if (jsonArray.Count > 0)
                {
                    JObject lastObject = (JObject)jsonArray[jsonArray.Count - 1];
                    JToken notifyToken = lastObject["notify"];
                    

                    if (notifyToken != null && notifyToken.ToString() == "img")
                    {

                        pictureBox1.ImageLocation = path_img;
                        return true;

                    }
                    else if (notifyToken != null && notifyToken.ToString() == "Mã captcha không đúng.")
                    {
                        MessageBox.Show("Nhập mã sai mã capcha,vui lòng nhập lại  ");

                    }
                    if (!isMessageBoxShown && notifyToken != null && !shouldStop && notifyToken.ToString() == "over")
                    {
                        MessageBox.Show("Vượt qúa số lần nhập mã capcha, vui lòng chạy lại chương trình");
                        isMessageBoxShown = true; // Đánh dấu rằng MessageBox đã được hiển thị
                        CloseFormFromMainThread();
                    }
                    else if (notifyToken != null && notifyToken.ToString() == "correct")
                    {
                        MessageBox.Show("Đăng  nhập thành công ");
                        shouldStop = true;
                        //this.Close();
                        CloseFormFromMainThread();
                        invoice.showWaitForm();

                        return true;
                    }
                     else if (notifyToken != null && notifyToken.ToString() == "out of time")
                    {
                        MessageBox.Show("Vượt quá thời gian nhập mã capcha, vui lòng chạy lại chương trình");
                        shouldStop = true;
                        //this.Close();
                        CloseFormFromMainThread();

                        return true;
                    }

                }
            }
            catch (JsonReaderException)
            {
            }

            return false;
        }
        public void CloseFormFromMainThread()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(CloseFormFromMainThread));
                return;
            }

            this.Close();
        }

    }
}
