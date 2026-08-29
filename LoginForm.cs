using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using BloodConnect.UI.Admin;
using BloodConnect.UI.Donor;
using BloodConnect.UI.Receiver;

namespace BloodConnect
{
    public partial class LoginForm : Form
    {
        private CheckBox chkShowPassword;

        public LoginForm()
        {
            InitializeComponent();
            ApplyModernStyle();
            SetupPlaceholder(txtUsername, "Enter username");
            SetupPasswordField();
            AddShowPasswordToggle();
        }

        private void SetupPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Tag = placeholder;
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.GotFocus += (sender, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }

        private void SetupPasswordField()
        {
            txtPassword.Text = "";
            txtPassword.ForeColor = Color.Black;
            txtPassword.Tag = null;
            ApplyPasswordMask();
            txtPassword.TextChanged += (s, e) => ApplyPasswordMask();
        }

        private void ApplyPasswordMask()
        {
            bool showPlain = chkShowPassword != null && chkShowPassword.Checked;
            txtPassword.UseSystemPasswordChar = false;
            txtPassword.PasswordChar = showPlain ? '\0' : '●';
        }

        private string GetFieldValue(TextBox textBox)
        {
            if (textBox == txtPassword)
                return textBox.Text.Trim();

            string placeholder = textBox.Tag as string ?? "";
            string value = textBox.Text.Trim();
            return value == placeholder ? "" : value;
        }

        private void ApplyModernStyle()
        {
            BackColor = Color.FromArgb(245, 247, 250);
            cardPanel.BackColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            txtUsername.Font = new Font("Segoe UI", 11);
            txtPassword.Font = new Font("Segoe UI", 11);
            btnLogin.BackColor = Color.FromArgb(220, 53, 69);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnReg.LinkColor = Color.FromArgb(220, 53, 69);
        }

        private void AddShowPasswordToggle()
        {
            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                BackColor = Color.White,
                Location = new Point(txtPassword.Left, txtPassword.Bottom + 12)
            };

            chkShowPassword.CheckedChanged += (s, e) => ApplyPasswordMask();

            cardPanel.Controls.Add(chkShowPassword);
            btnLogin.Top = chkShowPassword.Bottom + 15;
            btnReg.Top = btnLogin.Bottom + 20;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = GetFieldValue(txtUsername);
            string password = GetFieldValue(txtPassword);

            if (username == "" || password == "")
            {
                MessageBox.Show("Please enter username and password.");
                return;
            }

            DataAccess da = new DataAccess();
            string query = @"SELECT ID, FullName, UserRoleID 
                             FROM Users 
                             WHERE Username = @username AND Password = @password";

            SqlParameter[] param =
            {
                new SqlParameter("@username", username),
                new SqlParameter("@password", password)
            };

            try
            {
                DataTable dt = da.ExecuteQuery(query, param);

                if (dt.Rows.Count > 0)
                {
                    int userId = Convert.ToInt32(dt.Rows[0]["ID"]);
                    string fullName = dt.Rows[0]["FullName"].ToString();
                    int userRoleId = Convert.ToInt32(dt.Rows[0]["UserRoleID"]);

                    if (userRoleId == 1)
                    {
                        AdminDashboard admin = new AdminDashboard(userId, fullName);
                        admin.Show();
                        Hide();
                    }
                    else if (userRoleId == UserRoles.Donor)
                    {
                        DonorDashboard donor = new DonorDashboard(userId, fullName);
                        donor.Show();
                        Hide();
                    }
                    else if (userRoleId == UserRoles.Receiver)
                    {
                        ReceiverDashboard receiver = new ReceiverDashboard(userId, fullName);
                        receiver.Show();
                        Hide();
                    }
                    else
                    {
                        MessageBox.Show("Unknown user role.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Username or Password");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }

        private void btnReg_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterForm regForm = new RegisterForm(this);
            regForm.Show();
            Hide();
        }
    }
}
