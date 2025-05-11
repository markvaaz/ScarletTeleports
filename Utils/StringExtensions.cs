using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScarletTeleports.Utils;

public static class StringExtensions {
  public static string HighlightColor = "#a963ff";
  public static string HighlightErrorColor = "#ff4040";
  public static string HighlightWarningColor = "#ffff00";
  public static string TextColor = "#ffffff";
  public static string ErrorTextColor = "#ff8f8f";
  public static string WarningTextColor = "#ffff9e";
  public static string Bold(this string text) => $"<b>{text}</b>";
  public static string Italic(this string text) => $"<i>{text}</i>";
  public static string Underline(this string text) => $"<u>{text}</u>";
  public static string Red(this string text) => $"<color=red>{text}</color>";
  public static string Green(this string text) => $"<color=green>{text}</color>";
  public static string Blue(this string text) => $"<color=blue>{text}</color>";
  public static string Yellow(this string text) => $"<color=yellow>{text}</color>";
  public static string White(this string text) => $"<color=white>{text}</color>";
  public static string Black(this string text) => $"<color=black>{text}</color>";
  public static string Orange(this string text) => $"<color=orange>{text}</color>";
  public static string Lime(this string text) => $"<color=#00FF00>{text}</color>";
  public static string Gray(this string text) => $"<color=#cccccc>{text}</color>";
  public static string Hex(this string hex, string text) => $"<color={hex}>{text}</color>";

  public static string Format(this string text, List<string> highlightColors = null) {
    highlightColors ??= [HighlightColor];
    return ApplyFormatting(text, TextColor, highlightColors);
  }

  public static string FormatError(this string text) {
    return ApplyFormatting(text, ErrorTextColor, [HighlightErrorColor]);
  }

  public static string FormatWarning(this string text) {
    return ApplyFormatting(text, WarningTextColor, [HighlightWarningColor]);
  }

  private static string ApplyFormatting(string text, string baseColor, List<string> highlightColors) {
    var boldPattern = @"\*\*(.*?)\*\*";
    var italicPattern = @"\*(.*?)\*";
    var underlinePattern = @"__(.*?)__";
    var highlightPattern = @"~(.*?)~";

    var result = Regex.Replace(text, boldPattern, m => Bold(m.Groups[1].Value));
    result = Regex.Replace(result, italicPattern, m => Italic(m.Groups[1].Value));
    result = Regex.Replace(result, underlinePattern, m => Underline(m.Groups[1].Value));

    int highlightIndex = 0;

    result = Regex.Replace(result, highlightPattern, m => {
      string color = highlightIndex < highlightColors.Count ? highlightColors[highlightIndex] : HighlightColor;
      if (color == null) color = HighlightColor;
      highlightIndex++;
      return Hex(color, m.Groups[1].Value);
    });

    return Hex(baseColor, result);
  }
}