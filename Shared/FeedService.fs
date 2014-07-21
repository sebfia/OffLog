module FeedService

open System
open System.Linq
open Feed
open Helpers
open ModernHttpClient
open System.Net.Http

    type Rss = FSharp.Data.XmlProvider<"Sample.xml">

    type FeedService(saveLocation) =

        let combine = fun l r -> System.IO.Path.Combine(l,r)
        let feedsPath = saveLocation |> combine "Feeds.db"
        let save = FeedStorage.freeze feedsPath
        let client = new HttpClient(new NativeMessageHandler())
        let getAsync (url:string) = client.GetAsync(url) |> Async.AwaitTask
        let asStringAsync (content:HttpContent) = content.ReadAsStringAsync() |> Async.AwaitTask
        let getRss feed =
            async {
                use! response = getAsync feed.Url
                match response.StatusCode with
                | Net.HttpStatusCode.OK -> 
                    let! str = asStringAsync response.Content
                    return Rss.Parse(str) |> Some
                | _ -> return None
            }
        let loadFeed feed =
            async {
                try
                    let! response = getRss feed
                    match response with
                    | None -> return []
                    | Some rss ->
                        return rss.Channel.Items |> Seq.map (fun i -> { Feed = feed; Categories = i.Categories; Title = i.Title; Author = i.Creator; Description = i.Description; Content = i.Encoded; Date = i.PubDate}) |> Seq.toList
                with | e -> return []
            }

        member this.ValidateFeed feed =
            async {
                try
                    let! response = getRss feed
                    match response with
                    | None -> return Failure "Unable to open this Rss-Feed!"
                    | Some rss ->
                        let name = if String.IsNullOrWhiteSpace feed.Name then rss.Channel.Title else feed.Name
                        return { Name = name; Url = feed.Url } |> Success
                with | e -> return Failure "Unable to open this Rss-Feed!"
            }

        member val NewFeed = Feed.Zero with get,set

        member this.GetFeeds() =
            async {
                let! result = FeedStorage.thaw<Feed list> feedsPath
                match result with
                | None -> return [Feed.Xamarin]
                | Some feeds -> return feeds |> List.sortBy (fun f -> f.Name)
            }

        member this.AddNewFeed feed = 
            async {
                let! result = FeedStorage.thaw<Feed list> feedsPath
                match result with
                | None -> return! save [feed]
                | Some feeds -> return! feed::feeds |> save
            }

        member this.RemoveFeedWithName name =
            async {
                let! result = FeedStorage.thaw<Feed list> feedsPath
                match result with
                | None -> return ()
                | Some feeds -> return! feeds |> List.filter (fun f -> f.Name <> name) |> save
            }

        member this.LoadFeedEntries () : Async<FeedEntry array> =
            async {
                let! result = FeedStorage.thaw<Feed list> feedsPath
                match result with
                | None ->
                    let! entries = loadFeed Feed.Xamarin
                    return entries |> fun e -> e.OrderByDescending(fun k -> k.Date) |> Seq.toArray
                | Some feeds ->
                    let! entries = feeds |> Seq.map (fun f -> loadFeed f) |> Async.Parallel
                    return  entries |> Seq.collect (fun a -> a) |> fun e -> e.OrderByDescending (fun k -> k.Date) |> Seq.toArray
            }
    
    let Shared = new FeedService(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Data"))