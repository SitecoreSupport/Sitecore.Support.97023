namespace Sitecore.Support.Pipelines.RenderField
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
  using Sitecore.Pipelines.RenderField;
  using Diagnostics;
  using System.Text;
  using Configuration;

  public class ProtectedImageLinkRenderer : Sitecore.Pipelines.RenderField.ProtectedImageLinkRenderer
  {
    // Fields
    private readonly char[] quotes = new char[] { '\'', '"' };

    // Methods
    protected new string HashImageReferences(string renderedText)
    {
      Assert.ArgumentNotNull(renderedText, "renderedText");
      if (renderedText.IndexOf("<img ", StringComparison.OrdinalIgnoreCase) < 0)
      {
        return renderedText;
      }
      if (renderedText.IndexOf(Settings.Media.RequestProtection.HashParameterName + "=", StringComparison.OrdinalIgnoreCase) > 0)
      {
        return renderedText;
      }
      int startIndex = 0;
      bool flag = false;
      while ((startIndex < renderedText.Length) && !flag)
      {
        int tagStartIndex = renderedText.IndexOf("<img", startIndex, StringComparison.OrdinalIgnoreCase);
        if (tagStartIndex < 0)
        {
          break;
        }
        flag = base.CheckReferenceForParams(renderedText, tagStartIndex);
        startIndex = renderedText.IndexOf(">", tagStartIndex, StringComparison.OrdinalIgnoreCase) + 1;
      }
      if (!flag)
      {
        return renderedText;
      }
      startIndex = 0;
      StringBuilder builder = new StringBuilder(renderedText.Length + 0x80);
      while (startIndex < renderedText.Length)
      {
        int num3 = renderedText.IndexOf("<img", startIndex, StringComparison.OrdinalIgnoreCase);
        if (num3 > -1)
        {
          int num4 = renderedText.IndexOf(">", num3, StringComparison.OrdinalIgnoreCase) + 1;
          builder.Append(renderedText.Substring(startIndex, num3 - startIndex));
          string imgTag = renderedText.Substring(num3, num4 - num3);
          builder.Append(this.ReplaceReference(imgTag));
          startIndex = num4;
        }
        else
        {
          builder.Append(renderedText.Substring(startIndex, renderedText.Length - startIndex));
          startIndex = 0x7fffffff;
        }
      }
      return builder.ToString();
    }

    public new void Process(RenderFieldArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (Settings.Media.RequestProtection.Enabled && !args.FieldTypeKey.StartsWith("__"))
      {
        args.Result.FirstPart = this.HashImageReferences(args.Result.FirstPart);
        args.Result.LastPart = this.HashImageReferences(args.Result.LastPart);
      }
    }

    private string ReplaceReference(string imgTag)
    {
      Assert.ArgumentNotNull(imgTag, "imgTag");
      bool flag = true;
      string str = imgTag;
      if (imgTag.Contains("&amp;"))
      {
        str = str.Replace("&amp;", "&");
      }
      else if (imgTag.Contains("&"))
      {
        flag = false;
      }
      int startIndex = str.IndexOf("src", StringComparison.OrdinalIgnoreCase) + 3;
      startIndex = str.IndexOfAny(this.quotes, startIndex) + 1;
      int num2 = str.IndexOfAny(this.quotes, startIndex);
      string url = str.Substring(startIndex, num2 - startIndex);
      if (!url.Contains("?"))
      {
        return imgTag;
      }
      url = this.GetProtectedUrl(url);
      if (flag)
      {
        url = url.Replace("&", "&amp;");
      }
      return (str.Substring(0, startIndex) + url + str.Substring(num2, str.Length - num2));
    }
  }





}