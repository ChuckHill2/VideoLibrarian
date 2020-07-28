# A Movie Guide for All Your Movies

This is a standalone Windows app that will automatically download information for all
the movies in your movie folders and present it to you in a searchable fashion
in order to select a movie to watch. This app is designed to be viewed on your
TV via an HDMI connection to your laptop.

## Prerequisites

MovieGuide uses .NET 4.5. If you are using a version of Windows less than
Windows 8, then you must first install .NET 4.5. If you attempt to run this
utility and .NET 4.5 is NOT installed, this application will fail to run.

You may download and install .NET 4.5 from:

<http://www.microsoft.com/en-us/download/details.aspx?id=30653>

## Installation

Copy MovieGuide.exe to any new or existing writeable directory. That's it!

Upon execution, MovieGuide.exe will create additional files in the same
directory as the executable.

-   **MovieGuide.log** – Text file contains status messages. It may be
    informational, warnings, and errors.
-   **MovieGuide.SavedState.xml** – This file contains all the current configuration
    values.
-   **NReco.VideoInfo.dll** – Used to extract information from the video file.
-   **ffprobe.exe** – Used to extract information from the video file.

## Movie Folder Setup

Within your root movie folder, each movie/episode video must be in a separate
folder with a matching IMDB movie shortcut (.url, .website). A TV series root
folder contains only the main IMDB TV series shortcut as it just refers to all
the episodes. The folders may be arranged in any way you see fit so long as the
full directory path (not filenames) does not exceed 245 characters. Any
additional files in these folders are ignored. Upon startup, this application
will add additional cache files to these folders.

As an example, your folders may be arranged as follows:
<pre>
moviesRoot
├── movie1
│   ├── anyname.mp4 – The movie file. May be any video format.
│   └── anyname.url – The IMDB movie shortcut (more below).
├── movie2
├── movieN
├── tvSeries1
│   ├── anyname.url – The IMDB movie shortcut (more below).
│   ├── episode1
│   │   ├── anyname.mp4 – The movie file. May be any video format.
│   │   └── anyname.url – The IMDB movie shortcut (more below).
│   ├── episode2
│   └── episodeN
├── tvSeries2
└── tvSeriesN
</pre>

**anyname.mp4** refers to the sole movie file in this directory. It may be in 
any video format (e.g. asf, avi, flv, mkv, mov, mp4, mpg, wmv, etc.). Note that 
the TV series root folders do not contain a movie file, just the url.<br />
**anyname.url** refers to an IMDB shortcut file to the IMDB page of the 
associated movie.

The full directory path (not including movie filename) must not exceed 245
characters. Keep your folder names small if you have many directory levels. This
is a Windows limitation.

Each movie or TV series folder must contain zero or one movies and exactly one
IMDB movie shortcut (.url, .website). All the other needed files will be
automatically generated upon demand. Any folders that do not contain a valid
shortcut (.url, .website) will be ignored. See File-\>Status Log for details.

### Generated Cache Files
These cache files reside in the same folders as the shortcut files. They are 
unique to each shortcut.
- tt1234567.htm – downloaded IMDB web page.
- tt1234567.jpg – downloaded movie poster.
- tt1234567.xml – extracted movie information.
- tt1234567S.png – cached UI movie information tile.
- tt1234567M.png – cached UI movie information tile.
- tt1234567L.png – cached UI movie information tile.

If any of these files are deleted, they will be recreated.

### Retrieving IMDB Movie Shortcut Link

Go to <https://www.imdb.com/find?ref_=nv_sr_fn&q=&s=tt>

In the web page search box, enter the name of the movie. The results may find
more than one entry. Verify by opening the relevant page. Click and drag the
link from the Chrome or IE address bar to the folder with the matching movie.

### First Time MovieGuide Startup

When starting MovieGuide for the first time, the root folder(s) containing the
movies has not yet been set in MovieGuide.

Open the File-\>Settings dialog to enter your root movie folders (you may have
more than one).

When OK is pressed, the movie information will start to be downloaded from the
internet. Depending on the number of movies you have, this may take from minutes
to hours. *Be patient*. This cached information for each movie is stored in the
same folder as the movie. These files are named tt0000000.xxx. If these files
are deleted, they will automatically be regenerated. Once these cached files
have been generated, startup will be a whole lot faster. This is the only time
the internet is accessed.

Review File-\>Status Log or the MovieGuide.log for any possible errors in file
generation.

### Movie Tile View Modes

Each UI movie information item is called a ‘tile’. These tiles just fill the
window, wrapping from left to right.

The tiles come in 3 sizes, small, medium, and large. ‘Large’ displays all the
information available, and ‘Small’ contains a subset because all the information
will not fit. However many more small tiles will fit on a single page than large
tiles. That is the tradeoff.

### Changing the Default Movie Poster Image

