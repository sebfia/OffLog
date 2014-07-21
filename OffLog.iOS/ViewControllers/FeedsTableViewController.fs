namespace OffLog

open System
open MonoTouch.UIKit
open System.Collections.Generic
open System.Linq

type FeedsPageSource(parent:FeedsTableViewController, filteredItems) =
    inherit UITableViewSource()

    member val FilteredItems = filteredItems with get,set
    member val Parent:FeedsTableViewController = parent with get,set
    member val FeedDeleted = fun _ -> () with get, set

    override this.RowsInSection (tableview, section) =
        if this.FilteredItems = null then 
            0
        else 
            let items:List<string> = this.FilteredItems
            items.Count

    override this.GetCell (tableView, indexPath) =
        let cell = tableView.DequeueReusableCell ("stringCell") 
        let cell = if cell <> null then cell else new UITableViewCell(UITableViewCellStyle.Default, "stringCell")
        if this.FilteredItems <> null then
            let items:List<string> = this.FilteredItems
            cell.TextLabel.Text <- items.ElementAt indexPath.Row
        cell

    override this.RowSelected (tableView, indexPath) =
        if this.Parent <> null then
            let items:List<string> = this.FilteredItems
            let item = items.ElementAt indexPath.Row
            this.Parent.ItemSelected item
            this.Parent.NavigationController.PopViewControllerAnimated true |> ignore

    override this.EditingStyleForRow (tableView, indexPath) =
        UITableViewCellEditingStyle.Delete

    override this.CommitEditingStyle (tableView, editingStyle, indexPath) =
        if editingStyle = UITableViewCellEditingStyle.Delete then
            let items:List<string> = this.FilteredItems
            let item = items.ElementAt indexPath.Row
            this.FeedDeleted item
            this.FilteredItems.Remove(item) |> ignore
            tableView.DeleteRows([|indexPath|], withRowAnimation = UITableViewRowAnimation.Fade)

and [<AllowNullLiteralAttribute>]FeedsTableViewController(createAddFeedButton) as this =
    inherit UITableViewController()

    let searchBar = new UISearchBar ()
    let mutable items = new List<string>()
    let mutable filteredItems = new List<string>()
    let source = new FeedsPageSource (this, filteredItems)

    let toNetList lst =
        let l = new List<_>()
        lst |> List.iter (fun i -> l.Add(i))
        l
    let getData () =
        async {
            let! feeds = FeedService.Shared.GetFeeds()
            let projected = feeds |> List.map (fun i -> i.Name) |> toNetList
            this.Items <- projected
        }

    do this.TableView.Source <- source
       searchBar.TextChanged.Add(fun _ -> this.FilteredItems <- items.Where(fun (x:string) -> x.IndexOf(searchBar.Text, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList()
                                          this.TableView.ReloadData() )
       searchBar.SizeToFit ()
       this.TableView.TableHeaderView <- searchBar
       this.Title <- "My Rss Feeds"
       source.FeedDeleted <- fun n -> FeedService.Shared.RemoveFeedWithName(n) |> Async.StartImmediate

    member this.FilteredItems
        with get () = filteredItems
        and set value = source.FilteredItems <- value
                        filteredItems <- value

    member val ItemSelected = fun (x:string) -> () with get,set
    member val AddItem = fun () -> System.Diagnostics.Debug.WriteLine("Adding Item") with get,set

    member this.Items
        with get () = items
        and set value = items <- value
                        this.FilteredItems <- items
                        this.TableView.ReloadData ()

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated
        source.Parent <- this
        this.NavigationItem.RightBarButtonItem <- createAddFeedButton()

    override this.ViewDidAppear animated =
        base.ViewDidAppear animated
        searchBar.BecomeFirstResponder () |> ignore
        getData() |> Async.StartImmediate 

    override this.ViewDidDisappear animated =
        base.ViewDidDisappear animated
        source.Parent <- null

