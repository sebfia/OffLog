namespace OffLog

open System
open MonoTouch.UIKit
open MonoTouch.Foundation
open System.Collections.Generic
open System.Drawing
open System.Linq
open Feed
open Helpers

type NewFeedPageSource() =
    inherit UITableViewSource()

    member val Cells = new List<UITableViewCell> () with get,set

    override this.RowsInSection (tableview, section) =
        this.Cells.Count

    override this.GetCell (tableView, indexPath) =
        this.Cells.[indexPath.Row]

    override this.GetHeightForRow (tableView, indexPath) =
        this.Cells.[indexPath.Row].Frame.Height

    override this.RowSelected (tableView, indexPath) =
        match this.Cells.[indexPath.Row] with
        | _ -> ()

        tableView.DeselectRow (indexPath, true)

type NewFeedViewController() as this =
    inherit UITableViewController()

    let Cells = new List<UITableViewCell>()

    let BottomView = new BottomButtonView ( ButtonText = "Add Feed",
                                            ButtonTapped = fun _ -> this.PlaceOrder() |> Async.StartImmediate)

    let UrlField = new TextEntryView (PlaceHolder = "Rss Feed Url", 
                                            Value = "",
                                            AutocapitalizationType = UITextAutocapitalizationType.None,
                                            KeyboardType = UIKeyboardType.Url,
                                            ReturnKeyType = UIReturnKeyType.Go)

    let NameField = new TextEntryView ( PlaceHolder = "(Optional) Feed Name", 
                                            Value = "",
                                            AutocapitalizationType = UITextAutocapitalizationType.Words)

    do 
       this.Title <- "Add a new feed"
       this.NavigationItem.BackBarButtonItem <- new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler = null)
       this.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None

       Cells.Add (new CustomViewCell (NameField ))
       Cells.Add (new CustomViewCell (UrlField ))

       this.TableView.Source <- new NewFeedPageSource ( Cells = Cells )
       this.TableView.TableFooterView <- new UIView(new RectangleF (0.0f, 0.0f, 0.0f, BottomButtonView.Height))
       this.TableView.ReloadData()

       this.View.AddSubview BottomView

    member val NewFeedAdded = fun ()->() with get,set

    member this.PlaceOrder() = async {
        let feed = { Name = NameField.Value; Url = UrlField.Value }
        let! isValid = FeedService.Shared.ValidateFeed feed
        match isValid with
        | Success msg -> 
            FeedService.Shared.NewFeed <- msg
            do! FeedService.Shared.AddNewFeed(msg)
            this.NavigationController.PopToRootViewController(true) |> ignore
            this.NewFeedAdded()
        | Failure msg -> (new UIAlertView("Error", msg, null, "Ok")).Show() }
            
    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()

        let mutable bound = this.View.Bounds
        bound.Y <- bound.Bottom - BottomButtonView.Height
        bound.Height <- BottomButtonView.Height
        BottomView.Frame <- bound