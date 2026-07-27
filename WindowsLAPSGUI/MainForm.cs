using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management.Automation;
using System.Collections;
using System.Web;
using System.DirectoryServices.AccountManagement;
using System.Collections.ObjectModel;

namespace WindowsLAPSGUI
{
    public partial class MainForm : Form
    {
        public class ComputerModel
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public string PasswordUpdateTime { get; set; } = string.Empty;
            public string ExpirationTimestamp { get; set; } = string.Empty;
            public string Source { get; set; }
        }
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!CheckPrerequisites())
            {
                MessageBox.Show(
                    "A required prerequisite is missing. The application will now exit.",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Close the form and exit the application cleanly
                this.Close();
                Application.Exit();
            }
        }

        private bool CheckPrerequisites()
        {

            bool PreReqPass = true;
            
            // PowerShell version 5.1
            Collection<PSObject> psVersion = PowerShell.Create().AddScript("$PSVersionTable.PSVersion.ToString()").Invoke();
            if (!(psVersion.First().ToString().StartsWith("5.1")))
            {
                MessageBox.Show("Windows PowerShell version 5.1 not found.", "Prerequisite Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PreReqPass = false;
            }

            // Windows LAPS cmdlets (included in the April 2023 CUs)
            // LAPS cmdlets are only available in 64-bit mode, so the application build platform target must be set to "x64" to work
            Collection<PSObject> lapsModule = PowerShell.Create().AddScript("Get-Module -Name Laps -ListAvailable").Invoke();
            if (!(lapsModule.Count > 0))
            {
                MessageBox.Show("Windows LAPS PowerShell module not found.", "Prerequisite Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PreReqPass = false;
            }

            // Active Directory member
            if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName == string.Empty)
            {
                MessageBox.Show("Computer is not a member of an Active Directory domain.", "Prerequisite Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PreReqPass = false;
            }

            return PreReqPass;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            edtPassword.Clear();
            edtExpiration.Clear();
            lstPwdHistory.Items.Clear();
            btnSet.Visible = false;
            this.Refresh();
            
            if (string.IsNullOrEmpty(edtComputerName.Text))
            {
                MessageBox.Show("A computer name is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string pcName = string.Empty, domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                try
                {
                    // Search AD for the computer name provided
                    using (PrincipalContext pcd = new PrincipalContext(ContextType.Domain, domain))
                    {
                        ComputerPrincipal pc = ComputerPrincipal.FindByIdentity(pcd, IdentityType.SamAccountName, $"{edtComputerName.Text}$");
                        pcName = pc.Name.ToString().ToUpper();
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("No computer object found that matches the information provided.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!(pcName == string.Empty)) // Computer object found in AD
                {
                    try
                    {
                        List<ComputerModel> computerModel = new List<ComputerModel>();
                        using (PowerShell ps = PowerShell.Create())
                        {
                            ps.AddScript($@"Get-LapsADPassword -Identity ""{pcName}"" -AsPlainText -IncludeHistory | Select-Object ComputerName,Password,PasswordUpdateTime,ExpirationTimestamp,Source");
                            Collection<PSObject> result = ps.Invoke();
                            int count = result.Count;
                            if (result.First().Properties["Source"].Value.ToString() == "LegacyLapsCleartextPassword")  // Microsoft LAPS
                            {
                                computerModel.Add(new ComputerModel
                                {
                                    Name = result.First().Properties["ComputerName"].Value.ToString(),
                                    Password = result.First().Properties["Password"].Value.ToString(),
                                    ExpirationTimestamp = result.First().Properties["ExpirationTimestamp"].Value.ToString()
                                });
                                btnSet.Visible = true;
                                edtComputerName.Text = computerModel[0].Name;
                                edtPassword.Text = computerModel[0].Password;
                                edtExpiration.Text = computerModel[0].ExpirationTimestamp;
                            }
                            else  // Windows LAPS
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    if (result[i].Properties["Source"].Value.ToString() == "CleartextPassword" ||
                                        result[i].Properties["Source"].Value.ToString() == "EncryptedPassword")
                                    {
                                        computerModel.Add(new ComputerModel
                                        {
                                            Name = result[i].Properties["ComputerName"].Value.ToString(),
                                            Password = result[i].Properties["Password"].Value.ToString(),
                                            PasswordUpdateTime = result[i].Properties["PasswordUpdateTime"].Value.ToString(),
                                            ExpirationTimestamp = result[i].Properties["ExpirationTimestamp"].Value.ToString(),
                                            Source = result[i].Properties["Source"].Value.ToString()
                                        });
                                    }
                                    else if (result[i].Properties["Source"].Value.ToString() == "EncryptedPasswordHistory")
                                    {
                                        computerModel.Add(new ComputerModel
                                        {
                                            Name = result[i].Properties["ComputerName"].Value.ToString(),
                                            Password = result[i].Properties["Password"].Value.ToString(),
                                            PasswordUpdateTime = result[i].Properties["PasswordUpdateTime"].Value.ToString(),
                                            Source = result[i].Properties["Source"].Value.ToString()
                                        });
                                    }
                                }
                                btnSet.Visible = true;
                                edtComputerName.Text = computerModel[0].Name;
                                edtPassword.Text = computerModel[0].Password;
                                edtExpiration.Text = computerModel[0].ExpirationTimestamp;
                                for (int i = 0; i < computerModel.Count; i++)
                                {
                                    if (computerModel[i].Source.ToString() == "EncryptedPasswordHistory")
                                    {
                                        string[] row = { computerModel[i].PasswordUpdateTime, computerModel[i].Password };
                                        ListViewItem listViewItem = new ListViewItem(row);
                                        lstPwdHistory.Items.Add(listViewItem);
                                    }
                                }
                            }
                        }
                        this.Refresh();
                    }
                    catch (InvalidOperationException)
                    {
                        edtPassword.Clear();
                        edtExpiration.Clear();
                        lstPwdHistory.Items.Clear();
                        btnSet.Visible = false;
                        this.Refresh();
                        MessageBox.Show("Computer does not have a LAPS policy applied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        edtPassword.Clear();
                        edtExpiration.Clear();
                        lstPwdHistory.Items.Clear();
                        btnSet.Visible = false;
                        this.Refresh();
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            // Button should only be shown when a valid computer object is found in AD
            Collection<PSObject> setExpiryTime = PowerShell.Create().AddScript($@"Set-LapsADPasswordExpirationTime -Identity {edtComputerName.Text} -WhenEffective (Get-Date -Date ""{dteNewPwdEpiry.Text}"")").Invoke();
            if (setExpiryTime.First().Properties["Status"].Value.ToString() == "PasswordReset")
            {
                edtExpiration.Text = dteNewPwdEpiry.Text;
            }
            else
            {
                MessageBox.Show($"An error occurred while updating the password expiration time for {edtComputerName.Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
