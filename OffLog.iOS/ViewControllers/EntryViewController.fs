namespace OffLog

open System
open MonoTouch.UIKit
open MonoTouch.CoreGraphics
open System.Drawing
open MonoTouch.CoreAnimation
open System.Linq
open System.Threading.Tasks
open Feed
open FeedService

[<AllowNullLiteral>]
type EntryCell() as this =
    inherit UITableViewCell(UITableViewCellStyle.Default, EntryCell.Key.ToString())

    let mutable entry = FeedEntry.Zero

    let TitleLabel = new UILabel ( Text = "Name",
                                  Lines = 10,
                                  Font = UIFont.BoldSystemFontOfSize (17.0f),
                                  BackgroundColor = UIColor.Clear,
                                  TextColor = UIColor.Black )
    let CategoriesLabel = new UILabel ( Text = "Size",
                                  Font = UIFont.BoldSystemFontOfSize (12.0f),
                                  BackgroundColor = UIColor.Clear,
                                  TextColor = UIColor.LightGray )
    let AuthorLabel = new UILabel ( Text = "Color",
                                   Font = UIFont.BoldSystemFontOfSize (12.0f),
                                   BackgroundColor = UIColor.Clear,
                                   TextColor = UIColor.LightGray )
    let DateLabel = new UILabel ( Text = "Price",
                                   Font = UIFont.BoldSystemFontOfSize (12.0f),
                                   BackgroundColor = UIColor.Clear,
                                   TextAlignment = UITextAlignment.Right,
                                   TextColor = Color.Blue.ToUIColor() )
    let DescriptionLabel = new UILabel ( Text = "Content",
                                     Lines = 0,
                                     Font = UIFont.SystemFontOfSize(12.0f),
                                     BackgroundColor = UIColor.Clear,
                                     TextColor = Color.Gray.ToUIColor())
    let LineView = new UIView ( BackgroundColor = UIColor.LightGray )

    let leftPadding = 15.0f
    let topPadding = 5.0f
    let fold = String.concat " "

    do this.SelectionStyle <- UITableViewCellSelectionStyle.None
       this.ContentView.BackgroundColor <- UIColor.Clear

       TitleLabel.SizeToFit()
       TitleLabel.Lines <- 10
       TitleLabel.LineBreakMode <- UILineBreakMode.TailTruncation
       this.ContentView.AddSubview TitleLabel

       CategoriesLabel.SizeToFit()
       this.ContentView.Add CategoriesLabel

       AuthorLabel.SizeToFit ()
       this.ContentView.AddSubview (AuthorLabel)

       DateLabel.SizeToFit ()
       this.ContentView.AddSubview(DateLabel)

       DescriptionLabel.SizeToFit()
       this.ContentView.AddSubview(DescriptionLabel)

       this.ContentView.AddSubview (LineView)

    static member val Key = "productCell" with get

    member this.Entry
        with get () = entry
        and set value =
            entry <- value

            TitleLabel.Text <- entry.Title
            DateLabel.Text <- entry.Date.ToShortDateString()
            CategoriesLabel.Text <- entry.Categories |> fold
            AuthorLabel.Text <- entry.Author
            DescriptionLabel.Text <- entry.Description

    override this.LayoutSubviews () =
        base.LayoutSubviews ()

        let bounds = this.ContentView.Bounds

        let mutable x = this.ImageView.Frame.Right + leftPadding
        let mutable y = this.ImageView.Frame.Top
        let labelWidth = bounds.Width - (x + (leftPadding * 2.0f))


        TitleLabel.Frame <- new RectangleF (x, y, labelWidth, 75.f)
        y <- TitleLabel.Frame.Bottom

        CategoriesLabel.Frame <- new RectangleF (x, y, labelWidth, CategoriesLabel.Frame.Height)
        y <- CategoriesLabel.Frame.Bottom

        AuthorLabel.Frame <- new RectangleF (x, y, labelWidth, AuthorLabel.Frame.Height)
        y <- AuthorLabel.Frame.Bottom

        DateLabel.Frame <- new RectangleF (x, y, labelWidth, DateLabel.Frame.Height)
        y <- DateLabel.Frame.Bottom

        DescriptionLabel.Frame <- new RectangleF(x, y, labelWidth, 200.f)
        y <- DescriptionLabel.Frame.Bottom  + topPadding

        LineView.Frame <- new RectangleF (0.0f, this.Bounds.Height - 0.5f, this.Bounds.Width, 0.5f)

type EntryViewTableViewSource(entrySelected: FeedEntry -> unit) =
    inherit UITableViewSource()

    member val Entries = [||] with get,set

    override this.RowsInSection(tableview, section) =
        if this.Entries = [||] then 1 else this.Entries.Length

    override this.RowSelected (tableView, indexPath) =
        if not (this.Entries = [||]) then
            entrySelected (this.Entries.[indexPath.Row])

    override this.GetCell (tableView, indexPath) =
        if this.Entries = [||] then
            new SpinnerCell() :> UITableViewCell
        else
            let cell = match tableView.DequeueReusableCell(EntryCell.Key) with
                        | :? EntryCell as cell when not (cell = null) -> cell
                        | _ -> new EntryCell()

            cell.Entry <- this.Entries.[indexPath.Row]
            cell :> UITableViewCell

type EntryViewController(createStoredFeedsButton, createReloadButton, showEntryDetail) as this =
    inherit UITableViewController()

    let mutable emptyFeedsView:EmptyFeedsView = null

    let source = new EntryViewTableViewSource (showEntryDetail)

    let removeEmptyFeedsView () =
        if emptyFeedsView <> null then
            emptyFeedsView.RemoveFromSuperview ()
            emptyFeedsView <- null

    let CheckEmpty animate =
        emptyFeedsView <- new EmptyFeedsView ( Alpha = if animate then 0.0f else 1.0f)
        if source.Entries.Length = 0 then
            this.View.AddSubview (emptyFeedsView)
            this.View.BringSubviewToFront (emptyFeedsView)
            if animate then
                UIView.Animate (0.25, fun () -> emptyFeedsView.Alpha <- 1.0f)

        else removeEmptyFeedsView()

    let getData () =
        async {
            source.Entries <- [||]
            this.TableView.ReloadData()
            removeEmptyFeedsView()
            let! entries = FeedService.Shared.LoadFeedEntries()
            source.Entries <- entries
            this.TableView.ReloadData()
            CheckEmpty true
        }

    do this.Title <- "OffLog"
       this.NavigationItem.BackBarButtonItem <- new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler = null)
       this.TableView.Source <- source :> UITableViewSource
       this.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
       this.TableView.RowHeight <- 300.0f
       this.TableView.TableFooterView <- new UIView (new RectangleF (0.0f, 0.0f, 0.0f, BottomButtonView.Height))
       this.NavigationItem.RightBarButtonItems <- [|createStoredFeedsButton(); createReloadButton()|]

    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()
        let mutable bound = this.View.Bounds
        bound.Y <- bound.Bottom - BottomButtonView.Height
        bound.Height <- BottomButtonView.Height
        if emptyFeedsView <> null then
            emptyFeedsView.Frame <- this.View.Bounds

    override this.ViewDidAppear _ =
        getData() |> Async.StartImmediate

    member this.Reload() = getData() |> Async.StartImmediate