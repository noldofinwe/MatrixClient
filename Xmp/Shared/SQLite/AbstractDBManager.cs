using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Logging;

namespace Shared.SQLite
{
  public abstract class AbstractDBManager
  {
    //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
    #region --Attributes--
    private static readonly string DB_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "data2.db"
    );
    public static TSSQLiteConnection dB = new TSSQLiteConnection(DB_PATH);

    public const bool RESET_DB_ON_STARTUP = false;

    #endregion
    //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
    #region --Constructors--
    protected AbstractDBManager()
    {
      if (RESET_DB_ON_STARTUP)
      {
#pragma warning disable CS0162 // Unreachable code detected
        DropTables();
#pragma warning restore CS0162 // Unreachable code detected
      }
      CreateTables();
    }

    #endregion
    //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
    #region --Set-, Get- Methods--

    #endregion
    //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
    #region --Misc Methods (Public)--
    public virtual void initManager()
    {
    }

    /// <summary>
    /// Opens a file picker and exports the DB to the selected path.
    /// </summary>
    public static async Task ExportDBAsync(string targetPath)
    {
      if (string.IsNullOrWhiteSpace(targetPath))
      {
        Logger.Info("Exporting DB canceled.");
        return;
      }
      Logger.Info("Started exporting DB to: " + targetPath);

      if (!File.Exists(DB_PATH))
      {
        Logger.Error("Failed to export DB - file not found.");
        return;
      }

      try
      {
        // Use async copy for cross-platform support
        using (var sourceStream = new FileStream(DB_PATH, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        using (var destinationStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
          await sourceStream.CopyToAsync(destinationStream);
        }
        Logger.Info("Exported DB successfully to:" + targetPath);
      }
      catch (Exception e)
      {
        Logger.Error("Error during exporting DB", e);
      }
    }

    ///// <summary>
    ///// Opens a file picker and imports the DB from the selected path.
    ///// </summary>
    //public static async Task ImportDBAsync()
    //{
    //  // Get the source file:
    //  StorageFile sourceFile = await GetTargetOpenPathAsync();
    //  if (sourceFile is null)
    //  {
    //    Logger.Info("Importing DB canceled.");
    //    return;
    //  }
    //  Logger.Info("Started importing DB to: " + sourceFile.Path);

    //  // Close the DB connection:
    //  dB.Close();

    //  // Delete all existing DB files:
    //  await DeleteDBFilesAsync();

    //  // Import DB:
    //  StorageFile dbFile = await StorageFile.GetFileFromPathAsync(DB_PATH);
    //  try
    //  {
    //    await sourceFile.CopyAndReplaceAsync(dbFile);
    //    Logger.Info("Imported DB successfully from:" + sourceFile.Path);

    //  }
    //  catch (Exception e)
    //  {
    //    Logger.Error("Error during importing DB", e);
    //  }

    //  // Open the new DB:
    //  dB = new TSSQLiteConnection(DB_PATH);
    //}

    /// <summary>
    /// Imports the DB from the specified source path.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    public static async Task ImportDBAsync(string sourcePath)
    {
      if (string.IsNullOrWhiteSpace(sourcePath))
      {
        Logger.Info("Importing DB canceled.");
        return;
      }
      Logger.Info("Started importing DB from: " + sourcePath);

      // Close the DB connection:
      dB.Close();

      // Delete all existing DB files:
      await DeleteDBFilesAsync();

      // Import DB:
      if (!File.Exists(sourcePath))
      {
        Logger.Error("Failed to import DB - source file not found.");
        return;
      }

      try
      {
        using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        using (var destinationStream = new FileStream(DB_PATH, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
          await sourceStream.CopyToAsync(destinationStream);
        }
        Logger.Info("Imported DB successfully from:" + sourcePath);
      }
      catch (Exception e)
      {
        Logger.Error("Error during importing DB", e);
      }

      // Open the new DB:
      dB = new TSSQLiteConnection(DB_PATH);
    }

    #endregion

    #region --Misc Methods (Private)--
    /// <summary>
    /// Opens a file save picker and lets the user pick the destination (Avalonia StorageProvider).
    /// </summary>
    /// <param name="parent">The parent control or window.</param>
    /// <returns>Returns the selected file path or null.</returns>
    public static async Task<string?> GetTargetSavePathAsync(Visual parent)
    {
      var topLevel = TopLevel.GetTopLevel(parent);
      if (topLevel?.StorageProvider == null)
        return null;

      var result = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
      {
        Title = "Export SQLite DB",
        SuggestedFileName = "data.db",
        FileTypeChoices = new List<FilePickerFileType>
    {
      new FilePickerFileType("SQLite DB") { Patterns = new[] { "*.db" } }
    }
      });

      return result?.Path.LocalPath;
    }

    /// <summary>
    /// Opens a file picker and lets the user pick the source file (Avalonia).
    /// </summary>
    /// <param name="parent">The parent window for the dialog.</param>
    /// <returns>Returns the selected file path or null.</returns>
    /// <summary>
    /// Opens a file picker and lets the user pick the source file (Avalonia StorageProvider).
    /// </summary>
    /// <param name="parent">The parent control or window.</param>
    /// <returns>Returns the selected file path or null.</returns>
    public static async Task<string?> GetTargetOpenPathAsync(Visual parent)
    {
      var topLevel = TopLevel.GetTopLevel(parent);
      if (topLevel?.StorageProvider == null)
        return null;

      var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
      {
        Title = "Select SQLite DB file",
        AllowMultiple = false,
        FileTypeFilter = new List<FilePickerFileType>
    {
      new FilePickerFileType("SQLite DB") { Patterns = new[] { "*.db" } }
    }
      });

      return files != null && files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    /// <summary>
    /// Deletes all DB files in the database folder (cross-platform).
    /// </summary>
    private static async Task DeleteDBFilesAsync()
    {
      try
      {
        var dbFile = new FileInfo(DB_PATH);
        if (!dbFile.Exists)
        {
          Logger.Warn("Unable to delete DB files - DB file not found.");
          return;
        }

        var folder = dbFile.Directory;
        if (folder == null || !folder.Exists)
        {
          Logger.Warn("Unable to delete DB files - folder not found.");
          return;
        }

        foreach (var file in folder.GetFiles($"{dbFile.Name}*"))
        {
          try
          {
            file.Delete();
            Logger.Info("Deleted DB file: " + file.Name);
          }
          catch (Exception ex)
          {
            Logger.Error($"Failed to delete DB file: {file.Name}", ex);
          }
          await Task.Yield(); // Yield for async signature
        }
        Logger.Info("Finished deleting DB files.");
      }
      catch (Exception e)
      {
        Logger.Error("Failed to delete DB files!", e);
      }
    }

    #endregion

    #region --Misc Methods (Protected)--
    /// <summary>
    /// Deletes the whole db and recreates an empty one.
    /// Only for testing use resetDB() instead!
    /// </summary>
    protected void DeleteDB()
    {
      try
      {
        dB.Close();
        File.Delete(DB_PATH);
      }
      catch (Exception e)
      {
        Logger.Error("Unable to close or delete the DB", e);
      }
      dB = new TSSQLiteConnection(DB_PATH);
    }

    /// <summary>
    /// Drops every table in the db.
    /// </summary>
    protected abstract void DropTables();

    /// <summary>
    /// Creates all required tables.
    /// </summary>
    protected abstract void CreateTables();

    #endregion
    //--------------------------------------------------------Events:---------------------------------------------------------------------\\
    #region --Events--


    #endregion
  }
}
