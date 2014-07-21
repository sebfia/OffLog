namespace OffLog

open System
open System.Collections.Generic
open System.Linq
open MonoTouch.Foundation
open MonoTouch.UIKit
open System.Drawing

open System.Threading.Tasks
open MonoTouch.CoreGraphics
open MonoTouch.CoreAnimation
open Helpers
open Feed

type EntryDetailViewController(entry:FeedEntry) as this =
    inherit UIViewController()
        
    let mutable webView:UIWebView = null
    let mutable content = ""

    do 
        this.Title <- entry.Title
        content <- entry.Content

    override this.ViewWillAppear animated =
        base.ViewWillAppear (animated)


    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()
        webView <- new UIWebView(Frame = this.View.Bounds)
        webView.LoadHtmlString(content, null)
        this.View.AddSubview(webView)

