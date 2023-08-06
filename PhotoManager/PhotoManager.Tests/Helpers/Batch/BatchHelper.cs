using PhotoManager.Domain.Interfaces;
using PhotoManager.Domain;
using System.IO;
using System.Text.RegularExpressions;

namespace PhotoManager.Tests.Helpers.Batch;

public static class BatchHelper
{
    private const int MAX_PATH_LENGTH = 256;

    public static bool IsValidBatchFormat(string batchFormat)
    {
        bool isValid = !string.IsNullOrWhiteSpace(batchFormat);

        if (isValid)
        {
            batchFormat = batchFormat.Trim();
            isValid = IsValidBatchFormatStart(batchFormat, isValid);
            isValid = IsValidBatchFormatEnd(batchFormat, isValid);
            (isValid, string remainingBatchFormat) = IdentifySupportedTags(isValid, batchFormat);
            isValid = IdentifyUnexpectedCharacters(isValid, batchFormat, remainingBatchFormat);
        }

        return isValid;
    }

    public static string ComputeTargetPath(Asset asset, string batchFormat, int ordinal, IFormatProvider provider, IStorageService storageService, bool overwriteExistingTargetFiles)
    {
        bool isValid = IsValidBatchFormat(batchFormat);

        if (isValid)
        {
            batchFormat = batchFormat.Trim();
            (batchFormat, bool includesOrdinal) = ReplaceSupportedTagsWithValues(asset, batchFormat, ordinal, provider);
            (Folder? folder, batchFormat) = ResolveTargetFolder(asset, batchFormat);
            batchFormat = folder != null ? Path.Combine(folder.Path, batchFormat) : string.Empty;
            batchFormat = !overwriteExistingTargetFiles ?
                ComputeUniqueTargetPath(folder, batchFormat, includesOrdinal, storageService) :
                batchFormat;
        }
        else
        {
            batchFormat = asset.FileName;
        }

        return isValid && batchFormat.Length <= MAX_PATH_LENGTH ? batchFormat : string.Empty;
    }

    private static bool IsValidBatchFormatStart(string batchFormat, bool isValid)
    {
        return batchFormat.StartsWith(".") ? batchFormat.StartsWith("..") : isValid;
    }

    private static bool IsValidBatchFormatEnd(string batchFormat, bool isValid)
    {
        return isValid
            && !batchFormat.EndsWith(".")
            && !batchFormat.EndsWith("<")
            && !batchFormat.EndsWith(">");
    }

