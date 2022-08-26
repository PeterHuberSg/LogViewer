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


namespace LogViewerLib {

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
      return newString.EndsWith(Environment.NewLine) ? newString.Substring(0, newString.Length-2) : newString;
    }


    override public string ToString() {
      return $"{StringStyle} {LineHandling} '{String}'";
    }
    #endregion
  }
}
