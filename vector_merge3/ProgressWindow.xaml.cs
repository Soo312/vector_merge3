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
using System.Windows.Shapes;

namespace vector_merge3
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public bool isstop = false;
        public ProgressWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isstop = true;
            this.Close();
        }

        internal void setProgressBar(int percentage,string txt)
        {

                progress_text.Content = txt+"  " + percentage + "%";
                progressbar.Value = percentage;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
