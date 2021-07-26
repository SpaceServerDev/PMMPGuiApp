using System;
using System.Windows;

namespace PMMPGuiApp.Windows.OptionWindow {
    /// <summary>
    /// Option.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window {
        public OptionWindow() {
            InitializeComponent();
        }

        private void pathLabel_Loaded(object sender, RoutedEventArgs e) {
            pathLabel.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\PMMPGui\";
        }
    }
}