    private static (bool isValid, string remainingBatchFormat) IdentifySupportedTags(bool isValid, string batchFormat)
    {
        Regex regex = new("(<[#A-Za-z0-9:]*>)", RegexOptions.IgnoreCase);
        var matches = regex.Matches(batchFormat);
        var remainingBatchFormat = batchFormat;

        // Identifies if the complete tags have supported expressions.
        foreach (Match match in matches.Cast<Match>())
        {
            string tag = match.Value[1..^1];
            isValid = isValid
                && match.Success
                && (string.Compare(tag, "#", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "##", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "###", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "####", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "#####", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "######", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "#######", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "########", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "#########", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "##########", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "PixelWidth", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "PixelHeight", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationDate", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationDate:yy", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationDate:yyyy", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationDate:MM", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationDate:MMMM", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationDate:dd", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationTime:HH", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationTime:mm", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationTime:ss", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "CreationTime", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationDate", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationDate:yy", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationDate:yyyy", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationDate:MM", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationDate:MMMM", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationDate:dd", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationTime:HH", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationTime:mm", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationTime:ss", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(tag, "ModificationTime", StringComparison.OrdinalIgnoreCase) == 0);
            remainingBatchFormat = remainingBatchFormat.Replace(match.Value, string.Empty);
        }

        return (isValid, remainingBatchFormat);
    }

    /// <summary>
    /// Identifies if the batch format has any unexpected values after removing the complete tags.
    /// </summary>
    /// <param name="isValid">If the batch format is valid.</param>
    /// <param name="remainingBatchFormat">The remaining of the batch format
    /// after removing the supported tags.</param>
    /// <returns>If the batch format has any unexpected values.</returns>
    private static bool IdentifyUnexpectedCharacters(bool isValid, string batchFormat, string remainingBatchFormat)
    {
        isValid = isValid && remainingBatchFormat
            .IndexOfAny(new[] { '/', '*', '?', '"', '<', '>', '|', '#' }) < 0;

        return isValid && (IsAbsolutePath(batchFormat) ? remainingBatchFormat.IndexOf(':', 3) < 0 :
            remainingBatchFormat.IndexOf(':') < 0);
    }

    private static bool IsAbsolutePath(string batchFormat)
    {
        return batchFormat.Length > 3 && batchFormat.Substring(1, 2) == @":\";
    }

    private static (Folder? folder, string batchFormat) ResolveTargetFolder(Asset asset, string batchFormat)
    {
        Folder? folder = asset.Folder;

        if (batchFormat.StartsWith(@"..\"))
        {
            // If the batch format starts with "..",
            // navigate to parent folder.
            while (batchFormat.StartsWith(@"..\") && folder != null)
            {
                folder = folder.Parent;
                batchFormat = batchFormat[3..];
            }
        }

        return (folder, batchFormat);
    }

    private static (string batchFormat, bool includesOrdinal) ReplaceSupportedTagsWithValues(Asset asset, string batchFormat, int ordinal, IFormatProvider provider)
    {
        (batchFormat, bool includesOrdinal) = ReplaceOrdinalTagWithValue(batchFormat, ordinal);
        batchFormat = batchFormat.Replace("<PixelWidth>", asset.PixelWidth.ToString(), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<PixelHeight>", asset.PixelHeight.ToString(), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationDate>", asset.FileCreationDateTime.ToString("yyyyMMdd", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationDate:yy>", asset.FileCreationDateTime.ToString("yy", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationDate:yyyy>", asset.FileCreationDateTime.ToString("yyyy", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationDate:MM>", asset.FileCreationDateTime.ToString("MM", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationDate:MMMM>", asset.FileCreationDateTime.ToString("MMMM", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationDate:dd>", asset.FileCreationDateTime.ToString("dd", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationTime:HH>", asset.FileCreationDateTime.ToString("HH", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationTime:mm>", asset.FileCreationDateTime.ToString("mm", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationTime:ss>", asset.FileCreationDateTime.ToString("ss", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<CreationTime>", asset.FileCreationDateTime.ToString("HHmmss", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationDate>", asset.FileModificationDateTime.ToString("yyyyMMdd", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationDate:yy>", asset.FileModificationDateTime.ToString("yy", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationDate:yyyy>", asset.FileModificationDateTime.ToString("yyyy", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationDate:MM>", asset.FileModificationDateTime.ToString("MM", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationDate:MMMM>", asset.FileModificationDateTime.ToString("MMMM", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationDate:dd>", asset.FileModificationDateTime.ToString("dd", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationTime:HH>", asset.FileModificationDateTime.ToString("HH", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationTime:mm>", asset.FileModificationDateTime.ToString("mm", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationTime:ss>", asset.FileModificationDateTime.ToString("ss", provider), StringComparison.OrdinalIgnoreCase);
        batchFormat = batchFormat.Replace("<ModificationTime>", asset.FileModificationDateTime.ToString("HHmmss", provider), StringComparison.OrdinalIgnoreCase);

        return (batchFormat, includesOrdinal);
    }

    private static (string batchFormat, bool includesOrdinal) ReplaceOrdinalTagWithValue(string batchFormat, int ordinal)
    {
        bool includesOrdinal = false;
        int ordinalStart = batchFormat.IndexOf("<#");
        int ordinalEnd = batchFormat.LastIndexOf("#>");

        if (ordinalStart >= 0)
        {
            string ordinalPlaceholder = batchFormat.Substring(ordinalStart + 1, ordinalEnd - ordinalStart);
            string ordinalFormat = new('0', ordinalPlaceholder.Length);
            string ordinalString = ordinal.ToString(ordinalFormat);
            batchFormat = batchFormat.Replace("<" + ordinalPlaceholder + ">", ordinalString);
            includesOrdinal = true;
        }

        return (batchFormat, includesOrdinal);
    }

    private static string ComputeUniqueTargetPath(Folder? folder,
        string targetFileName,
        bool targetFileNameIncludesOrdinal,
        IStorageService storageService)
    {
        if (folder != null && storageService.FileExists(targetFileName))
        {
            string[] fileNames = storageService.GetFileNames(folder.Path);

            while (fileNames.Any(f => string.Compare(targetFileName, f, StringComparison.OrdinalIgnoreCase) == 0))
            {
                string[] fileNameParts = targetFileName.Split('.');

                if (targetFileNameIncludesOrdinal)
                {
                    Regex regex = new("(_[0-9]*)$", RegexOptions.IgnoreCase);
                    var matches = regex.Matches(fileNameParts[0]);

                    if (matches.Count > 0)
                    {
                        int count = int.Parse(matches[0].Value[1..]) + 1;
                        string format = new('0', matches[0].Value.Length - 1);
                        targetFileName = $"{fileNameParts[0][..^matches[0].Value.Length]}_{count.ToString(format)}.{fileNameParts[1]}";
                    }
                }
                else
                {
                    targetFileName = $"{fileNameParts[0]}_1.{fileNameParts[1]}";
                }
            }
        }

        return targetFileName;
    }
}
