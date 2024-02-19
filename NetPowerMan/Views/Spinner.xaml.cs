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

namespace NetPowerMan.Views
{
    /// <summary>
    /// Logica di interazione per Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl
    {
        public Spinner()
        {
            InitializeComponent();
        }
        public Duration Duration2
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }
        public int From
        {
            get { return (int)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        public int To
        {
            get { return (int)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration2", typeof(Duration), typeof(Spinner), new PropertyMetadata(default(Duration)));
        
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(int), typeof(Spinner), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(int), typeof(Spinner), new PropertyMetadata(default(int)));

        public Brush SpinnerColor
        {
            get { return (Brush)GetValue(SpinnerColorProperty); }
            set { SetValue(SpinnerColorProperty, value); }
        }

        public static readonly DependencyProperty SpinnerColorProperty =
            DependencyProperty.Register("SpinnerColor", typeof(Brush), typeof(Spinner), new PropertyMetadata(Brushes.DodgerBlue));
    }
}
