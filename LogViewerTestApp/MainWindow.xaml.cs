/**************************************************************************************

LogViewerTestApp.MainWindow
===========================

WPF application to test the LogViewer control.

Written in 2022 by Jürgpeter Huber 
Contact: https://github.com/PeterHuberSg/LogViewer

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see LICENSE.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/


using LogViewerLib;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace LogViewerTestApp {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {

    readonly DispatcherTimer test3WpfTimer = new();

    public MainWindow() {
      InitializeComponent();

      test3WpfTimer.Tick += Test3WpfTimer_Tick;
      test3WpfTimer.Interval = TimeSpan.FromMilliseconds(80);
      Test1Button.Click += Test1Button_Click;
      Test2Button.Click += Test2Button_Click;
      Test3Button.Click += Test3Button_Click;
      ClearButton.Click += ClearButton_Click;
    }


    private void Test1Button_Click(object sender, RoutedEventArgs e) {
      TestLogViewer.WriteLine("WriteLine");
      TestLogViewer.WriteLine("WriteLine header1", StringStyleEnum.header1);
      TestLogViewer.WriteLine("WriteLine Label", StringStyleEnum.label);
      TestLogViewer.WriteLine("WriteLine ErrorHeader", StringStyleEnum.errorHeader);
      TestLogViewer.WriteLine("WriteLine errorText", StringStyleEnum.errorText);
      TestLogViewer.WriteLine("WriteLine errorText2", StringStyleEnum.errorText);
      TestLogViewer.WriteLine("WriteLine normal", StringStyleEnum.normal);

      TestLogViewer.Write("Word1 ");
      TestLogViewer.Write("errorHeader ", StringStyleEnum.errorHeader);
      TestLogViewer.Write("errorText ", StringStyleEnum.errorText);
      TestLogViewer.Write("Label ", StringStyleEnum.label);
      TestLogViewer.Write("header1 ", StringStyleEnum.header1);
      TestLogViewer.Write("normal ", StringStyleEnum.normal);
      TestLogViewer.WriteLine("WriteLine");

      TestLogViewer.WriteTempLine("tempLine1");
      TestLogViewer.WriteLine("overwrite tempLine1");
      TestLogViewer.Write("Normal Words ");
      TestLogViewer.WriteTempLine(" followed by tempLine2");
      TestLogViewer.WriteLine("overwrite tempLine2");

      TestLogViewer.WriteTempLine("tempLine3");
    }


    bool shouldTest1BeRunning;
    long test2Long;


    private void Test2Button_Click(object sender, RoutedEventArgs e) {
      if (Test2Button.IsChecked!.Value) {
        shouldTest1BeRunning = true;
        test2Long = 0;
        ThreadPool.QueueUserWorkItem(doTest2);
      } else {
        shouldTest1BeRunning = false;
      }
    }


    private void doTest2(object? state) {
      while (shouldTest1BeRunning) {
        TestLogViewer.WriteTempLine(test2Long.ToString("#,#"));
        test2Long++;
      };
    }


    int test3Int;

    private void Test3Button_Click(object sender, RoutedEventArgs e) {
      if (Test3Button.IsChecked!.Value) {
        test3Int = 0;
        test3WpfTimer.Start();
      } else {
        test3WpfTimer.Stop();
      }
    }

    private void Test3WpfTimer_Tick(object? sender, EventArgs e) {
      TestLogViewer.WriteLine("Test3: " + test3Int.ToString("#,#"));
      test3Int++;
    }


    private void ClearButton_Click(object sender, RoutedEventArgs e) {
      TestLogViewer.Clear();
    }

    private void TestLogViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
      ExtentTextBox.Text = e.ExtentHeight.ToString();
      ViewportTextBox.Text = e.ViewportHeight.ToString();
      VerticalOffsetTextBox.Text = e.VerticalOffset.ToString();
      DiffTextBox.Text = (e.VerticalOffset + e.ViewportHeight - e.ExtentHeight).ToString();
    }
  }
}
