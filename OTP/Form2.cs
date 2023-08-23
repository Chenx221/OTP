using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace OTP
{
    public partial class Form2 : Form
    {
        private string connectionString = "Data Source=key.db;Version=3;";
        private string backupPath = "key.db.bak"; // 备份数据库文件的路径
        private string sourceFileHash = ""; // 原始数据库文件的哈希值

        public Form2()
        {
            InitializeComponent();
            InitializeDataGridView();
            // 获取原始数据库文件的哈希值
            sourceFileHash = CalculateSHA256Hash("key.db");
            // 备份数据库文件并判断完整性
            bool backupSuccessful = BackupDatabase();

            if (!backupSuccessful)
            {
                // 备份创建失败，禁用按钮并提示用户
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;

                MessageBox.Show("Backup of data failed, modification is prohibited.", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadData();
        }
        private bool BackupDatabase()
        {
            try
            {
                if (File.Exists("key.db"))
                {
                    // 备份数据库文件
                    File.Copy("key.db", backupPath, true);

                    // 计算备份文件的哈希值
                    string backupFileHash = CalculateSHA256Hash(backupPath);

                    // 比较原始文件和备份文件的哈希值
                    if (sourceFileHash == backupFileHash)
                    {
                        return true; // 备份创建成功且完整性通过
                    }
                    else
                    {
                        return false; // 备份创建成功但完整性未通过
                    }
                }
                else
                {
                    return false; // 原始文件不存在，备份创建失败
                }
            }
            catch (Exception)
            {
                return false; // 备份创建失败
            }
        }
        // 计算文件的SHA-256哈希值
        private string CalculateSHA256Hash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }


        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Add("Title", "Title"); // 添加 Title 列
        }
        private void LoadData()
        {
            dataGridView1.Rows.Clear(); // 清空现有数据

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT Id, Title FROM TotpData", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string title = reader.GetString(1);
                            dataGridView1.Rows.Add(title);
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Tag = id; // 将 Id 存储在行的 Tag 属性中
                        }
                    }
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            using (Form3 form3 = new Form3())
            {
                DialogResult result = form3.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // 在 Form3 中添加数据完成后，刷新数据
                    LoadData();
                }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1 form1 = Application.OpenForms["Form1"] as Form1;
            if (form1 != null)
            {
                form1.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中的行
                foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows)
                {
                    // 获取选中行的数据
                    int id = (int)selectedRow.Tag; // 从行的 Tag 属性获取 Id 值

                    // 执行删除数据库记录的操作
                    DeleteRecordFromDatabase(id);

                    // 从 DataGridView 中移除选中行
                    dataGridView1.Rows.Remove(selectedRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the record: {ex.Message}");
            }
        }

        private void DeleteRecordFromDatabase(int recordId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM TotpData WHERE ID = @ID", connection))
                    {
                        command.Parameters.AddWithValue("@ID", recordId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the record: {ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 删除备份文件
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            // 关闭当前的 Form2 实例，返回到 Form1
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // 恢复备份文件
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, "key.db", true);

                    // 删除备份文件
                    File.Delete(backupPath);

                    MessageBox.Show("The operation was successful.", "Undo Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No backup file found to undo changes.", "Undo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while undoing changes: {ex.Message}", "Undo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 关闭当前的 Form2 实例，返回到 Form1，不进行保存操作
            this.Close();
        }

    }
}
