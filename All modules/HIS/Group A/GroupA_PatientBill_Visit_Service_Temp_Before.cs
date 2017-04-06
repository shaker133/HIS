﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace HIS
{
    
    public partial class GroupA_PatientBill_Visit_Service_Temp_Before : UserControl
    {
        int User_ID;
        Connection conn;
        SqlDataReader dr;
        string query;
        string EntityID;      //كود الجهة
        string BranchEntityID;//كود الجهة الفرعية
        string GroupID;       //كود الفئة
        string RegulationID;  //كود الائحة

        int Patient_service_percentage = 0;
        int Patient_medicine_percentage = 0;
        int Patient_residence_percentage = 0;

        int Entity_service_percentage = 0;
        int Entity_medicine_percentage = 0;
        int Entity_residence_percentage = 0;

        string VISIT_ID;

        public GroupA_PatientBill_Visit_Service_Temp_Before(int uid)
        {
            InitializeComponent();
            conn = new Connection();
            User_ID = uid;
        }
		
        private void calculate()
        {

            try
            {
                int num1 = dataGridView1.Rows.Count;
                double total1 = 0.0;
                for (int i = 0; i < num1; i++)
                {
                    total1 += Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value.ToString());
                }
                label11.Text = total1.ToString();
                label3.Text = (Patient_service_percentage * total1 / 100).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView_MouseEnter(object sender, EventArgs e)
        {
            DataGridView dv = (DataGridView)sender;
            dv.Focus();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 5)
            {
                var dialogResult = MessageBox.Show("  هل تريد حذف الخدمة المحددة من حساب المريض ؟", string.Empty, MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        conn.OpenConection();
                        query = "DELETE FROM Visit_Services_Temp WHERE ID=" + dataGridView1.Rows[e.RowIndex].Cells[0].Value + "";
                        conn.ExecuteQueries(query);
                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                        calculate();
                        conn.CloseConnection();
                        MessageBox.Show("تم حذف الخدمة المحددة من حساب المريض");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

                else
                {
                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                }
            }
        }

        private void fill_data()
        {
			if (VISIT_ID == "" || VISIT_ID == null)
            {
                return;
            }
            try
            {
                dataGridView1.Rows.Clear();
                conn.OpenConection();
                query = @"SELECT Registeration_patientRegisteration.patient_name, entranceoffice_visit.entrance_date, entranceoffice_visit.type_of_visit, tb_Entities_Category.EC_id, tb_Entities_Branches.EB_id, tb_Contracting_Entities.CE_Id
                        FROM  tb_Entities_Branches INNER JOIN
                        tb_Contracting_Entities ON tb_Entities_Branches.EB_CE_id = tb_Contracting_Entities.CE_Id INNER JOIN
                        tb_Entities_Category ON tb_Entities_Branches.EB_id = tb_Entities_Category.EC_EB_id INNER JOIN
                        entranceoffice_visit INNER JOIN
                        Registeration_patientRegisteration ON entranceoffice_visit.pat_id = Registeration_patientRegisteration.patient_id ON tb_Entities_Category.EC_id = entranceoffice_visit.EC_id 
                        AND entranceoffice_visit.visit_id=" + VISIT_ID + "";
                dr = conn.DataReader(query);
                if (dr.Read())
                {
                    label7.Text = dr[0].ToString();
                    label12.Text = dr[2].ToString();
                    label14.Text = dr[1].ToString();

                    EntityID = dr[5].ToString();      //كود الجهة
                    BranchEntityID = dr[4].ToString();//كود الجهة الفرعية
                    GroupID = dr[3].ToString();       //كود الفئة

                }
                dr.Close();

                query = "SELECT  CE_RP_id FROM tb_Contracting_Entities WHERE CE_Id='" + EntityID + "'";
                dr = conn.DataReader(query);
                if (dr.Read())
                {
                    RegulationID = dr[0].ToString();
                }
                dr.Close();

                query = @"SELECT  EC_Service_Contribution,  EC_Drugs_Contribution, EC_Accomodation_Contribution, EC_Service_Disprecent, EC_Drugs_Disprecent, EC_Accomodation_Disprecent
                      FROM tb_Entities_Category WHERE EC_Id='" + GroupID + "'";
                dr = conn.DataReader(query);
                if (dr.Read())
                {
                    Patient_service_percentage = Convert.ToInt32(dr[0]);
                    Patient_medicine_percentage = Convert.ToInt32(dr[1]);
                    Patient_residence_percentage = Convert.ToInt32(dr[2]);

                    Entity_service_percentage = Convert.ToInt32(dr[3]);
                    Entity_medicine_percentage = Convert.ToInt32(dr[4]);
                    Entity_residence_percentage = Convert.ToInt32(dr[5]);
                }
                dr.Close();
                dataGridView1.Rows.Clear();

                query = @"SELECT   Visit_Services_Temp.ID,  tb_Second_Level_Service.SLS_Aname,(" + Entity_service_percentage + @" * tb_Service_Pricing.SP_Service_Price / 100), Visit_Services_Temp.date, employee.name
                    FROM Visit_Services_Temp,tb_Second_Level_Service,tb_Service_Pricing,employee,Users WHERE 
                    Visit_Services_Temp.visit_ID=" + VISIT_ID + @" AND
                    Visit_Services_Temp.SLS_id = tb_Second_Level_Service.SLS_id AND
                    tb_Second_Level_Service.SLS_id=tb_Service_Pricing.SP_SLS_id AND 
                    tb_Service_Pricing.SP_RP_id=" + RegulationID + @" AND
                    Users.emp_id = employee.emp_id AND
                    Visit_Services_Temp.User_code=Users.User_Code";
                dr = conn.DataReader(query);
                while (dr.Read())
                {
                    dataGridView1.Rows.Add(dr[0], dr[1], dr[2], dr[3], dr[4],"حذف");
                }
                dr.Close();
                calculate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.CloseConnection();
            }
        }

        private void INT_TextChanged(object sender, EventArgs e)
        {
            TextBox TXB = (TextBox)sender;
            string T = "";
            foreach (char c in TXB.Text)
            {
                if ((int)c >= 48 && (int)c <= 57)
                {
                    T += c;
                }
            }
            TXB.Text = T;
            TXB.SelectionStart = TXB.TextLength;
            TXB.SelectionLength = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var forms = Application.OpenForms.Cast<Form>().ToArray();
            foreach (Form tempForm in forms)
            {
                if (tempForm.Name == "GroupA_PatientBill_Visit_Patients")
                {
                    tempForm.Hide();
                }
            }
            var fo = new GroupA_PatientBill_Visit_Patients(conn);
            fo.FormClosed += new FormClosedEventHandler(fo_FormClosed);
            fo.Show();
        }

        void fo_FormClosed(object sender, FormClosedEventArgs e)
        {
            VISIT_ID = conn.PATIENT_BILL_VID;
            label6.Text = VISIT_ID;
            fill_data();
        }
    }
}