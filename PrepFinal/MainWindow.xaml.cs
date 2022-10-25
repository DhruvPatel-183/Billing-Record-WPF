using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrepFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetAllData();

            GetAttorneys();

        }
        void GetAttorneys()
        {
            using(var context = new LawFirmDBEntities())
            {
                var attorneys = from s in context.Attorneys
                                select s.LastName;

                cmbAttorney.ItemsSource = attorneys.ToList();
                //cmbAttorney.DisplayMemberPath = "LastName";
                //cmbAttorney.SelectedValuePath = "AttorneyID";
            }
        }
        void GetAllData()
        {
            using (var context = new LawFirmDBEntities())
            {
                var bills = from s in context.Billings
                            join att in context.Attorneys on s.AttorneyID equals att.AttorneyID
                            join cli in context.Clients on s.ClientID equals cli.ClientID
                            join rat in context.Rates on s.RateID equals rat.RateID
                            select new
                            {
                                BillingId = s.BillingID,
                                Date = s.Date,
                                Rate = rat.Rate1,
                                ClientFirstName = cli.FirstName,
                                ClientLastName = cli.LastName,
                                AttorneyFirstName = att.FirstName,
                                AttorneyLastName = att.LastName
                            };
                
                grdLawFirm.ItemsSource = bills.ToList();
            }
        }

        private void cmbAttorney_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAttorney.SelectedIndex >= 0)
            {
                //get selected attorney from combobox
                string selectedAttorney = cmbAttorney.SelectedItem.ToString();
                //find selected attorney
                using(var context = new LawFirmDBEntities())
                {
   

                    var attoryneyList = from s in context.Billings
                                join att in context.Attorneys on s.AttorneyID equals att.AttorneyID
                                join cli in context.Clients on s.ClientID equals cli.ClientID
                                join rat in context.Rates on s.RateID equals rat.RateID
                                where att.LastName == selectedAttorney
                                select new
                                {
                                    BillingId = s.BillingID,
                                    Date = s.Date,
                                    Rate = rat.Rate1,
                                    ClientFirstName = cli.FirstName,
                                    ClientLastName = cli.LastName,
                                    AttorneyFirstName = att.FirstName,
                                    AttorneyLastName = att.LastName
                                };


                    grdLawFirm.ItemsSource = attoryneyList.ToList();



                }
            }

        }

        private void btnGetAllBills_Click(object sender, RoutedEventArgs e)
        {
            GetAllData();

        }

        private void btnSearchClient_Click(object sender, RoutedEventArgs e)
        {
            string name = txtClientSearch.Text;

            using(var context = new LawFirmDBEntities())
            {
                
                if(name == null)
                {
                    MessageBox.Show("Enter Valid Client Name!!");
                }
                else
                {

                    var clientList = from s in context.Billings
                                        join att in context.Attorneys on s.AttorneyID equals att.AttorneyID
                                        join client in context.Clients on s.ClientID equals client.ClientID
                                        join rat in context.Rates on s.RateID equals rat.RateID
                                        where client.LastName.Contains(name)  || client.FirstName.Contains(name)
                                        select new
                                        {
                                            BillingId = s.BillingID,
                                            Date = s.Date,
                                            Rate = rat.Rate1,
                                            ClientFirstName = client.FirstName,
                                            ClientLastName = client.LastName,
                                            AttorneyFirstName = att.FirstName,
                                            AttorneyLastName = att.LastName
                                        };


                    grdLawFirm.ItemsSource = clientList.ToList();

                }
            }
           

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            cmbAttorney.SelectedIndex = -1;
            txtClientSearch.Text = "";
            txtId.Text = "";
            txtName.Text = "";
            txtCategory.Text = "";
            txtAttorney.Text = "";
            txtFee.Text = "";
            strtDate.Text = "";
            endDate.Text = "";
            GetAllData();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(txtId.Text);
            using(var context= new LawFirmDBEntities())
            {
                Billing bill = context.Billings.Find(id);
                
                if (bill == null)
                {

                    MessageBox.Show("Enter A valid Bill ID!");
                }
                else
                {
                    var bills = (from s in context.Billings
                               join att in context.Attorneys on s.AttorneyID equals att.AttorneyID
                               join client in context.Clients on s.ClientID equals client.ClientID
                               join rat in context.Rates on s.RateID equals rat.RateID
                               join cat in context.Categories on s.CategoryID equals cat.CategoryID
                               where s.BillingID == bill.BillingID
                               select new
                               {
                                   BillingId = s.BillingID,
                                   Date = s.Date,
                                   Rate = rat.Rate1,
                                   Hours = s.Hours,
                                   Category = cat.Category1,
                                   ClientFirstName = client.FirstName,
                                   ClientLastName = client.LastName,
                                   AttorneyFirstName = att.FirstName,
                                   AttorneyLastName = att.LastName
                               }).FirstOrDefault();

                   
                    txtName.Text = bills.ClientFirstName + " " + bills.ClientLastName;
                    txtCategory.Text = bills.Category;
                    txtAttorney.Text = bills.AttorneyFirstName + " " + bills.AttorneyLastName;
                    double hours = (double)bills.Hours;
                    decimal rate = (decimal)bills.Rate;
                    //calculating the fee
                    double fee = hours * (double)rate;
                    txtFee.Text = fee.ToString("C");

                    

                }

                

            }


        }

        //code taken from https://stackoverflow.com/questions/7978249/date-formatting-in-wpf-datagrid
        private void grdLawFirm_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy";
            }
        }

        private void btnFindBillsByDate_Click(object sender, RoutedEventArgs e)
        {

            if (strtDate != null || endDate != null)
            {
                DateTime startDate = strtDate.SelectedDate.Value;
                DateTime edDate = endDate.SelectedDate.Value;

                using (var context = new LawFirmDBEntities())
                {

                    
     

                        var clientList = from s in context.Billings
                                         join att in context.Attorneys on s.AttorneyID equals att.AttorneyID
                                         join client in context.Clients on s.ClientID equals client.ClientID
                                         join rat in context.Rates on s.RateID equals rat.RateID
                                         where s.Date >= startDate & s.Date <= edDate
                                         select new
                                         {
                                             BillingId = s.BillingID,
                                             Date = s.Date,
                                             Rate = rat.Rate1,
                                             ClientFirstName = client.FirstName,
                                             ClientLastName = client.LastName,
                                             AttorneyFirstName = att.FirstName,
                                             AttorneyLastName = att.LastName
                                         };


                        grdLawFirm.ItemsSource = clientList.ToList();

                    
                }
            }
            else
            {
                MessageBox.Show("Please Enter Start date and End date.");
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(txtId.Text);


            using (var context = new LawFirmDBEntities())
            {
                Billing bill = context.Billings.Find(id);
                if(bill == null)
                {
                    MessageBox.Show("Enter Valid ID");
                }
                else
                {
                    context.Billings.Remove(bill);
                    context.SaveChanges();
                    
                }
            }
        }
    }
}
