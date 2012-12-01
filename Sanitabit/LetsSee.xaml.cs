using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using SilverlightPhoneDatabase;
using Microsoft.Phone.Scheduler;

namespace Sanitabit
{
    public partial class LetsSee : PhoneApplicationPage
    {
        List<DateTime> FeedingtimeList = new List<DateTime>();
        List<TimePicker> TimePickerList = new List<TimePicker>();
        
        public LetsSee()
        {
            InitializeComponent();
            if (Database.OpenDatabase("Data").Table<DataRow>("UserData").Count != 0)
            {
                //MessageBox.Show(MealSlider.Value.ToString());
                LoadLastState();
            }
            else
            {

                try
                {
                    InputStackPanel.Children.Clear();
                    
                    for (int i = 0; i < MealSlider.Value; i++)
                    {
                        TimePicker timePicker = new TimePicker();
                        timePicker.Name = "time" + (i + 1).ToString();
                        TimePickerList.Add(timePicker);
                        //timePicker.ValueChanged += timePicker_ValueChanged;
                        InputStackPanel.Children.Add(timePicker);
                    }
                }
                catch (Exception excep)
                { }
            }
            

        }
        public void LoadLastState()
        {
            Database db=Database.OpenDatabase("Data");
            Table<DataRow> tb = db.Table<DataRow>("UserData");
            int count = tb.Last<DataRow>().TimeList.Length;
            //MessageBox.Show(count.ToString());
            MealSlider.Value = count;


            for (int i = 0; i < count; i++)
            {
                TimePickerList[i].Value = tb.Last<DataRow>().TimeList[i];
            }

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                TimePickerList.Clear();
                InputStackPanel.Children.Clear();
                for (int i = 0; i < (int)MealSlider.Value; i++)
                {
                    TimePicker timePicker = new TimePicker();
                    timePicker.Name = "time" + (i + 1).ToString();
                    TimePickerList.Add(timePicker);
                    //timePicker.ValueChanged += timePicker_ValueChanged;
                    InputStackPanel.Children.Add(timePicker);

                }
            }
            catch (Exception excep)
            { }
        }

        void timePicker_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            
            
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            Database db = Database.OpenDatabase("Data");
            DataRow entryRow=new DataRow();
            entryRow.setTimeList((int)MealSlider.Value);
            for (int i = 0; i < (int)MealSlider.Value; i++)
            {
                entryRow.TimeList[i] = (DateTime)TimePickerList[i].Value;
                //MessageBox.Show(TimePickerList[i].Value + "  " + i);
            }
            db.Table<DataRow>("UserData").Add(entryRow);
            MessageBox.Show("Database Synced");
            db.Save();

           // TriggerBreakFastAlarm();
            int rowCount = db.Table<DataRow>("UserData").Count;

            DateTime[][] DateTimeArray = new DateTime[rowCount][];


            for (int i = 0; i < rowCount; i++)
            {
                DateTimeArray[i] = new DateTime[db.Table<DataRow>("UserData")[i].TimeList.Length];
                DateTimeArray[i] = db.Table<DataRow>("UserData")[i].TimeList;
            }

            TimeSpan sum = new TimeSpan();
            DateTime init = DateTimeArray[rowCount - 1][0];

            for (int i = rowCount - 2; i > 1; i--)
            {
                init.Add(DateTimeArray[i][0] - DateTimeArray[i - 1][0]);

            }




            if (Convert.ToBoolean(Lactose.IsChecked) || Convert.ToBoolean(Diarrhoea.IsChecked) || Convert.ToBoolean(ConstipationCheck.IsChecked))
            {
                init.Subtract(TimeSpan.FromHours(4));
            }

            if (ScheduledActionService.Find("TestAlarm") != null)
            {
                ScheduledActionService.Remove("TestAlarm");
            }



            Alarm a = new Alarm("TestAlarm");
            
            a.Content = "Shouldn't you wash your hand?";
            a.Sound = new Uri("alarm-clock-1.wav", UriKind.Relative);
            if (DateTime.Now > init)
                a.BeginTime = DateTime.Now.Add(DateTime.Now - init);
            else
                a.BeginTime = DateTime.Now.Add(init - DateTime.Now);
            ScheduledActionService.Add(a);

            MessageBox.Show("Alarm triggered at " + a.BeginTime.ToShortTimeString());

        }

        public void TriggerBreakFastAlarm()
        {
            Database db = Database.OpenDatabase("Data");
            
        }

        private void PanoramaItem_Loaded_1(object sender, RoutedEventArgs e)
        {
            
        }
    }
}