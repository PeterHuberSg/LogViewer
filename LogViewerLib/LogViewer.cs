/**************************************************************************************

LogViewerLib.StyledString
=========================

Defines text formatting supported by LogViewer

Written in 2022 by Jürgpeter Huber 
Contact: https://github.com/PeterHuberSg/LogViewer

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see LICENSE.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogViewerLib {


  /// <summary>
  /// This WPF control displays logging information, scrolling the displayed text if there is not enough screen space. 
  /// The text can be bold, italic, red, etc.
  /// Some information can be written temporarly. This kind of text gets only written to the screen when nothing else 
  /// was written for 300 msecs and it gets overwritten by the next log text.
  /// </summary>
  public class LogViewer: RichTextBox {

    #region Properties
    //      ----------

    /// <summary>
    /// How long a temporary write gets delayed before it is shown to the user
    /// </summary>
    public int TemporaryDelay {
      get { return (int)GetValue(TemporaryDelayProperty); }
      set { SetValue(TemporaryDelayProperty, value); }
    }

    // DependencyProperty definition
    public static readonly DependencyProperty TemporaryDelayProperty =
        DependencyProperty.Register("TemporaryDelay", typeof(int), typeof(LogViewer), new PropertyMetadata(300));

        public string TimeStampFormat { get; set; } = "hh:mm:ss";
    #endregion


    #region Constructor
    //      -----------

    readonly DispatcherTimer wpfTimer = new ();
    readonly FlowDocument flowDoc;
    Paragraph styledParagraph;


    public LogViewer() {
      HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      AcceptsReturn = false;
      IsReadOnly = true;
      IsUndoEnabled = false;

      wpfTimer.Tick += WpfTimer_Tick;
      wpfTimer.Interval = TimeSpan.FromMilliseconds(300);
      flowDoc = new FlowDocument();
      Document = flowDoc;
      styledParagraph = getEmptyParagraph();
    }
    #endregion


    #region Public Methods
    //      --------------

    /// <summary>
    /// Empties the LogViwer content
    /// </summary>
    public void Clear() {
      flowDoc.Blocks.Clear();
    }


    public void WriteLine() {
      write(new StyledString("", StringStyleEnum.normal, LineHandlingEnum.endOfLine));
    }


    public void WriteLine(string line) {
      write(new StyledString(line, LineHandlingEnum.endOfLine));
    }


    /// <summary>
    /// This line gets only shown to the user when nothing else gets written for a short while and it will get overwritten
    /// by the next WriteXxx(). This is usefull when the user should get only few notifications per second even there
    /// might be many WriteTempXxx() be executed.
    /// </summary>
    public void WriteTempLine(string line) {
      write(new StyledString(line, LineHandlingEnum.temporaryEOL));
    }


    public void WriteLine(string line, StringStyleEnum stringStyle) {
      write(new StyledString(line, stringStyle, LineHandlingEnum.endOfLine));
    }


    /// <summary>
    /// This line gets only shown to the user when nothing else gets written for a short while and it will get overwritten
    /// by the next WriteXxx(). This is usefull when the user should get only few notifications per second even there
    /// might be many WriteTempXxx() be executed.
    /// </summary>
    public void WriteTempLine(string line, StringStyleEnum stringStyle) {
      write(new StyledString(line, stringStyle, LineHandlingEnum.endOfLine));
    }


    public void Write(string text) {
      write(new StyledString(text));
    }


    public void Write(string text, StringStyleEnum stringStyle) {
      write(new StyledString(text, stringStyle));
    }


    public void Write(StyledString styledString) {
      write(styledString);
    }


    public void Write(params StyledString[] styledStrings) {
      foreach (var styledString in styledStrings) {
        write(styledString);
      }
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void WpfTimer_Tick(object? sender, EventArgs e) {
      List<StyledString> stringBuffer;
      lock (stringBuffers) {
        stringBuffer = stringBuffers[stringBufferIndex];
        if (stringBuffer.Count==0) {
          isWpfTimerActivated = false;
          wpfTimer.Stop();
          return;

        } else {
          stringBufferIndex = stringBufferIndex==0 ? 1 : 0;
        }
      }
      writeLog(stringBuffer);
    }
    #endregion


    #region Private methods
    //      ---------------

    bool isWpfTimerActivated;
    readonly List<StyledString>[] stringBuffers = { new List<StyledString>(), new List<StyledString>()};
    int stringBufferIndex;


    private void write(StyledString styledString) {
      lock (stringBuffers) {
        var stringBuffer = stringBuffers[stringBufferIndex];

        //remove last line if it is temporary
        var lastStringIndex = stringBuffer.Count-1;
        if (lastStringIndex>=0) {
          if (stringBuffer[lastStringIndex].LineHandling==LineHandlingEnum.temporaryEOL) {
            do {
              stringBuffer.RemoveAt(lastStringIndex--);
            } while (lastStringIndex>=0 && stringBuffer[lastStringIndex].LineHandling!=LineHandlingEnum.endOfLine);
          }
        }

        stringBuffer.Add(styledString);

        if (!isWpfTimerActivated) {
          //nothing was written to log for some time. Write styledString immediately to log
          isWpfTimerActivated = true;
          if (CheckAccess()) {
            //running on wpf thread, write to log immediately, which will empty stringBuffer and start WPF timer
            writeLog(stringBuffer);
            stringBuffer.Clear();
          } else {
            //running on a different thread, start WPF timer on WPF Window thread.
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
             new Action(wpfTimer.Start));
          }
        }
      }
    }


    private void writeLog(List<StyledString> stringBuffer) {
      var needsScrollingToEnd = ExtentHeight<=ViewportHeight || //content is smaller than screen, start automatic scrolling
        ViewportHeight + VerticalOffset - ExtentHeight==0; //

      foreach (var styledString in stringBuffer) {
        append(styledString);
      }
      stringBuffer.Clear();
      if (needsScrollingToEnd) {
        ScrollToEnd();
      }
      wpfTimer.Start();
    }


    bool lastLineWasTemporary;


    private void append(StyledString styledString) {
      //breakup styledString into lines
      String[] lineStrings = styledString.String.Split(Environment.NewLine, StringSplitOptions.None);
      for (int lineIndex = 0; lineIndex < lineStrings.Length; lineIndex++) {

        //Process every line
        string lineString = lineStrings[lineIndex];

        //apply styles
        var inline = styledString.ToInline(styledParagraph, lineString);

        if (lastLineWasTemporary) {
          //last line was temporary. overwrite it
          lastLineWasTemporary = false;
          styledParagraph.Inlines.Clear();
        }

        //if last styledString was end of line, add a new paragraph to flow document.
        if (styledParagraph.Inlines.Count==0) {
          flowDoc.Blocks.Add(styledParagraph);
          if (styledString.LineHandling!=LineHandlingEnum.endOfLine || styledString.String.Length>0) {
            //not an empty line. Add the time
            Run timeRun = new(styledString.Created.ToString($"{TimeStampFormat}  ")) { 
              Foreground = Brushes.DimGray
            };
            styledParagraph.Inlines.Add(timeRun);
          }
        }

        //add line
        styledParagraph.Inlines.Add(inline);

        //check for end of line
        if (styledString.LineHandling==LineHandlingEnum.endOfLine) {
          styledParagraph = getEmptyParagraph();
        } else if (styledString.LineHandling==LineHandlingEnum.temporaryEOL) {
          lastLineWasTemporary = true;
        } else if (lineIndex < lineStrings.Length-1) {
          styledParagraph = getEmptyParagraph();
        }
      }
    }


    private Paragraph getEmptyParagraph() {
      Paragraph newParagraph = new Paragraph {
        Margin = new Thickness(0)
      };
      return newParagraph;
    }
    #endregion

  }
}
