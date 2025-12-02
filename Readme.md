# PhotoManager

![PhotoManager][app-icon]

[![Build & Test][build-badge]][build-link]
[![Release][release-badge]][release-link]
[![CodeQL][code-ql-badge]][code-ql-link]
[![Coverage Status][coverage-status-badge]][coverage-status-link]

**I used this [repo][jp-photo-manager-repo] as a base to shape my own PhotoManager with optimizations, new features, bugs fix...
Thank you jpablodrexler for your work :heart:**

## Features :newspaper:

PhotoManager is a desktop application that allows:

- Visualization of image galleries.
- Find duplicates.
- Copy / move images.
- Know if images are corrupted or rotated.
- Import / export images from local folders / shared folders in the local network.
- Delete images in local or shared folders that are not present in source folder.
- Detect videos duplicates.

**It is a local tool, that does not require an Internet connection to work.
Your data stay on your computer and nothing is collected from you.
Even the database is stored in your computer.**

## Upcoming :next_track_button:

- 100% of code coverage.
- Progress bar.
- Multithread.
- Handling corrupted images and videos.
- New logs system.
- Event sourcing.
- Conversion feature (convert heic to jpg for example).
- Migration to MAUI for full compatibity on macOs.

## Run the application :rocket:

Open the PhotoManager\PhotoManager.UI\appsettings.json and [configure it](#config-file-card_file_box).

**Basic usage**: run the .exe file.
**Advanced usage**: open the solution file `PhotoManager\PhotoManager.sln`, set `PhotoManager\PhotoManager.UI\PhotoManager.UI.csproj` as the project to launch and run it.

## Installation instructions :man_teacher:

- Download the zip file with the compiled application files (`publish.zip` or `photo-manager-{version}.zip`) for the latest release.
- Unzip the content of the zip file to a new folder.
- [Configure the appsettings.json file](#config-file-card_file_box)
- Run `PhotoManager.UI.exe`.
- The application saves the cataloged files in the following path: `BackupPath` (Path settings).

## Config file :card_file_box:

If you use the .exe file, you can find a appsettings.json file in the same directory.
You can also find it at `PhotoManager\PhotoManager.UI\appsettings.json`.

The aim is to let you configure it as you need.

**The `Asset` part is about settings of asset:** :framed_picture:

- `AnalyseVideos = false`: Enable it to extract the first frame from each videos, that will be stored in the folder `FirstFrameVideosFolderName` (Path settings).
- `CorruptedMessage = "The asset is corrupted"`: The message to display when the asset is corrupted.
- `RotatedMessage = "The asset has been rotated"`: The message to display when the asset has been rotated from the original.
- `CatalogBatchSize = 100000`: The max amount of pictures to analyse, once the number reached, the analyse will stop.
- `CatalogCooldownMinutes = 2`: The number of minutes before starting a new analysis.
- `CorruptedImageOrientation = 10000`: The default orientation for a corrupted image, while it's corrupted, the tool cannot determine it.
- `DefaultExifOrientation = 1`: The default Exif orientation (0 degree) if the `System.Photo.Orientation` value has not been stored in the metadata of the image.
- `DetectThumbnails = false`: Enable it to detect duplicates between a thumbnail and the original (need `UsingPHash` to be true).
- `SyncAssetsEveryXMinutes = false`: Enable it to sync your assets every X minutes (`CatalogCooldownMinutes`).
- `ThumbnailMaxHeight = 150`: The height of the thumbnail.
- `ThumbnailMaxWidth = 200`: The width of the thumbnail.

**The `Hash` part is about settings of hash (for basic usages, you will not need to modify the settings):** :woman_technologist:

- `PHashThreshold = 10`: The value of the threshold (used when `UsingPHash` is true).
- `UsingDHash = false`: Enable it to hash in DHash (Difference Hash). This hashing method returns "00000000000000" for some pictures.
- `UsingMD5Hash = false`: Enable it to hash in MD5.
- `UsingPHash = false`: Enable it to hash in PHash (Perceptual Hash). It can detect duplicates between rotated assets (improve detection), thumbnails, images part and same images with differents resolutions.
Performances are decreased with PHash by 6 times (for ex: 0.17s for 140 pictures with SHA512 and 1.11s with PHash).

The HashingService works as follows:

- If `UsingPHash` is :heavy_check_mark:, then all the assets will have a PHash as hash type.
- If `UsingPHash` is :x: and `UsingDHash` is :heavy_check_mark:, then all the assets will have a DHash as hash type.
- If `UsingPHash` is :x:, `UsingDHash` is :x: and `UsingMD5Hash` is :heavy_check_mark:, then all the assets will have a MD5Hash as hash type.
- If `UsingPHash`, `UsingDHash` and `UsingMD5Hash` are :x:, then all the assets will have a basic hash type (SHA512).

About `DetectThumbnails`, you will need to set `UsingPHash` to true as well because:
Between Original and Thumbnail:

- PHash the hamming distance is 10/210 (the most accurate).
- DHash the hamming distance is 5/14.
- MD5Hash the hamming distance is 32/32.
- SHA512 the hamming distance is 118/128.

Moreover, there is a parameter that you can adjust following the need, it is `PHashThreshold`.
The max advised is less than 90 (for example 68 can detect false positives).
The default value is `10`, because it can detect a Thumbnail and an original with low quality as duplicates.
But 5 or 6 is often used as a default value in image comparison libraries.
If the hamming distance is lower or equal to the `PHashThreshold` value, then it is a duplicate.
The lower the value of `PHashThreshold`, the more precise it is.

**The `Paths` part is about settings of paths:** :open_file_folder:

- `AssetsDirectory = "the_directory\\to_your_pictures"`: The directory where your assets are, to analyse them.
- `BackupPath = "the_directory\\to_your_local_database"`:  The directory where the database will be stored (The backup must be upper than the location to prevent bugs like "Process is already used" **WIP**).
- `ExemptedFolderPath = "the_directory\\to_your_protected_assets"`: The path where PhotoManager will protect your assets and if there are duplicates in others paths, you will be able to delete all of them except the assets in this exempted path.
- `FirstFrameVideosFolderName = "OutputVideoFirstFrame"`: The folder to save the first frame for each video file (Used if you set `AnalyseVideos` to true), the path will be "`AssetsDirectory` + `\\FirstFrameVideosFolderName`".

**The `Project` part is about settings of project (there is no need to update it):** :building_construction:

- `Name = "PhotoManager`: The name of the tool.
- `Owner = "Toto"`: The name of the owner of the tool.

**The `Storage` part is about settings of storage (update it only for a certain purpose):** :floppy_disk:

- `BackupsToKeep = 2`: The number of backups to keep (the oldest ones are deleted).
- `FoldersName.Blobs = "Blobs"`: The name of the folder to store the blobs (in the `BackupPath`).
- `FoldersName.Tables = "Tables"`: The name of the folder to store the tables (in the `BackupPath`).
- `Separator = "|"`: The separator used to delimit each column (data).
- `StorageVersion = "1.0"`: The version of the local database.
- `Tables.AssetsTableName = "Assets"`: The table's name that stores assets data (a `.db` file in `FoldersName.Tables`).
- `Tables.FoldersTableName = "Folders"`: The table's name that stores folders data (a `.db` file in `FoldersName.Tables`).
- `Tables.RecentTargetPathsTableName = "RecentTargetPaths"`: The table's name that stores recentTargetPaths data (a `.db` file in `FoldersName.Tables`).
- `Tables.SyncAssetsDirectoriesDefinitionsTableName = "SyncAssetsDirectoriesDefinitions"`: The table's name that stores syncAssetsDirectoriesDefinitions data (a `.db` file in `FoldersName.Tables`).
- `ThumbnailsDictionaryEntriesToKeep = 5`: The number of dictionnaries to keep (the key is the path of the current folder and the value is a dictionnary where the key is the asset's name and the value its data).

## Compatible picture formats :camera:

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

## Compatible video formats :video_camera:

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

## Deletion modes :x:

There are three ways to delete duplicates:
The button `Delete` :arrow_right: This will delete the selected duplicate.
The button `DeleteAllButThis` :arrow_right: This will delete all duplicates associated to this asset and keep the selected duplicate.
The button `Delete Every Duplicates Linked To Exempt Folder` :arrow_right: This will delete all duplicates found everywhere that has a duplicate linked in the `ExemptedFolderPath`, but all duplicates in the `ExemptedFolderPath` will be protected.
:memo: Note: The `ExemptedFolder` should not contain any folder inside, all assets must be in the root (otherwise, they will not be exempted).

## How to use it for videos ? :clapper:

PhotoManager is able to detect duplicates between videos.
To do so, you'll need to set `AnalyseVideos` to true and put all of your videos in a single folder.
It will create another folder (**it should not exists**), which is `FirstFrameVideosFolderName`, and will store, inside of it, the first frame for each video (with the name of the video file).
:warning: Be aware that the `FirstFrameVideosFolderName` folder should be deleted before each run to prevent some conflicts. :warning:
Then, you'll be able to see if there are duplicates between videos and to delete, manually, the videos in question.
This feature is only here to help you to identify videos duplicates.
Improvements **WIP**.

## Technologies used :man_technologist:

- [.NET 9.0][dotnet]
- [Windows Presentation Foundation][wpf]
- [NUnit][nunit]
- [Moq framework for .NET (v4.18.4)][moq]
- [MagickImage][magickimage]
- [log4net][log4net]
- [coverlet][coverlet]
- [ReportGenerator][reportgenerator]
- [FFMpegCore][ffmpegcore]

## Code coverage :bar_chart:

[![codecov][codecov-badge]][codecov-link]

## Transparency :handshake:

This project has some versionned dll and rar files for its own good working.
There are 3 rar files, located here: PhotoManager\PhotoManager.Common\Ffmpeg

- **ffmpeg.rar**
- **ffplay.rar**
- **ffprobe.rar**

They are used for the video duplicates detection feature and are here to ensure everyone has the same exact version (even for GitHub CI). That will prevent asset generation differences accross various versions.
To add to that, without them, everyone who want to use it will have to install on their own Ffmpeg and add the path to the .exe file to the env variables. With the rar file in the project, there is no need to do all of this (working only for Windows because these are .exe files in the end **WIP**).

When the project is built for the first time, the three .exe files will be extracted from their rar file (in here: PhotoManager\PhotoManager.Common\Ffmpeg\Bin), done by the **FileExtractionTask.dll**.

The FileExtractionTask.dll is located in here: PhotoManager\PhotoManager.Common\MSBuildTask
Its goal is only to extract the content of a rar file.
It is launched by a MSBuild custom task in here: PhotoManager\PhotoManager.Common\PhotoManager.Common.csproj
And this dll depends on **SharpCompress** library. To avoid the installation of a nuGet just for that, it was better to just add the generated dll of that library.

For the last dll, it is located here: PhotoManager\PhotoManager.Tests\MSBuildTask
It is only used for the well working of the tests, accross each machine.
The **FileDateTask.dll** is used to set a fixed date for every tests files used for integration testing.
It is launched by a MSBuild custom task in here: PhotoManager\PhotoManager.Tests\PhotoManager.Tests.csproj

I've made a specific repo for the two customs dll, injected in the project: [photo-manager-tasks][photo-manager-tasks-link]

[app-icon]: PhotoManager/Images/AppIcon.png

[build-badge]: https://github.com/PABERTHIER/photo-manager/actions/workflows/build.yml/badge.svg
[build-link]: https://github.com/PABERTHIER/photo-manager/actions/workflows/build.yml

[release-badge]: https://github.com/PABERTHIER/photo-manager/actions/workflows/release.yml/badge.svg
[release-link]: https://github.com/PABERTHIER/photo-manager/actions/workflows/release.yml

[code-ql-badge]: https://github.com/PABERTHIER/photo-manager/actions/workflows/codeql-analysis.yml/badge.svg
[code-ql-link]: https://github.com/PABERTHIER/photo-manager/actions/workflows/codeql-analysis.yml

[coverage-status-badge]: https://codecov.io/gh/PABERTHIER/photo-manager/graph/badge.svg?token=DILR0QRXVN
[coverage-status-link]: https://codecov.io/gh/PABERTHIER/photo-manager

[jp-photo-manager-repo]: https://github.com/jpablodrexler/jp-photo-manager

[dotnet]: https://dotnet.microsoft.com/
[wpf]: https://docs.microsoft.com/en-us/dotnet/framework/wpf/
[nunit]: https://nunit.org/
[moq]: https://github.com/moq/moq4
[magickimage]: https://github.com/dlemstra/Magick.NET
[log4net]: https://logging.apache.org/log4net/
[coverlet]: https://github.com/coverlet-coverage/coverlet
[reportgenerator]: https://github.com/danielpalme/ReportGenerator
[ffmpegcore]: https://github.com/rosenbjerg/FFMpegCore

[codecov-badge]: https://codecov.io/github/PABERTHIER/photo-manager/graphs/sunburst.svg?token=DILR0QRXVN
[codecov-link]: https://codecov.io/github/PABERTHIER/photo-manager

[photo-manager-tasks-link]: https://github.com/PABERTHIER/photo-manager-tasks
