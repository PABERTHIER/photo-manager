# PhotoManager

![PhotoManager](PhotoManager/Images/AppIcon.png)

[![Test](https://github.com/jpablodrexler/jp-photo-manager/actions/workflows/test.yml/badge.svg)](https://github.com/jpablodrexler/jp-photo-manager/actions/workflows/test.yml)
[![Release](https://github.com/jpablodrexler/jp-photo-manager/actions/workflows/release.yaml/badge.svg)](https://github.com/jpablodrexler/jp-photo-manager/actions/workflows/release.yaml)
[![CodeQL](https://github.com/jpablodrexler/jp-photo-manager/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/jpablodrexler/jp-photo-manager/actions/workflows/codeql-analysis.yml)

## Features

PhotoManager is a desktop application that allows:

- Visualization of image galleries
- Change Windows wallpaper
- Find duplicates
- Copy/move images
- Know if images are corrupted or rotated
- Import images from local folders (such as game screenshots folders)
- Import images from shared folders in the local network
- Export images to local folders
- Export images to shared folders in the local network
- Delete images in local or shared folders that are not present in source folder
- Detect videos duplicates

Soon will allow to:

- Add your own metadata to the images
- Search images
- Rename images in batches

For the whole roadmap for the application, please take a look at the issues in this repository.

## Run the application

Open the solution file `PhotoManager/PhotoManager.sln` and run the `PhotoManager/PhotoManager.UI/PhotoManager.UI.csproj` project.

## Installation instructions

- Download the zip file with the compiled application files (`publish.zip` or `photo-manager-{version}.zip`) for the latest release.
- Unzip the content of the zip file to a new folder.
- Run `PhotoManager.UI.exe`.
- The application saves the catalog files in the following folder: `C:\Users\{username}\AppData\Local\PhotoManager`.

## Compatible picture formats

- .bmp - Windows bitmap
- .dng - RAW Format
- .gif - Graphics Interchange Format
- .heic - Apple Format
- .ico - Icon file
- .jfif - JPEG File Interchange Format
- .jpeg, .jpg - Joint Photographic Experts Group
- .png - Portable Network Graphics
- .tiff, .tif - Tagged Image File Format
- .webp - WebP image

## Compatible video formats

- .3g2 - Mobile video
- .3gp - Mobile video
- .asf - Advanced Systems Format
- .av1 - Video coding format for videos transmissions
- .avi - Audio Video Interleave
- .flv - Flash video
- .m4v - MP4 video
- .mkv - Matroska video
- .mov - QuickTime movie
- .mp4 - MP4 video
- .mpeg - Moving Picture Experts Group
- .mpg - Moving Picture Experts Group
- .ogv - Ogg Vorbis video
- .webm / av1 - WebM video
- .wmv - Windows Media Video

## Switches usage

There are many switches, enable or disable it as you wish:

- `UsingMD5Hash = false;`: Enable it to Hash in MD5.
- `UsingPHash = false;`: Enable it to detect duplicates between rotated assets (improve detection) PHash = Perceptual Hash. Performances are decreased with PHash by 6 times (for ex: 0.17s for 140 pictures with SHA512 and 1.11s with PHash).
- `UsingDHash = false;`: Enable it to Hash in DHash (Difference Hash). This hashing method always returns "0" for Gif file.
- `DetectThumbnails = false;`: Enable it to detect duplicates between a thumbnail and the original.
- `AnalyseVideos = false;`: Enable it to extract the first frame from each videos, that will be stored in the folder `FirstFrameVideosFolderName`.
- `SyncAssetsEveryXMinutes = false;`: Enable it to sync your assets every X minutes (CATALOG_COOLDOWN_MINUTES).

About `DetectThumbnails`, you will need to activate the `UsingPHash` switch as well because:
Between Original and Thumbnail:

- PHash the hammingDistance is 36/210.
- DHash the hammingDistance is 16/17.
- MD5Hash the hammingDistance is 32/32.
- SHA512 the hammingDistance is 118/128.

Moreover, there is a parameter that you can adjust following the need, it is `PHashThreshold`.
The max advised is less than 90 (for example 68 can detect false positives).
The default value is `40`, because it can detect a Thumbnail and an original with low quality as duplicates.
But 5 or 6 is often used as a default value in image comparison libraries.
If the hammingDistance is lower or equal to the `PHashThreshold` value, then it is a duplicate.

## Path settings

To run the application, you will need to set some settings first, for the path:

- `AssetsDirectory = "";`: The path where PhotoManager will scan your assets.
- `ExemptedFolderPath = "";`: The path where PhotoManager will protect your assets and if there are duplicates in others paths, you will be able to delete all of them except the asset in this exempted path.
- `FfmpegPath = "Path\\ffmpeg.exe";`: The path where your ffmpeg.exe is located (Needed to be installed first, used if you activate the switch `AnalyseVideos`).
- `BackupPath = "";`: The path to store your backup (The backup must be upper than the location to prevent bugs like "Process is already used").
- `FirstFrameVideosFolderName = "";`: The folder to save the first frame for each video file (Used if you activate the switch `AnalyseVideos`), the path will be "`AssetsDirectory` + `\\FirstFrameVideosFolderName`".

## Deletion modes

There are 3 ways to delete duplicates:
The button `Delete` => This will delete the selected duplicate.
The button `DeleteAllButThis` => This will delete all duplicates associated to this asset and keep the selected duplicate.
The button `Delete Every Duplicates Linked To Exempt Folder` => This will delete all duplicates found everywhere that has a duplicate linked in the `ExemptedFolderPath`, but all duplicates in the `ExemptedFolderPath` will be protected.

## How to use it for videos ?

PhotoManager is able to detect duplicate between many videos.
To do so, you'll need to activate the switch `AnalyseVideos` and put all of your videos in a single folder.
It will create another folder (it should not exists), called `outputVideoThumbnails`, and will store inside of it the first frame for each video (with the name of the video file).
⚠ Be aware that `outputVideoThumbnails` folder should be deleted before each running to prevent some conflicts. ⚠
Then, you'll be able to see if there are some duplicates between videos and delete, manually, the videos in question.
This feature is only here to let you to identify if you got some videos duplicates.

## Technologies used

- [.NET 8.0](https://dotnet.microsoft.com/)
- [Windows Presentation Foundation](https://docs.microsoft.com/en-us/dotnet/framework/wpf/)
- [NUnit](https://nunit.org/)
- [Moq framework for .NET](https://github.com/moq/moq4)
- [MagickImage](https://github.com/dlemstra/Magick.NET)
- [log4net](https://logging.apache.org/log4net/)
- [coverlet](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [FFMPEG](https://ffmpeg.org/)
