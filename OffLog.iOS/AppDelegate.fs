namespace OffLog

open System
open System.Linq
open System.Drawing
open System.Collections.Generic

open MonoTouch.UIKit
open MonoTouch.Foundation
open MonoTouch.CoreAnimation
open MonoTouch.CoreGraphics
open Feed

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit UIApplicationDelegate ()

    let mutable window = null
    let mutable navigation:UINavigationController = null
    let mutable myFeeds:MyFeedsButton = null
    let mutable addFeedButton:AddFeedButton = null
    let mutable reloadButton:ReloadButton = null

    let createMyFeedsButton () =
        myFeeds.ItemsCount <- 0
        new UIBarButtonItem(myFeeds)

    let createAddFeedButton () =
        new UIBarButtonItem(addFeedButton)

    let createReloadButton () =
        new UIBarButtonItem(reloadButton)

    let orderCompleted () =
        navigation.PopToRootViewController true |> ignore
    
    let showAddress () =
        let addreesVc = new NewFeedViewController (NewFeedAdded = fun()->())
        navigation.PushViewController (addreesVc, true)

    let showBasket () =
        let basketVc = new FeedsTableViewController(createAddFeedButton, AddItem = showAddress)
        navigation.PushViewController (basketVc, true)
    

    let showEntryDetail (entry: FeedEntry) =
        let productDetails = new EntryDetailViewController(entry)
        navigation.PushViewController (productDetails, true)

    override this.FinishedLaunching (app, options) =
        let buttonRectangle() = new RectangleF(0.0f, 0.0f, 44.0f, 44.0f)

        window <- new UIWindow (UIScreen.MainScreen.Bounds)
        myFeeds <- new MyFeedsButton(Frame = buttonRectangle())
        myFeeds.TouchUpInside.Add(fun _ -> showBasket())
        addFeedButton <- new AddFeedButton(Frame = buttonRectangle())
        addFeedButton.TouchUpInside.Add(fun _ -> showAddress())
        reloadButton <- new ReloadButton(Frame = buttonRectangle())

        UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.LightContent, false)

        UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes (TextColor = UIColor.White))

        let entriesVc = new EntryViewController (createMyFeedsButton, createReloadButton, showEntryDetail)
        reloadButton.TouchUpInside.Add(fun _ -> entriesVc.Reload())
        navigation <- new UINavigationController (entriesVc)

        navigation.NavigationBar.TintColor <- UIColor.White
        navigation.NavigationBar.BarTintColor <- Color.Blue.ToUIColor()

        window.RootViewController <- navigation
        
        window.MakeKeyAndVisible ()
        true

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0

