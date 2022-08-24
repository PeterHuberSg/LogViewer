using System;


namespace LogViewerLib {

  #region Delegates
  //      ---------

  /// <summary>
  /// Encapsulates a method that takes one or many StyledString as parameter and does not return a value.
  /// </summary>
  /// <param name="s"></param>
  public delegate void ActionStyledStringDelegate(params StyledString[] styledString);
  #endregion


  #region Enumerations
  //      ------------

  /// <summary>
  /// StringStyleEnum can be applied to StyledString. Works similarly like styles in HTML or Word.
  /// </summary>
  public enum StringStyleEnum {
    none = 0,
    normal,
    label,
    header1,
    errorHeader,
    errorText,
  }


  /// <summary>
  /// Does the StyledString mark the end of a line ? If temporary, the next line will overwrite the current line
  /// </summary>
  public enum LineHandlingEnum {
    none = 0,
    endOfLine,
    temporaryEOL,
  }
  #endregion


  /// <summary>
  /// A class to store a string together with some formatting instructions. The formatting is technology
  /// independent and can be used for WPF, HTML, etc.
  /// </summary>
  public class StyledString {
    #region Properties
    //      ----------

    /// <summary>
    /// actual string
    /// </summary>
    public string String { get; private set; }

    /// <summary>
    /// Style to be applied to string
    /// </summary>
    public StringStyleEnum StringStyle { get; private set; }

    /// <summary>
    /// Should a new line be added after this string ? A string can actually contain
    /// Environment.NewLine, meaning one StyledString might cove several lines. LineHandling
    /// applies to the end of StyledString.
    /// </summary>
    public LineHandlingEnum LineHandling { get; private set; }

    /// <summary>
    /// time when string was created. Automatically filled in.
    /// </summary>
    public DateTime Created { get; private set; }
    #endregion


    #region Constructor
    //      -----------
    /// <summary>
    /// constructor
    /// </summary>
    public StyledString(string newString, StringStyleEnum newStringStyle, LineHandlingEnum newLineHandling) {
      String = newString;
      StringStyle = newStringStyle;
      LineHandling = newLineHandling;
      Created = DateTime.Now;
    }


    /// <summary>
    /// constructor, style is normal
    /// </summary>
    public StyledString(string newStringString, LineHandlingEnum newLineHandling) :
      this(newStringString, StringStyleEnum.normal, newLineHandling) { }


    /// <summary>
    /// constructor, no new line needed
    /// </summary>
    public StyledString(string newStringString, StringStyleEnum newStringStyle) :
      this(newStringString, newStringStyle, LineHandlingEnum.none) { }


    /// <summary>
    /// constructor, style is normal, no new line needed
    /// </summary>
    public StyledString(string newStringString) :
      this(newStringString, StringStyleEnum.normal, LineHandlingEnum.none) { }


    /// <summary>
    /// constructor, probably for empty new line
    /// </summary>
    public StyledString(LineHandlingEnum newLineHandling) :
      this("", StringStyleEnum.normal, newLineHandling) { }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// If there is a new line at the end, it gets removed
    /// </summary>
    public static string RemoveNewLineAtEnd(string newString) {
      if (newString.EndsWith(Environment.NewLine)) {
        return newString.Substring(0, newString.Length-2);
      } else {
        return newString;
      }
    }


    #endregion
  }
}
