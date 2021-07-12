using PMMPGuiApp.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace PMMPGuiApp.PoggitWindow {
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class PoggitWindow : Window {



        public PoggitWindow() {
            InitializeComponent();

        }

        private void PluginList_Loaded(object sender, RoutedEventArgs e) {
            ObservableCollection<PoggitData> data = new ObservableCollection<PoggitData>();
            int index = 0;
            data.Add(new PoggitData { id = ++index, name = "EconomyS" + index, tagline = "yeah" + index ,download = 10000,image= "https://raw.githubusercontent.com/poggit-orphanage/EconomyS/449b2cbd25c250aff680738ae2654ba11751e347/EconomyAPI/icon.png" });
            data.Add(new PoggitData { id = ++index, name = "Devtools" + index, tagline = "foo" + index, download = 1005 });
            data.Add(new PoggitData { id = ++index, name = "Texter" + index, tagline = "test" + index, download = 10013 });
            data.Add(new PoggitData { id = ++index, name = "unko" + index, tagline = "test" + index, download = 10013 });
            data.Add(new PoggitData { id = ++index, name = "poop" + index, tagline = "test" + index, download = 10013 });
            data.Add(new PoggitData { id = ++index, name = "buri" + index, tagline = "test" + index, download = 10013 });
            PluginList.ItemsSource = data;
        }

        private void Click(ButtonData item) {
            Debug.Print(string.Format("This book was clicked: {0}", item.url));
        }

    }
    class ButtonData {
        public string url { get; set; }

        public ICommand ClickCommand { get; set; }
    }
}
