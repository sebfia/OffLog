module Feed

open System.Runtime.Serialization
open Newtonsoft.Json

    [<CLIMutable; DataContract>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type Feed =
        {
            Name: string;
            Url: string;
        }
        with static member Zero = { Name = ""; Url = ""}
             static member Xamarin = {Name="Xamarin"; Url="http://blog.xamarin.com/feed"}

    type FeedEntry =
        {
            Feed: Feed;
            Categories: string array;
            Author: string;
            Title: string;
            Date: System.DateTime;
            Description: string;
            Content: string;
        }
        with
            static member Zero = { Feed= Feed.Zero; Categories = [||]; Author = ""; Title = ""; Date = System.DateTime.Today; Description = ""; Content = ""}