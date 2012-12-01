using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SilverlightPhoneDatabase;

namespace Sanitabit
{
    public partial class Profile : PhoneApplicationPage
    {
        
        public Profile()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
            if (Database.DoesDatabaseExists("Data"))
            {
                try
                {
                    Database database = Database.OpenDatabase("Data");
                    Table<PersonData> table = database.Table<PersonData>("login");
                    
                    Salute.Text = "Hello!";
                    PersonName.Text = table.First<PersonData>().Name.ToString();
                    Title.Visibility = Visibility.Collapsed;
                    Salutation.Visibility = Visibility.Collapsed;
                    MoveBaby.Visibility = Visibility.Visible;
                    MoveMe.Visibility = Visibility.Visible;
                    ClearStuff.Visibility = Visibility.Visible;
                    
                    ContentPanel.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    Database.DeleteDatabase("Data");
                }
            }
        } 

        public void ProfileSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(NameBox.Text.Trim()) || String.IsNullOrEmpty(Agebox.Text.Trim())||(!Convert.ToBoolean(MaleCheck.IsChecked)&&!Convert.ToBoolean(FemaleCheck.IsChecked)))
            {
                
                MessageBox.Show("You gotta fill up stuff man!");
            }
            
            else
            {
                
                PersonData person = new PersonData();
                person.Name = NameBox.Text.Trim();
                person.Age = Agebox.Text.Trim();
                
                double age;
                
                double.TryParse(Agebox.Text.Trim(), out age);

                person.SetSex = Convert.ToBoolean(MaleCheck.IsChecked) ? PersonData.Sex.Male : PersonData.Sex.Female;

                
                Database database;

                if (!Database.DoesDatabaseExists("Data"))
                {
                    database = Database.CreateDatabase("Data");
                    database.CreateTable<PersonData>("login");
                    database.CreateTable<DataRow>("UserData");
                    database.Table<PersonData>("login").Add(person);
                    try
                    {
                        database.Save();

                    }
                    catch
                    {

                    }
                }
                NavigationService.Navigate(new Uri("/LetsSee.xaml", UriKind.Relative));
                
                    
                    
             


                

            }
        }

        private void MoveBaby_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LetsSee.xaml", UriKind.Relative));
        }

        private void ClearStuff_Click(object sender, RoutedEventArgs e)
        {
            if (Database.DoesDatabaseExists("Data"))
            {
                Database.DeleteDatabase("Data");
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }
    }
}
