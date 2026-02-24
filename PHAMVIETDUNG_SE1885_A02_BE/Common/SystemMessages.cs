using System.Collections.Generic;

namespace PHAMVIETDUNG_SE1885_A02_BE.Common
{
  public static class SystemMessages
  {
    // --- Error Codes ---
    public const string GeneralError = "ER000";
    public const string ActiveArticleDeleteError = "ER001";
    public const string UsedCategoryDeleteError = "ER002";
    public const string UsedTagDeleteError = "ER003";
    public const string AccountWithArticlesDeleteError = "ER004";
    public const string DuplicateEmail = "ER005";
    public const string DuplicateCategory = "ER006";
    public const string DuplicateTag = "ER007";

    public const string ValidationRequired = "ER101";
    public const string ValidationMaxLength = "ER102";
    public const string ValidationInvalidFormat = "ER103";
    public const string ValidationRange = "ER104";
    public const string ValidationFutureDate = "ER105";

    public const string DbConnectionError = "ER998";
    public const string ExternalServiceError = "ER999";

    // --- Message Dictionary ---
    private static readonly Dictionary<string, string> _messages = new Dictionary<string, string>
        {
            { GeneralError, "An unexpected error occurred." },
            { ActiveArticleDeleteError, "Cannot delete an active article. Please deactivate it first." },
            { UsedCategoryDeleteError, "Cannot delete category because it is used by news articles." },
            { UsedTagDeleteError, "Cannot delete tag because it is used in news articles." },
            { AccountWithArticlesDeleteError, "Cannot delete account because they have created news articles." },
            { DuplicateEmail, "Email already exists in the system." },
            { DuplicateCategory, "Category name already exists." },
            { DuplicateTag, "Tag name already exists." },

            { ValidationRequired, "{0} is required." },
            { ValidationMaxLength, "{0} cannot exceed {1} characters." },
            { ValidationInvalidFormat, "Invalid format for {0}." },
            { ValidationRange, "{0} must be between {1} and {2}." },
            { ValidationFutureDate, "{0} cannot be in the future." },

            { DbConnectionError, "Database connection failed." },
            { ExternalServiceError, "External service unavailable." }
        };

    public static string GetMessage(string code, params object[] args)
    {
      if (_messages.TryGetValue(code, out var template))
      {
        return string.Format(template, args);
      }
      return "Unknown Error Code: " + code;
    }

    public static string GetMessageWithCode(string code, params object[] args)
    {
      return $"[{code}] {GetMessage(code, args)}";
    }
  }
}
