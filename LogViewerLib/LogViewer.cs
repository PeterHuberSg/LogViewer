using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
    #endregion


    #region Constructor
    //      -----------

    readonly FlowDocument flowDoc;
    Paragraph styledParagraph;


    public LogViewer() {
      HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      AcceptsReturn = false;
      IsReadOnly = true;
      IsUndoEnabled = false;

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
      invokeWrite(new StyledString("", StringStyleEnum.normal, LineHandlingEnum.endOfLine));
    }


    public void WriteLine(string line) {
      invokeWrite(new StyledString(line, LineHandlingEnum.endOfLine));
    }


    /// <summary>
    /// This line gets only shown to the user when nothing else gets written for a short while and it will get overwritten
    /// by the next WriteXxx(). This is usefull when the user should get only few notifications per second even there
    /// might be many WriteTempXxx() be executed.
    /// </summary>
    public void WriteTempLine(string line) {
      invokeWrite(new StyledString(line, LineHandlingEnum.temporaryEOL));
    }


    public void WriteLine(string line, StringStyleEnum stringStyle) {
      invokeWrite(new StyledString(line, stringStyle, LineHandlingEnum.endOfLine));
    }


    /// <summary>
    /// This line gets only shown to the user when nothing else gets written for a short while and it will get overwritten
    /// by the next WriteXxx(). This is usefull when the user should get only few notifications per second even there
    /// might be many WriteTempXxx() be executed.
    /// </summary>
    public void WriteTempLine(string line, StringStyleEnum stringStyle) {
      invokeWrite(new StyledString(line, stringStyle, LineHandlingEnum.endOfLine));
    }


    public void Write(string text) {
      invokeWrite(new StyledString(text));
    }


    public void Write(string text, StringStyleEnum stringStyle) {
      invokeWrite(new StyledString(text, stringStyle));
    }


    public void Write(StyledString styledString) {
      invokeWrite(styledString);
    }


    public void Write(params StyledString[] styledStrings) {
      invokeWrite(styledStrings);
    }
    #endregion


    #region Private methods
    //      ---------------

    private void invokeWrite(object styledObject) {
      if (!CheckAccess()) {
        //we are on a different thread, synchronise with the WPF Window thread.
        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
         new Action<object>(invokeWrite), styledObject);
      }

      if (styledObject is StyledString styledString) {
        append(styledString);
      } else if (styledObject is StyledString[] styledStrings) {
        foreach (StyledString styledString2 in styledStrings) {
          append(styledString2);
        }
      } else {
        throw new NotSupportedException($"LogViewer.Write(): unsupported type '{styledObject.GetType()}' with content '{styledObject}'.");
      }
      ScrollToEnd();
    }


    bool lastLineWasTemporary;


    private void append(StyledString styledString) {
      //breakup styledString into lines
      String[] lineStrings = styledString.String.Split(Environment.NewLine, StringSplitOptions.None);
      for (int lineIndex = 0; lineIndex < lineStrings.Length; lineIndex++) {

        //Process every line
        string lineString = lineStrings[lineIndex];
        Inline inline;

        //apply styles
        switch (styledString.StringStyle) {
        case StringStyleEnum.errorHeader:
          styledParagraph.Margin = new Thickness(0, 24, 0, 4);
          inline = new Bold(new Run(lineString));
          inline.FontSize = styledParagraph.FontSize * 1.2;
          inline.Foreground = Brushes.Red;
          break;
        case StringStyleEnum.errorText:
          inline = new Run(lineString);
          inline.Foreground = Brushes.Red;
          break;
        case StringStyleEnum.label:
          inline = new Run(lineString);
          inline.Foreground = Brushes.MidnightBlue;
          break;
        case StringStyleEnum.header1:
          styledParagraph.Margin = new Thickness(0, 24, 0, 4);
          inline = new Bold(new Run(lineString));
          inline.FontSize = styledParagraph.FontSize * 1.2;
          break;
        case StringStyleEnum.normal:
        case StringStyleEnum.none:
        default:
          inline = new Run(lineString);
          break;
        }

        if (lastLineWasTemporary) {
          //last line was temporary. overwrite it
          lastLineWasTemporary = false;
          styledParagraph.Inlines.Clear();
        }

        //if last styledString was end of line, start a new paragraph to flow document.
        if (styledParagraph.Inlines.Count==0) {
          flowDoc.Blocks.Add(styledParagraph);
          if (styledString.LineHandling!=LineHandlingEnum.endOfLine || styledString.String.Length>0) {
            //not an empty line. Add the time
            Run timeRun = new(styledString.Created.ToString("hh:mm:ss  ")) {
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
      Paragraph newParagraph = new Paragraph();
      newParagraph.Margin = new Thickness(0);
      return newParagraph;
    }


    //public void DoEvents() {
    //  DispatcherFrame frame = new DispatcherFrame();
    //  Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
    //      new DispatcherOperationCallback(ExitFrames), frame);
    //  Dispatcher.PushFrame(frame);
    //}

    //public object ExitFrames(object f) {
    //  ((DispatcherFrame)f).Continue = false;

    //  return null;
    //}
    #endregion

  }
}