When this application gathers the information from the url shortcut you
specified, it downloads the first poster image it finds. This image may not be
the best poster image. The IMDB movie page actually has many poster images (some
in foreign languages). You can manually download a poster image from IMDB or
elsewhere and replace the one that MovieGuide downloaded. If you do, you must
delete the tt\*.png cache files so they can be regenerated with your new image
embedded. Also for backup, you should also update the poster url in the XML
file. The ideal image ratio is 250x365 pixels. Larger images are better so they
can be scaled without any loss of image quality.

### Changing/Correcting Movie Information

The movie information is all stored in the xml file, tt1234567.xml. Most of the
elements are descriptive and can be changed as necessary.

Special notes:

If element EpisodeCount is greater than zero, the movie information refers to a
TV series. A movie in this folder is ignored. Child episode subfolders contain
the video files.

If element Season is greater than zero, the movie information refers to a TV
series episode.

Element Episode may be any integer but must be unique within the series.

The episode ‘MovieName’ field consists of the series name and episode name. The
two names are delimited by Unicode dash (" \\xAD ", with spaces), NOT a regular
ansi dash ‘-‘. They look alike. If necessary, just copy the ‘dash’ from another
episode. This was done to distinguish between regular dashes in the movie or
episode names.

If you make any changes, you must delete the tt\*.png cache files so these files
can be regenerated with your new changes.

### Features

-   Multiple root media folders may be defined. Available through menu
    File-\>Settings…

-   Automatically downloads missing movie information from the internet and caches 
    it for faster startup.

-   Error/status text log. Available through menu File-\>Status Log…

-   3 different movie info item (aka UI ‘tile’) sizes. Available through menu
    View.

-   Sort tiles by multiple properties.

-   Filter tiles by multiple properties.

-   Maximum supported movies is 3200. Additional movies are ignored. See log for
    details. Note that each entire TV-Series count as one ‘movie’.

-   Scrolling is supported by clicking and dragging on the scrollbar, mouse
    wheel, and keyboard arrow keys, Home, End, PageUp, PageDown and Alt-arrow
    keys. Arrow keys scroll by 1/10 height (or width) of the current tile.
    Alt-arrow keys scroll by 1 pixel.

-   Clicking on title opens the movie in the default video player.

-   Clicking on the description (or small tile body), opens the full summary in
    a popup window.

-   Clicking on the location opens containing folder in Windows Explorer.

-   Clicking on the “IMDB” icon, opens the IMDB movie page in the default
    browser.

-   Clicking on the “Watched” checkbox, marks the movie as ‘previously viewed’
    and also shows date viewed. One can also filter and sort on this property.

-   Clicking on the poster will blow up the tile so it fits the full screen
    (Large tiles only). Click anywhere on the full screen image exits the
    fullscreen mode. Useful when sitting on the couch.

### Recommended Tools

These are 3rd party tools that enhance the video management experience. They are
not required, but they are sure helpful.

#### Video Player

In short, any video player will do. However, if you want a better viewing
experience, VideoLAN VLC video player supports all video formats and is open
source freeware actively supported by the VideoLAN community. See:
<https://www.videolan.org/>

#### Windows Explorer Video Properties Extension

When it comes to video properties, Windows Explorer only supports a handful of
video formats. Most notably, it *doesn’t*  support the popular mkv format!
Icaros Shell Extensions (freeware) will support nearly all video and audio
formats not supported by Windows Explorer. See:
<https://www.videohelp.com/software/Icaros>

#### Video Conversion

Invariably, movie videos are extremely large. With the right tool, one can
compress a movie by 50% without any noticeable differences. There are many video
conversion tools out there. Good (and not so good) tools cost money, but the
best one that does not require you to be a video expert is the Divx Video
Converter (freeware). It may be found at
<https://www.divx.com/en/software/divx>. It includes a video player but it is
not as flexible or comprehensive as the VLC video player.

If you run into conversion problems, K-Lite Codec Pack contains the most 
comprehensive set of codecs http://www.codecguide.com/download_kl.htm 

#### Video Properties Viewer

The popular MediaInfo viewer displays *all* the info/properties in any video or
audio file. See: <https://mediaarea.net/en/MediaInfo>.

#### MKV Properties Editor

MKVToolNix allows one to add/remove/edit properties and components of a video
and save it as an MKV format. It does not perform any video conversion. Most
other video formats do not have editable properties. That’s why the MKV video
container format is so popular. See: <https://mkvtoolnix.download>.

### Developer

With the following excepions, no 3rd-party source code was used. It was entirely 
hand-crafted and optimized. It was developed entirely within .Net Framework 4.5 
Forms on Visual Studo 2019.

#### 3rd Party Code

<https://www.codeproject.com/Articles/624997/Enhanced-Scrollbar>

This is used (and slightly modified) because MovieGuide is a gallery of tiles
that vastly exceeds the maximum size of the default virtual window. As a result,
the default windows scroll bars are woefully inadequate.

<https://www.nuget.org/packages/NReco.VideoInfo/>

This is used to extract media info directly from the video files. It is only 
used during movie information download.
