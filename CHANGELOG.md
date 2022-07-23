# Change History
#### 8/27/2018
* Created

#### 12/29/2018
* Download date not set for TV Series.
* When defined, TV Series end year always set to start year.
* Release date not always set to proper date.
* If 2 different TV series have the exact same name (but different years), the child episodes will be mixed.
* Properly supports Up/Down, Home/End, and PageUp/PageDown keys.
* Moving to another window and back always scrolls up to the first tile.
* Fixed mysterious white stray pixels in poster image.
* Log now not overwritten upon every startup. Reset to empty only after 100MB in size.
* Log can now be opened into your default text editor from within the application under menu File->Status Log…
* Handle null reference exception when movie properties cannot be downloaded from internet. (See Log for details).
* The ‘Watched’ checkbox flag on the large tiles was never saved. Now it is.
* Replaced ‘Watched’ checkbox with a better graphical version.
* Replaced the IMDB icon with the latest official images.
* Maximum number movies supported is 1950 movies. Any more than that will be ignored.
* Clicking on the large tile poster image will maximize the entire tile to full screen. Clicking or pressing any key or moving the mouse wheel will close the full screen window.
* Clicking on any tile description will pop up a more detailed description.
* Scaled up Settings, Filter, and Sort dialogs to be more readable.

#### 4/20/2019
* Failed to download movie poster JPGs. Fixed by hardcoding SecurityProtocol to Tls for images only.

#### 7/6/2019
* Changed ‘Watched’ flag to the date the flag was changed. Now you know when you watched the movie.

#### 7/21/2019
* Now supports up to 3200 movies (entire TV series count as one ‘movie’). Any more than that will be ignored. See File->Log Status for details.

#### 11/23/2019
* Fixed location directory path from disappearing when other windows temporarily overlay the location field.
* Shrink width of location field to fit just the directory path. Not the entire width of tile as previously.
* Trimmed odd ‘See full summary’ at end of some movie descriptions.

#### 12/21/2019
* Handles full directory paths up to 245 characters. Movie file names may be up to 255 characters. URL shortcut full path name must still be less than 260 characters (windows limitation).

#### 03/22/2020
* Support both Chrome and IE web shortcuts.
* Allow multiple shortcuts in video folder so long as only one is an IMDB movie link.
* Disallow duplicate producer, writer, and actor names.
* Remove spurious trailing “1 more credit” on above names.

#### 7/4/2020
* Move to local GIT
* Fixed loosing watched state when recreating cached tile image.

#### 7/25/2020
* Upon IMDB scrape, the poster image retrieved was the IMDB logo not the movie poster. IMDB html changed slightly. Fixed. 

#### 8/14/2020
* Poster image URL missing when movie properties are regenerated and image already exists.

#### 9/18/2020
* Added helper utility VideoOrganizer.exe to create/configure video folder layout and create requisite IMDB movie shortcuts. See VideoOrganizer.exe About box for usage.

#### 10/02/2020
* Added ability to filter on substrings (movie name, plot, or crew).

#### 10/11/2020
* Fixed bug in filtering Rating where rating==0
* Fixed parsing IMDB movie names (didn't support '()' in movie names)

#### Version 2.3
* Fixed Summary popup bottom did not fit within the screen dimensions. 
* Truncate plot strings to fit tile fields
* Added logging severity
* Added verbose-level download performance metrics.
* Fixed sorting by multiple keys.
* MouseWheel was going in the wrong direction relative to other apps.

#### Version 2.4
* Added video validator console app to verify that videos files are not changed or corrupted.
* Added folder exclusion pattern to block movie search thru excluded folders. 
* Recover from missing poster when source IMDB page has not yet been completely updated. Mainly occurs with TV episodes.
* Fixed IMDB web page scraper because of major IMDB design update on 06-04-2021.
* Increased MouseWheel click to 2 line scroll events for faster scrolling.
* Outstanding Bug - Rare crash due to multi-threaded access to an image. 
* Enabled fatal error message box rather than have app just mysteriously disappear. Log shows details.

#### Version 2.4.1
###### <small>*VideoLibrarian*</small>
* IMDB changed their web page layout once again so we needed to update the web scraper. 
  This will no doubt be a continuous wack-a-mole issue.
* Fixed bug in restoring window size and position.
* Better handle http download redirect by updating shortcut as necessary.
###### <small>*VideoOrganizer*</small>
* Added support for parsing another video file name format.

#### Version 2.5.0

* Update .NET Framework from 4.5 to .NET Framework 4.8 (latest)
* Added UpdateXml console tool to refresh all the stored movie properties.
* Optimized directory enumeration for speed.
* Optimize many file I/O methods for speed.
* Replaced mini popup MessageBox.
* Perform Filtering and MovieProperty search asynchronously.
* Added poster download during new MovieProperties creation for asynchronous download.
* VideoLibrarian.Settings: Fix failure to update when adding/removing a media folder.
* Fix logging to support asynchronous usage.
* Restrict app to run only one instance.
* Refactored http web page downloading.
* Small tiles: Fixed clicking on TvSeries title opened TvEpisodes AND opened SummaryPopup. 

#### Version 2.6
* Cache 30+ compiled regex patterns for speed.
* Better handling of missing poster images.
* Support scroll position in TV Series view.
* Set logging severity levels in Settings dialog.
* Fixed bug where TV Series episode that has no episode name, orphans subsequent episodes.
* Movie description web scrape update.
* Added custom user-defined attributes for filtering.
* Added support to VideoOrganizer/manual editing for adding user-defined attributes.


### Notes
#### Not a bug
At runtime, if there are 2 identical TV Series in 2 different root folders, all episodes
will be merged under single TV Series tile with the other one containing no episodes
thus no child episodes to expand into.
This does not occur with movies. In this case an extra instance tile of the same movie
title will be shown.
