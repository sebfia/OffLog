OffLog
======

An Rss reader for iOS written completely in FSharp with Xamarin utilizing the FSharp.Data.XmlProvider.

This app allows to view and add/remove rss-feeds.

A few points to note here:

1) The implementation showcases how well FSharp.Data type providers work with Xamarin on the iOS platform and how few code is needed with F# to create a full blown app.

2) In order to make FSharp type providers work I had to add the NUget package: FSharp.Core.Mono.Signed (prerelease).

3) Using the awesome ModernHttpClient, which is allso available as Xamarin component nearly tripled the download speed.

4) I gained a lot of dev speed by using the Xamarin T-Shirt store app as a template and shamelessly copied some awesome controls and images from there.

5) As new files are always added behind all previous files in Xamarin Studio I recommend the following pattern to add new fs source files: Add the new file via the command line (touch /path/to/new/file) and then edit your fsproj with a text editor of your choice (sublime, atom, emacs...) and add the file to the right place.

6) As this app was hacked out quickly this morning I did not strive for a pretty design in the first place. A prettier design will be added together with an Android version and improved offline capabilities if I win the Xamarin F# app contest! :-)
