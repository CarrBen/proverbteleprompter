﻿
using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ProverbTeleprompter.Helpers;
//using ProverbTeleprompter.WebController;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;

namespace ProverbTeleprompter
{
    [ApiController]
    public class TestController : Controller
    {
        [Route("/test")]
        public string Test()
        {
            return "This is a test";
        }
    }

    public class TestStartup : IStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddMvc();
            return services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
    	public static MainWindow SharedMainWindow;

        private bool _isDraggingEyeline;
		public SynchronizationContext SyncContext { get; set; }

        public static FrameworkElement PromptView { get; set; }


    	public MainWindowViewModel MainWindowViewModel { get; set; }

        public MainWindow()
        {
        	SyncContext = SynchronizationContext.Current;
            InitializeComponent();
            MainWindowViewModel = new MainWindowViewModel(MainTextBox)
                                      {
										  BookmarkImage = Resources["WhiteBookmarkImage"] as ImageSource
                                      };


            DataContext = MainWindowViewModel;
           // MainWindowViewModel.ToolsVisible = true;

            MainTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(MainTextBox_TextChanged);

            PreviewKeyDown += MainWindow_PreviewKeyDown;
            PreviewKeyUp += MainWindow_PreviewKeyUp;
            LocationChanged += MainWindow_LocationChanged;
            SizeChanged += MainWindow_SizeChanged;
            Loaded += MainWindow_Loaded;

            MouseDoubleClick += new MouseButtonEventHandler(MainWindow_MouseDoubleClick);
            
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;


            Closing += MainWindow_Closing;

            PromptView = LayoutRoot;

        	SharedMainWindow = this;
			if(Properties.Settings.Default.StartWebController)
			{
				//Hosting.Start();
			}

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var host = new WebHostBuilder().UseConfiguration(config).UseKestrel().UseStartup<TestStartup>().Build();
            Task.Run(host.Run);
			

        }


        void MainTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MainWindowViewModel.IsDocumentDirty = true;
        }



        void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {


                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

  
        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
  
            DragMove();
            
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
		//SetToolSizeAndPos();
        }

        void MainWindow_LocationChanged(object sender, EventArgs e)
        {
       //     SetToolSizeAndPos();
        }

        private void SetToolSizeAndPos()
        {
            MainWindowViewModel.ToolWindowHeight = 280;
            MainWindowViewModel.ToolWindowLeft =  Left;
            MainWindowViewModel.ToolWindowWidth = ActualWidth;
            MainWindowViewModel.ToolWindowTop = Top + ActualHeight - MainWindowViewModel.ToolWindowHeight;
        }

        void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            MainWindowViewModel.KeyUp(sender, e);
        }

        void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MainWindowViewModel.KeyDown(sender, e);
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();

            if(MainWindowViewModel.CanShutDownApp())
            {
                MainWindowViewModel.Dispose();
            }
            else
            {
                e.Cancel = true;
            }

			
            
        }

        void RemoteHandler_RemoteButtonPressed(object sender, RemoteButtonPressedEventArgs e)
        {
            MainWindowViewModel.RemoteButtonPressed(sender, e);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
         
            //Setup event handler for remote control buttons (multi media buttons)
			HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			SystemHandler.RegisterHidNotification(source.Handle);
			source.AddHook(new HwndSourceHook(SystemHandler.WndProc));

            MainWindowViewModel.ToggleToolsWindow();
            MainWindowViewModel.InitializeConfig();
        }



        private void EyelineLeftTriangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingEyeline = true;
            e.Handled = true;
        }

        private void Grid_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(_isDraggingEyeline && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);

                var newPos = mousePos.Y - (EyelineLeftTriangle.Height/2);

                //SetEyeLinePosition(newPos);

                MainWindowViewModel.EyelinePosition = newPos;
                
            }
            else if(_isDraggingEyeline && e.LeftButton == MouseButtonState.Released)
            {
                AppConfigHelper.SetUserSetting("EyeLinePosition", MainWindowViewModel.EyelinePosition);
                _isDraggingEyeline = false;
            }
            else
            {
              //  _isDraggingEyeline = false;
            }
        }


    }
}